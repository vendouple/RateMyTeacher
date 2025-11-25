using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models.Lessons;
using RateMyTeacher.Services;

namespace RateMyTeacher.Controllers;

[Authorize(Roles = "Teacher,Admin")]
public class LessonsController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILessonSummaryService _lessonSummaryService;

    public LessonsController(ApplicationDbContext dbContext, ILessonSummaryService lessonSummaryService)
    {
        _dbContext = dbContext;
        _lessonSummaryService = lessonSummaryService;
    }

    [HttpGet]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var form = new LessonSummaryForm();
        if (!User.IsInRole("Admin"))
        {
            form.TeacherId = await GetTeacherIdForCurrentUserAsync(cancellationToken);
        }

        var teacherId = form.TeacherId;
        var viewModel = await BuildSummaryPageAsync(form, teacherId, null, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Summary(LessonSummaryForm form, CancellationToken cancellationToken)
    {
        var resolvedTeacherId = await ResolveTeacherIdAsync(form, cancellationToken);
        if (resolvedTeacherId.teacherId is null)
        {
            if (!string.IsNullOrEmpty(resolvedTeacherId.error))
            {
                ModelState.AddModelError(nameof(form.TeacherId), resolvedTeacherId.error);
            }

            var fallbackVm = await BuildSummaryPageAsync(form, null, null, cancellationToken);
            return View(fallbackVm);
        }

        if (!ModelState.IsValid)
        {
            var invalidVm = await BuildSummaryPageAsync(form, resolvedTeacherId.teacherId, null, cancellationToken);
            return View(invalidVm);
        }

        LessonSummaryDisplayModel? generated = null;
        try
        {
            var userId = GetCurrentUserId() ?? resolvedTeacherId.teacherId.Value;
            generated = await _lessonSummaryService.GenerateAsync(resolvedTeacherId.teacherId.Value, userId, form.LessonNotes, cancellationToken);
            ViewData["StatusMessage"] = "Lesson summary generated.";
            form = new LessonSummaryForm
            {
                TeacherId = User.IsInRole("Admin") ? resolvedTeacherId.teacherId : null
            };
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        var vm = await BuildSummaryPageAsync(form, resolvedTeacherId.teacherId, generated, cancellationToken);
        return View(vm);
    }

    private async Task<LessonSummaryPageViewModel> BuildSummaryPageAsync(
        LessonSummaryForm form,
        int? teacherId,
        LessonSummaryDisplayModel? generatedSummary,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<LessonSummaryDisplayModel> history = Array.Empty<LessonSummaryDisplayModel>();
        if (teacherId is int existingTeacherId)
        {
            history = await _lessonSummaryService.GetHistoryAsync(existingTeacherId, 5, cancellationToken);
        }

        var teacherOptions = await GetTeacherOptionsAsync(cancellationToken);
        var requireTeacherSelection = User.IsInRole("Admin") && teacherId is null;
        return new LessonSummaryPageViewModel(form, generatedSummary, history, teacherOptions, requireTeacherSelection);
    }

    private async Task<(int? teacherId, string? error)> ResolveTeacherIdAsync(LessonSummaryForm form, CancellationToken cancellationToken)
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
        return int.TryParse(claimValue, out var userId) ? userId : null;
    }
}
