using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models;
using RateMyTeacher.Models.Dashboard;

namespace RateMyTeacher.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private static readonly string[] DefaultQuickLinkIcons = new[] { "users", "clipboard", "settings", "trending" };

    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApplicationDbContext dbContext, ILogger<DashboardController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Admin(CancellationToken cancellationToken)
    {
        var teacherCountTask = _dbContext.Users.CountAsync(u => u.Role == "Teacher", cancellationToken);
        var studentCountTask = _dbContext.Users.CountAsync(u => u.Role == "Student", cancellationToken);
        var ratingsCountTask = _dbContext.Ratings.CountAsync(cancellationToken);
        var summariesCountTask = _dbContext.AISummaries.CountAsync(cancellationToken);

        await Task.WhenAll(teacherCountTask, studentCountTask, ratingsCountTask, summariesCountTask);

        var cards = new[]
        {
            new DashboardCardViewModel("Teachers", teacherCountTask.Result.ToString("N0"), "Active instructors", "people", "View teachers", "Teachers", "Index"),
            new DashboardCardViewModel("Students", studentCountTask.Result.ToString("N0"), "Enrolled learners", "graduation", "Student list", "UserManagement", "Index"),
            new DashboardCardViewModel("Ratings", ratingsCountTask.Result.ToString("N0"), "Submitted all time", "star", "Leaderboard", "Admin", "Leaderboard"),
            new DashboardCardViewModel("AI summaries", summariesCountTask.Result.ToString("N0"), "Generated lessons", "cpu", "AI controls", "Settings", "AiControls")
        };

        var aiActivities = await _dbContext.AIUsageLogs
            .AsNoTracking()
            .Include(l => l.User)
            .OrderByDescending(l => l.Timestamp)
            .Take(5)
            .Select(l => new DashboardActivityViewModel(
                $"AI {l.Mode}",
                $"{l.User.FirstName} {l.User.LastName} requested a response",
                l.Timestamp))
            .ToListAsync(cancellationToken);

        var newUsers = await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new DashboardActivityViewModel(
                $"New {u.Role}",
                $"{u.FirstName} {u.LastName} was added to the workspace",
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        var activities = aiActivities
            .Concat(newUsers)
            .OrderByDescending(a => a.Timestamp)
            .Take(6)
            .ToList();

        var quickLinks = new[]
        {
            new DashboardQuickLinkViewModel("Manage users", "UserManagement", "Index", DefaultQuickLinkIcons[0]),
            new DashboardQuickLinkViewModel("Bonus settings", "Admin", "Leaderboard", DefaultQuickLinkIcons[1]),
            new DashboardQuickLinkViewModel("System settings", "Settings", "AiControls", DefaultQuickLinkIcons[2]),
            new DashboardQuickLinkViewModel("Teacher rankings", "Admin", "Leaderboard", DefaultQuickLinkIcons[3])
        };

        var viewModel = new AdminDashboardViewModel
        {
            Cards = cards,
            Activities = activities,
            QuickLinks = quickLinks
        };

        return View(viewModel);
    }

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Teacher(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var teacherProfile = await _dbContext.Teachers
            .AsNoTracking()
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserId == userId, cancellationToken);

        if (teacherProfile is null)
        {
            _logger.LogWarning("Teacher profile missing for user {UserId}", userId);
            var emptyView = new TeacherDashboardViewModel
            {
                HasProfile = false,
                TeacherName = User.Identity?.Name ?? "Teacher",
                QuickLinks = BuildTeacherQuickLinks(null)
            };
            return View(emptyView);
        }

        var ratingStats = await _dbContext.Ratings
            .AsNoTracking()
            .Where(r => r.TeacherId == teacherProfile.Id)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Count = g.Count(),
                Average = g.Average(r => (double)r.Stars)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var summaryCountTask = _dbContext.AISummaries.CountAsync(a => a.TeacherId == teacherProfile.Id, cancellationToken);
        var aiSessionsTask = _dbContext.AIUsageLogs.CountAsync(l => l.UserId == userId, cancellationToken);
        await Task.WhenAll(summaryCountTask, aiSessionsTask);

        var recentRatings = await _dbContext.Ratings
            .AsNoTracking()
            .Where(r => r.TeacherId == teacherProfile.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .Select(r => new TeacherDashboardRatingViewModel(
                r.Student.User.FirstName + " " + r.Student.User.LastName,
                r.Semester.Name,
                r.Stars,
                r.Comment,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        var viewModel = new TeacherDashboardViewModel
        {
            HasProfile = true,
            TeacherName = $"{teacherProfile.User.FirstName} {teacherProfile.User.LastName}",
            AverageRating = ratingStats?.Average ?? 0,
            RatingsCount = ratingStats?.Count ?? 0,
            SummaryCount = summaryCountTask.Result,
            AiSessions = aiSessionsTask.Result,
            RecentRatings = recentRatings,
            QuickLinks = BuildTeacherQuickLinks(teacherProfile.Id)
        };

        return View(viewModel);
    }

    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Student(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return RedirectToAction("Login", "Account");
        }

        var studentProfile = await _dbContext.Students
            .AsNoTracking()
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        if (studentProfile is null)
        {
            _logger.LogWarning("Student profile missing for user {UserId}", userId);
            var placeholder = new StudentDashboardViewModel
            {
                HasProfile = false,
                StudentName = User.Identity?.Name ?? "Student",
                QuickLinks = BuildStudentQuickLinks()
            };
            return View(placeholder);
        }

        var ratingsCountTask = _dbContext.Ratings.CountAsync(r => r.StudentId == studentProfile.Id, cancellationToken);
        var teacherCountTask = _dbContext.Teachers.CountAsync(cancellationToken);
        var aiSessionsTask = _dbContext.AIUsageLogs.CountAsync(l => l.UserId == userId, cancellationToken);
        await Task.WhenAll(ratingsCountTask, teacherCountTask, aiSessionsTask);

        var suggestions = await _dbContext.Ratings
            .AsNoTracking()
            .GroupBy(r => new
            {
                r.TeacherId,
                r.Teacher.User.FirstName,
                r.Teacher.User.LastName,
                r.Teacher.Department
            })
            .Select(g => new StudentSuggestedTeacherViewModel(
                g.Key.TeacherId,
                g.Key.FirstName + " " + g.Key.LastName,
                g.Average(r => (double)r.Stars),
                g.Count(),
                g.Key.Department))
            .OrderByDescending(g => g.AverageRating)
            .ThenByDescending(g => g.RatingCount)
            .Take(4)
            .ToListAsync(cancellationToken);

        var viewModel = new StudentDashboardViewModel
        {
            HasProfile = true,
            StudentName = $"{studentProfile.User.FirstName} {studentProfile.User.LastName}",
            RatingsSubmitted = ratingsCountTask.Result,
            AvailableTeachers = teacherCountTask.Result,
            AiSessions = aiSessionsTask.Result,
            Suggestions = suggestions,
            QuickLinks = BuildStudentQuickLinks()
        };

        return View(viewModel);
    }

    private IReadOnlyList<DashboardQuickLinkViewModel> BuildTeacherQuickLinks(int? teacherId)
    {
        var links = new List<DashboardQuickLinkViewModel>
        {
            new DashboardQuickLinkViewModel("Teacher roster", "Teachers", "Index", "people"),
            new DashboardQuickLinkViewModel("Portal home", "Dashboard", "Teacher", "dashboard")
        };

        if (teacherId.HasValue)
        {
            links.Add(new DashboardQuickLinkViewModel(
                "My public profile",
                "Teachers",
                "Details",
                "profile",
                new { id = teacherId.Value }));
        }

        return links;
    }

    private IReadOnlyList<DashboardQuickLinkViewModel> BuildStudentQuickLinks()
        => new[]
        {
            new DashboardQuickLinkViewModel("Browse teachers", "Teachers", "Index", "people"),
            new DashboardQuickLinkViewModel("Submit a rating", "Teachers", "Index", "star", new { highlightRating = true }),
            new DashboardQuickLinkViewModel("Privacy & policies", "Home", "Privacy", "shield")
        };

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : null;
    }
}
