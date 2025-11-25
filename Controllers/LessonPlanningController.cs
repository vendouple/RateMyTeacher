using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models.Lessons;
using RateMyTeacher.Services;

namespace RateMyTeacher.Controllers;

[Authorize(Roles = "Teacher,Admin")]
public class LessonPlanningController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILessonPlanningService _lessonPlanningService;

    public LessonPlanningController(ApplicationDbContext dbContext, ILessonPlanningService lessonPlanningService)
    {
        _dbContext = dbContext;
        _lessonPlanningService = lessonPlanningService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var form = new LessonPlanForm
        {
            TeacherId = User.IsInRole("Admin") ? null : await GetTeacherIdForCurrentUserAsync(cancellationToken),
            Subject = "",
            GradeLevel = "",
            TopicFocus = string.Empty
        };

        var vm = await BuildPlanningViewModelAsync(form, form.TeacherId, null, cancellationToken);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(LessonPlanForm form, CancellationToken cancellationToken)
    {
        var resolvedTeacherId = await ResolveTeacherIdAsync(form, cancellationToken);
        if (resolvedTeacherId.teacherId is null)
        {
            if (!string.IsNullOrEmpty(resolvedTeacherId.error))
            {
                ModelState.AddModelError(nameof(form.TeacherId), resolvedTeacherId.error);
            }

            var fallbackVm = await BuildPlanningViewModelAsync(form, null, null, cancellationToken);
            return View(fallbackVm);
        }

        if (!ModelState.IsValid)
        {
            var invalidVm = await BuildPlanningViewModelAsync(form, resolvedTeacherId.teacherId, null, cancellationToken);
            return View(invalidVm);
        }

        LessonPlanDisplayModel? generated = null;
        try
        {
            var userId = GetCurrentUserId() ?? resolvedTeacherId.teacherId.Value;
            var request = new LessonPlanRequest(
                resolvedTeacherId.teacherId.Value,
                userId,
                form.Subject,
                form.GradeLevel,
                form.TopicFocus,
                form.StudentNeeds,
                form.DurationMinutes);

            generated = await _lessonPlanningService.GenerateAsync(request, cancellationToken);
            ViewData["StatusMessage"] = "Lesson plan created.";
            form = new LessonPlanForm
            {
                TeacherId = User.IsInRole("Admin") ? resolvedTeacherId.teacherId : null
            };
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        var vm = await BuildPlanningViewModelAsync(form, resolvedTeacherId.teacherId, generated, cancellationToken);
        return View(vm);
    }

    private async Task<LessonPlanningPageViewModel> BuildPlanningViewModelAsync(
        LessonPlanForm form,
        int? teacherId,
        LessonPlanDisplayModel? generatedPlan,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<LessonPlanDisplayModel> history = Array.Empty<LessonPlanDisplayModel>();
        if (teacherId is int existingTeacherId)
        {
            history = await _lessonPlanningService.GetHistoryAsync(existingTeacherId, 5, cancellationToken);
        }

        var teacherOptions = await GetTeacherOptionsAsync(cancellationToken);
        var requireTeacherSelection = User.IsInRole("Admin") && teacherId is null;
        return new LessonPlanningPageViewModel(form, generatedPlan, history, teacherOptions, requireTeacherSelection);
    }

    private async Task<(int? teacherId, string? error)> ResolveTeacherIdAsync(LessonPlanForm form, CancellationToken cancellationToken)
    {
        if (User.IsInRole("Admin"))
        {
            if (form.TeacherId is null)
            {
                return (null, "Select a teacher to continue.");
            }

            var exists = await _dbContext.Teachers.AnyAsync(t => t.Id == form.TeacherId.Value, cancellationToken);
            return exists
                ? (form.TeacherId, null)
                : (null, "Teacher not found.");
        }

        var teacherId = await GetTeacherIdForCurrentUserAsync(cancellationToken);
        return teacherId is null
            ? (null, "We could not find a teacher profile for your account.")
            : (teacherId, null);
    }

    private async Task<int?> GetTeacherIdForCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return null;
        }

        return await _dbContext.Teachers
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => (int?)t.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<TeacherOption>> GetTeacherOptionsAsync(CancellationToken cancellationToken)
    {
        if (!User.IsInRole("Admin"))
        {
            return Array.Empty<TeacherOption>();
        }

        return await _dbContext.Teachers
            .AsNoTracking()
            .OrderBy(t => t.User.FirstName)
            .ThenBy(t => t.User.LastName)
            .Select(t => new TeacherOption(t.Id, t.User.FullName))
            .ToListAsync(cancellationToken);
    }

    private int? GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claimValue, out var userId) ? userId : (int?)null;
    }
}
