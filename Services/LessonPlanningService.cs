using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RateMyTeacher.Data;
using RateMyTeacher.Models;
using RateMyTeacher.Models.Enums;
using RateMyTeacher.Models.Lessons;
using RateMyTeacher.Services.Models;

namespace RateMyTeacher.Services;

public class LessonPlanningService : GeminiService, ILessonPlanningService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LessonPlanningService> _logger;

    public LessonPlanningService(
        ApplicationDbContext dbContext,
        IAIUsageService aiUsageService,
        ILogger<GeminiService> geminiLogger,
        ILogger<LessonPlanningService> logger)
        : base(aiUsageService, geminiLogger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<LessonPlanDisplayModel> GenerateAsync(LessonPlanRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureTeacherExistsAsync(request.TeacherId, cancellationToken);
        var governance = await GetTeacherAiStateAsync(cancellationToken);
        if (!governance.isEnabled || governance.mode == TeacherAiMode.Off)
        {
            throw new InvalidOperationException("AI lesson planning is currently disabled for teachers.");
        }

        var sections = BuildPlanSections(request, governance.mode);
        var resources = BuildResources(request);

        var entity = new LessonPlan
        {
            TeacherId = request.TeacherId,
            Subject = request.Subject.Trim(),
            GradeLevel = request.GradeLevel.Trim(),
            TopicFocus = request.TopicFocus.Trim(),
            StudentNeeds = string.IsNullOrWhiteSpace(request.StudentNeeds) ? null : request.StudentNeeds.Trim(),
            DurationMinutes = request.DurationMinutes,
            SectionsJson = JsonSerializer.Serialize(sections, SerializerOptions),
            ResourcesJson = JsonSerializer.Serialize(resources, SerializerOptions),
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.LessonPlans.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await RecordUsageAsync(
            new AiUsageLogRequest(
                request.RequestedByUserId,
                BuildPromptPreview(request),
                FormatPlanForLogging(sections, resources),
                ResolveInteractionMode(governance.mode)),
            cancellationToken);

        return MapToDisplayModel(entity, sections, resources);
    }

    public async Task<IReadOnlyList<LessonPlanDisplayModel>> GetHistoryAsync(int teacherId, int take, CancellationToken cancellationToken = default)
    {
        await EnsureTeacherExistsAsync(teacherId, cancellationToken);
        var plans = await _dbContext.LessonPlans
            .AsNoTracking()
            .Where(plan => plan.TeacherId == teacherId)
            .OrderByDescending(plan => plan.CreatedAt)
            .Take(Math.Clamp(take, 1, 25))
            .ToListAsync(cancellationToken);

        return plans.Select(MapToDisplayModel).ToList();
    }

    private async Task EnsureTeacherExistsAsync(int teacherId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Teachers.AnyAsync(t => t.Id == teacherId, cancellationToken);
        if (!exists)
        {
            throw new ArgumentException("Teacher not found.", nameof(teacherId));
        }
    }

    private async Task<(bool isEnabled, TeacherAiMode mode)> GetTeacherAiStateAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.SystemSettings
            .AsNoTracking()
            .Where(s => s.Key == SystemSetting.Keys.Ai.GlobalEnabled || s.Key == SystemSetting.Keys.Ai.TeacherMode)
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        var enabled = settings.TryGetValue(SystemSetting.Keys.Ai.GlobalEnabled, out var enabledValue) && bool.TryParse(enabledValue, out var parsedEnabled)
            ? parsedEnabled
            : true;
        var mode = settings.TryGetValue(SystemSetting.Keys.Ai.TeacherMode, out var modeValue) && Enum.TryParse<TeacherAiMode>(modeValue, out var parsedMode)
            ? parsedMode
            : TeacherAiMode.Guided;

        return (enabled, mode);
    }

    private static LessonPlanDisplayModel MapToDisplayModel(LessonPlan entity)
    {
        var sections = JsonSerializer.Deserialize<IReadOnlyList<LessonPlanSection>>(entity.SectionsJson, SerializerOptions)
            ?? Array.Empty<LessonPlanSection>();
        var resources = JsonSerializer.Deserialize<IReadOnlyList<string>>(entity.ResourcesJson, SerializerOptions)
            ?? Array.Empty<string>();
        return MapToDisplayModel(entity, sections, resources);
    }

    private static LessonPlanDisplayModel MapToDisplayModel(LessonPlan entity, IReadOnlyList<LessonPlanSection> sections, IReadOnlyList<string> resources)
    {
        return new LessonPlanDisplayModel(
            entity.Id,
            entity.CreatedAt,
            entity.Subject,
            entity.GradeLevel,
            entity.TopicFocus,
            entity.StudentNeeds,
            entity.DurationMinutes,
            sections,
            resources);
    }

    private static IReadOnlyList<LessonPlanSection> BuildPlanSections(LessonPlanRequest request, TeacherAiMode mode)
    {
        var keywords = LessonTextHelper.ExtractKeywords(request.TopicFocus, 4);
        var focus = keywords.FirstOrDefault() ?? LessonTextHelper.NormalizeWhitespace(request.TopicFocus);
        var sections = new List<LessonPlanSection>
        {
            new("Engage", new[]
            {
                $"Quick entry ticket: ask students to jot down what they already know about {focus}.",
                mode == TeacherAiMode.Guided
                    ? $"Facilitate a short discussion to surface misconceptions around {focus}."
                    : $"Show a real-world photo or prompt connected to {focus} and invite rapid observations."
            }),
            new("Explore", new[]
            {
                $"Model a worked example that spotlights the critical steps for {focus}.",
                $"Provide a guided practice problem, then release pairs to attempt a similar task.",
                request.DurationMinutes.HasValue && request.DurationMinutes < 45
                    ? "Keep this block tight—aim for two concise cycles of modeling and student tries."
                    : "Include one extension question for early finishers who need extra challenge."
            }),
            new("Apply", new[]
            {
                $"Students create a quick artifact (exit ticket, mini-poster, or audio note) demonstrating their grasp of {focus}.",
                mode == TeacherAiMode.Unrestricted
                    ? "Offer immediate feedback with exemplar answers accessible in the LMS."
                    : "Use guiding questions to prompt deeper explanations before revealing solutions."
            }),
            new("Reflect", new[]
            {
                "Capture lingering questions in a shared doc so tomorrow's warm-up can target them.",
                string.IsNullOrWhiteSpace(request.StudentNeeds)
                    ? "Invite students to self-assess confidence with a thumbs rating or quick poll."
                    : $"Address the noted learner needs ({request.StudentNeeds}) by planning a specific follow-up action."
            })
        };

        return sections;
    }

    private static IReadOnlyList<string> BuildResources(LessonPlanRequest request)
    {
        var focus = LessonTextHelper.ExtractKeywords(request.TopicFocus, 3);
        var top = focus.FirstOrDefault() ?? request.TopicFocus;
        return new List<string>
        {
            $"One-page reference sheet summarizing the process for {top}.",
            $"Short video or simulation introducing {request.Subject} at the {request.GradeLevel} level.",
            "Three differentiated practice questions (core, stretch, and challenge) ready for independent work.",
            "Exit ticket template in Google Forms or Microsoft Forms to capture evidence quickly."
        };
    }

    private static string BuildPromptPreview(LessonPlanRequest request)
    {
        return $"Subject:{request.Subject}; Grade:{request.GradeLevel}; Topic:{request.TopicFocus}; Needs:{request.StudentNeeds}";
    }

    private static string FormatPlanForLogging(IReadOnlyList<LessonPlanSection> sections, IReadOnlyList<string> resources)
    {
        return string.Join(" | ", sections.Select(section => $"{section.Title}:{string.Join(';', section.Steps)}")) +
               " | Resources:" + string.Join(';', resources);
    }

    private static AiInteractionMode ResolveInteractionMode(TeacherAiMode mode) => mode switch
    {
        TeacherAiMode.Unrestricted => AiInteractionMode.ShowAnswer,
        TeacherAiMode.Guided => AiInteractionMode.Guide,
        _ => AiInteractionMode.Explain
    };
}
