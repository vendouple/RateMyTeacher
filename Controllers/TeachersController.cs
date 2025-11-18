using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateMyTeacher.Models.Teachers;
using RateMyTeacher.Services;

namespace RateMyTeacher.Controllers;

[Authorize]
public class TeachersController : Controller
{
    private readonly ITeacherService _teacherService;
    private readonly IRatingService _ratingService;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(
        ITeacherService teacherService,
        IRatingService ratingService,
        ILogger<TeachersController> logger)
    {
        _teacherService = teacherService;
        _ratingService = ratingService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var summaries = await _teacherService.GetSummariesAsync(cancellationToken);
        return View(new TeacherListViewModel { Teachers = summaries });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var viewModel = await BuildDetailViewModelAsync(id, null, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        if (TempData.TryGetValue("RatingMessage", out var message) && message is string text)
        {
            viewModel.StatusMessage = text;
        }

        return View(viewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Student")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rate(RatingFormModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await RedisplayDetailsAsync(model.TeacherId, model, cancellationToken);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Challenge();
        }

        var submission = new RatingSubmission(model.TeacherId, model.Stars, model.Comment);
        var result = await _ratingService.SubmitAsync(userId.Value, submission, cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to submit rating at this time.");
            return await RedisplayDetailsAsync(model.TeacherId, model, cancellationToken);
        }

        TempData["RatingMessage"] = result.CreatedNew
            ? "Thanks for sharing your feedback!"
            : "Your rating has been updated.";

        return RedirectToAction(nameof(Details), new { id = model.TeacherId });
    }

    private async Task<IActionResult> RedisplayDetailsAsync(int teacherId, RatingFormModel model, CancellationToken cancellationToken)
    {
        var viewModel = await BuildDetailViewModelAsync(teacherId, model, cancellationToken);
        if (viewModel is null)
        {
            return NotFound();
        }

        return View("Details", viewModel);
    }

    private async Task<TeacherDetailViewModel?> BuildDetailViewModelAsync(
        int teacherId,
        RatingFormModel? overrideForm,
        CancellationToken cancellationToken)
    {
        var detail = await _teacherService.GetDetailAsync(teacherId, cancellationToken);
        if (detail is null)
        {
            return null;
        }

        var userId = GetUserId();
        var canSubmitRating = User.IsInRole("Student") && userId.HasValue;

        StudentRatingDto? studentRating = null;
        if (canSubmitRating)
        {
            studentRating = await _ratingService.GetStudentRatingAsync(userId!.Value, teacherId, cancellationToken);
        }

        var form = overrideForm ?? new RatingFormModel
        {
            TeacherId = teacherId,
            Stars = studentRating?.Stars ?? 5,
            Comment = studentRating?.Comment
        };

        return new TeacherDetailViewModel
        {
            Teacher = detail,
            ExistingRating = studentRating,
            RatingForm = form,
            CanSubmitRating = canSubmitRating
        };
    }

    private int? GetUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var parsed) ? parsed : null;
    }
}
