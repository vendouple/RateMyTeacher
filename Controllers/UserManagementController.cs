using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models;
using RateMyTeacher.Models.UserManagement;

namespace RateMyTeacher.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private static readonly string[] DefaultRoles = new[] { "Admin", "Teacher", "Student" };
    private const string DefaultResetPassword = "Classroom123!";

    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UserManagementController(
        ApplicationDbContext dbContext,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var viewModel = await BuildIndexViewModelAsync(null, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidVm = await BuildIndexViewModelAsync(model, cancellationToken);
            return View("Index", invalidVm);
        }

        var normalizedRole = NormalizeRole(model.Role);
        if (normalizedRole is null)
        {
            ModelState.AddModelError(nameof(CreateUserViewModel.Role), "Choose a valid role.");
            var invalidVm = await BuildIndexViewModelAsync(model, cancellationToken);
            return View("Index", invalidVm);
        }

        if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email, cancellationToken))
        {
            ModelState.AddModelError(nameof(CreateUserViewModel.Email), "Another account already uses that email address.");
            var invalidVm = await BuildIndexViewModelAsync(model, cancellationToken);
            return View("Index", invalidVm);
        }

        if (await _dbContext.Users.AnyAsync(u => u.Username == model.Username, cancellationToken))
        {
            ModelState.AddModelError(nameof(CreateUserViewModel.Username), "That username is already taken.");
            var invalidVm = await BuildIndexViewModelAsync(model, cancellationToken);
            return View("Index", invalidVm);
        }

        var user = new User
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            Email = model.Email.Trim(),
            Username = model.Username.Trim(),
            Role = normalizedRole,
            TimeZone = "UTC",
            Locale = "en-US",
            MustChangePassword = true
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await EnsureProfileAsync(user, cancellationToken);

        TempData["UserManagementStatus"] = $"Created {normalizedRole} {user.FullName}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRole(UpdateUserRoleViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var invalidVm = await BuildIndexViewModelAsync(null, cancellationToken);
            return View("Index", invalidVm);
        }

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == model.UserId, cancellationToken);
        if (user is null)
        {
            TempData["UserManagementStatus"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var normalizedRole = NormalizeRole(model.Role);
        if (normalizedRole is null)
        {
            TempData["UserManagementStatus"] = "Choose a valid role.";
            return RedirectToAction(nameof(Index));
        }

        user.Role = normalizedRole;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        await EnsureProfileAsync(user, cancellationToken);

        TempData["UserManagementStatus"] = $"Updated {user.FullName}'s role to {normalizedRole}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int userId, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            TempData["UserManagementStatus"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, DefaultResetPassword);
        user.MustChangePassword = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData["UserManagementStatus"] = $"Reset password for {user.FullName}. Temporary password: {DefaultResetPassword}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<UserManagementIndexViewModel> BuildIndexViewModelAsync(CreateUserViewModel? form, CancellationToken cancellationToken)
    {
        var summaries = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new UserSummaryViewModel(
                u.Id,
                u.FullName,
                u.Email,
                u.Role,
                u.CreatedAt,
                u.LastLoginAt,
                u.MustChangePassword))
            .ToListAsync(cancellationToken);

        var roleOptions = await _dbContext.Users
            .AsNoTracking()
            .Select(u => u.Role)
            .Distinct()
            .ToListAsync(cancellationToken);

        var availableRoles = DefaultRoles
            .Concat(roleOptions)
            .Select(NormalizeRole)
            .Where(r => r is not null)
            .Select(r => r!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(r => r.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
            .ThenBy(r => r, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var viewModel = new UserManagementIndexViewModel
        {
            Users = summaries,
            AvailableRoles = availableRoles,
            CreateUser = form ?? new CreateUserViewModel { Role = "Teacher" },
            StatusMessage = TempData.TryGetValue("UserManagementStatus", out var status) ? status?.ToString() : null
        };

        return viewModel;
    }

    private async Task EnsureProfileAsync(User user, CancellationToken cancellationToken)
    {
        if (string.Equals(user.Role, "Teacher", StringComparison.OrdinalIgnoreCase))
        {
            var hasProfile = await _dbContext.Teachers.AnyAsync(t => t.UserId == user.Id, cancellationToken);
            if (!hasProfile)
            {
                _dbContext.Teachers.Add(new Teacher
                {
                    UserId = user.Id,
                    Department = "General Studies",
                    HireDate = DateTime.UtcNow
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
        else if (string.Equals(user.Role, "Student", StringComparison.OrdinalIgnoreCase))
        {
            var hasProfile = await _dbContext.Students.AnyAsync(s => s.UserId == user.Id, cancellationToken);
            if (!hasProfile)
            {
                _dbContext.Students.Add(new Student
                {
                    UserId = user.Id,
                    GradeLevel = 9,
                    EnrollmentDate = DateTime.UtcNow,
                    IsActive = true
                });
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }

    private static string? NormalizeRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return null;
        }

        return DefaultRoles.FirstOrDefault(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
    }
}
