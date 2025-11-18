using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models.Admin;
using RateMyTeacher.Services;

namespace RateMyTeacher.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IRatingService _ratingService;
    private readonly IBonusService _bonusService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext dbContext,
        IRatingService ratingService,
        IBonusService bonusService,
        ILogger<AdminController> logger)
    {
        _dbContext = dbContext;
        _ratingService = ratingService;
        _bonusService = bonusService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Leaderboard(int? semesterId, CancellationToken cancellationToken)
    {
        var settings = await _bonusService.GetSettingsAsync(cancellationToken);
        var leaderboard = await _ratingService.GetLeaderboardAsync(semesterId, settings.MinimumRatingsThreshold, cancellationToken);
        var payouts = _bonusService.CalculatePayouts(leaderboard.Entries, settings);
        var semesters = await GetSemesterOptionsAsync(cancellationToken);

        var viewModel = new LeaderboardPageViewModel
        {
            Settings = settings,
            Leaderboard = leaderboard,
            Payouts = payouts,
            Semesters = semesters,
            SelectedSemesterId = leaderboard.Semester.Id,
            StatusMessage = TempData.TryGetValue("LeaderboardMessage", out var status) ? status?.ToString() : null
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AwardBonuses(LeaderboardAwardRequest request, CancellationToken cancellationToken)
    {
        var settings = await _bonusService.GetSettingsAsync(cancellationToken);
        var leaderboard = await _ratingService.GetLeaderboardAsync(
            request.SemesterId,
            settings.MinimumRatingsThreshold,
            cancellationToken);

        if (!leaderboard.Entries.Any())
        {
            TempData["LeaderboardMessage"] = "No teachers met the minimum ratings threshold for that semester.";
            return RedirectToAction(nameof(Leaderboard), new { semesterId = leaderboard.Semester.Id });
        }

        var payouts = _bonusService.CalculatePayouts(leaderboard.Entries, settings);

        if (payouts.Count == 0)
        {
            TempData["LeaderboardMessage"] = "Bonus tiers did not match any teachers. Adjust the configuration and try again.";
            return RedirectToAction(nameof(Leaderboard), new { semesterId = leaderboard.Semester.Id });
        }

        var result = await _bonusService.AwardAsync(leaderboard, payouts, GetUserId(), cancellationToken);
        TempData["LeaderboardMessage"] = $"Awarded {result.AwardedCount} bonuses (batch {result.BatchId}).";

        return RedirectToAction(nameof(Leaderboard), new { semesterId = leaderboard.Semester.Id });
    }

    [HttpGet("/Admin/Settings/AiControls")]
    public IActionResult AiControlsShortcut()
    {
        return RedirectToAction("AiControls", "Settings");
    }

    private async Task<IReadOnlyList<SemesterOptionViewModel>> GetSemesterOptionsAsync(CancellationToken cancellationToken)
    {
        var semesters = await _dbContext.Semesters
            .AsNoTracking()
            .OrderByDescending(s => s.IsCurrent)
            .ThenByDescending(s => s.StartDate)
            .Select(s => new SemesterOptionViewModel(s.Id, s.Name, s.IsCurrent))
            .ToListAsync(cancellationToken);

        return semesters;
    }

    private int? GetUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var parsed) ? parsed : null;
    }
}
