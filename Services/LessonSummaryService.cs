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

public class LessonSummaryService : GeminiService, ILessonSummaryService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<LessonSummaryService> _logger;

    public LessonSummaryService(
        ApplicationDbContext dbContext,
        IAIUsageService aiUsageService,
        ILogger<GeminiService> geminiLogger,
        ILogger<LessonSummaryService> logger)
        : base(aiUsageService, geminiLogger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<LessonSummaryDisplayModel> GenerateAsync(int teacherId, int requestedByUserId, string lessonNotes, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(lessonNotes))
        {
            throw new ArgumentException("Lesson notes are required", nameof(lessonNotes));
        }

        await EnsureTeacherExistsAsync(teacherId, cancellationToken);
        var governance = await GetTeacherAiStateAsync(cancellationToken);
        if (!governance.isEnabled || governance.mode == TeacherAiMode.Off)
        {
            throw new InvalidOperationException("AI summaries are currently disabled for teachers.");
        }

        var trimmedNotes = lessonNotes.Trim();
        var sections = BuildSections(trimmedNotes, governance.mode);
        var payload = JsonSerializer.Serialize(sections, SerializerOptions);

        var entity = new AISummary
        {
            TeacherId = teacherId,
            LessonNotes = trimmedNotes,
            Summary = payload,
            GeneratedAt = DateTime.UtcNow,
            Model = "gemini-2.5-flash"
        };

        _dbContext.AISummaries.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await RecordUsageAsync(
            new AiUsageLogRequest(
                requestedByUserId,
                trimmedNotes,
                FormatSectionsForLogging(sections),
                ResolveInteractionMode(governance.mode)),
            cancellationToken);

        return MapToDisplayModel(entity, sections);
    }

    public async Task<IReadOnlyList<LessonSummaryDisplayModel>> GetHistoryAsync(int teacherId, int take, CancellationToken cancellationToken = default)
    {
        await EnsureTeacherExistsAsync(teacherId, cancellationToken);
        var summaries = await _dbContext.AISummaries
            .AsNoTracking()
            .Where(s => s.TeacherId == teacherId)
            .OrderByDescending(s => s.GeneratedAt)
            .Take(Math.Clamp(take, 1, 25))
            .ToListAsync(cancellationToken);

        var results = new List<LessonSummaryDisplayModel>(summaries.Count);
        foreach (var summary in summaries)
        {
            results.Add(MapToDisplayModel(summary));
        }

        return results;
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

    private static LessonSummaryDisplayModel MapToDisplayModel(AISummary entity, IReadOnlyList<LessonSummarySection>? precomputedSections = null)
    {
        var sections = precomputedSections ?? DeserializeSections(entity.Summary);
        return new LessonSummaryDisplayModel(
            entity.Id,
            entity.GeneratedAt,
            entity.Model,
            entity.LessonNotes,
            sections);
    }

    private static IReadOnlyList<LessonSummarySection> DeserializeSections(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return new[] { new LessonSummarySection("Summary", new[] { "No details were captured." }) };
        }

        try
        {
            var sections = JsonSerializer.Deserialize<IReadOnlyList<LessonSummarySection>>(payload, SerializerOptions);
            if (sections is not null && sections.Count > 0)
            {
                return sections;
            }
        }
        catch
        {
            // Fall back to text parsing below.
        }

        return new[] { new LessonSummarySection("Summary", payload.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) };
    }

    private static IReadOnlyList<LessonSummarySection> BuildSections(string lessonNotes, TeacherAiMode mode)
    {
        var sentences = LessonTextHelper.SplitSentences(lessonNotes);
        var keywords = LessonTextHelper.ExtractKeywords(lessonNotes, 6);
        if (keywords.Count == 0)
        {
            keywords.Add("core ideas");
        }

        var sections = new List<LessonSummarySection>
        {
            new("Main Topics", BuildMainTopics(sentences, keywords)),
            new("Key Concepts", BuildKeyConcepts(sentences, keywords)),
            new("Important Takeaways", BuildTakeaways(keywords)),
            new("Study Tips", BuildStudyTips(keywords, mode))
        };

        return EnforceWordLimit(sections, 300);
    }

    private static IReadOnlyList<string> BuildMainTopics(IReadOnlyList<string> sentences, IReadOnlyList<string> keywords)
    {
        var bullets = new List<string>();
        foreach (var keyword in keywords.Take(3))
        {
            bullets.Add($"Explored {keyword} with concrete examples and quick check-ins.");
        }

        if (bullets.Count == 0 && sentences.Count > 0)
        {
            bullets.Add($"Reviewed: {sentences[0]}");
        }

        return bullets;
    }

    private static IReadOnlyList<string> BuildKeyConcepts(IReadOnlyList<string> sentences, IReadOnlyList<string> keywords)
    {
        var bullets = new List<string>();
        foreach (var sentence in sentences.Take(3))
        {
            bullets.Add(sentence);
        }

        if (bullets.Count < 3)
        {
            foreach (var keyword in keywords.Skip(bullets.Count).Take(3 - bullets.Count))
            {
                bullets.Add($"Connected {keyword} to prior knowledge and real-world contexts.");
            }
        }

        return bullets;
    }

    private static IReadOnlyList<string> BuildTakeaways(IReadOnlyList<string> keywords)
    {
        if (keywords.Count == 0)
        {
            return new[] { "Students summarized the session in their own words and captured next steps." };
        }

        return keywords.Take(3)
            .Select(keyword => $"Students articulated why {keyword} matters and how to demonstrate mastery.")
            .ToList();
    }

    private static IReadOnlyList<string> BuildStudyTips(IReadOnlyList<string> keywords, TeacherAiMode mode)
    {
        var focus = keywords.FirstOrDefault() ?? "today's focus";
        return mode switch
        {
            TeacherAiMode.Unrestricted => new[]
            {
                $"Review the worked examples on {focus} and attempt a fresh practice problem without the answer key.",
                $"Summarize the process for {focus} in three sentences, then teach it to a partner or family member.",
                "Check any remaining questions and bring them to the next lesson so we can unblock you quickly."
            },
            TeacherAiMode.Guided => new[]
            {
                $"Use a think-pair-share reflection: what was clear about {focus}? what still feels fuzzy?", 
                $"Create a two-column note (Steps vs. Why) for {focus} and add one example in each column.",
                "Plan a short review checkpoint tomorrow to verify retention."
            },
            _ => new[]
            {
                $"Skim today's notebook section on {focus} and highlight anything worth revisiting during independent study.",
                "Spend five minutes setting a goal for the next lesson and list a single question you want answered."
            }
        };
    }

    private static IReadOnlyList<LessonSummarySection> EnforceWordLimit(IReadOnlyList<LessonSummarySection> sections, int wordLimit)
    {
        var result = new List<LessonSummarySection>();
        var runningCount = 0;

        foreach (var section in sections)
        {
            var acceptedPoints = new List<string>();
            foreach (var point in section.Points)
            {
                var words = CountWords(point);
                if (runningCount + words > wordLimit)
                {
                    if (acceptedPoints.Count == 0 && runningCount < wordLimit)
                    {
                        var trimmed = TrimToWordBudget(point, wordLimit - runningCount);
                        if (!string.IsNullOrWhiteSpace(trimmed))
                        {
                            acceptedPoints.Add(trimmed);
                            runningCount = wordLimit;
                        }
                    }
                    break;
                }

                acceptedPoints.Add(point);
                runningCount += words;
            }

            if (acceptedPoints.Count > 0)
            {
                result.Add(new LessonSummarySection(section.Title, acceptedPoints));
            }

            if (runningCount >= wordLimit)
            {
                break;
            }
        }

        if (result.Count == 0)
        {
            return new[] { new LessonSummarySection("Summary", new[] { "Summary trimmed due to word limit." }) };
        }

        return result;
    }

    private static int CountWords(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
    }

    private static string TrimToWordBudget(string value, int remainingWords)
    {
        if (remainingWords <= 0)
        {
            return string.Empty;
        }

        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (words.Length <= remainingWords)
        {
            return value;
        }

        return string.Join(' ', words.Take(remainingWords)) + " …";
    }

    private static string FormatSectionsForLogging(IReadOnlyList<LessonSummarySection> sections)
    {
        return string.Join(" | ", sections.Select(section => $"{section.Title}:{string.Join(';', section.Points)}"));
    }

    private static AiInteractionMode ResolveInteractionMode(TeacherAiMode mode) => mode switch
    {
        TeacherAiMode.Unrestricted => AiInteractionMode.ShowAnswer,
        TeacherAiMode.Guided => AiInteractionMode.Guide,
        _ => AiInteractionMode.Explain
    };
}
