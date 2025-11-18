using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models;
using RateMyTeacher.Models.Account;

namespace RateMyTeacher.Controllers;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AccountController> _logger;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AccountController(
        ApplicationDbContext dbContext,
        ILogger<AccountController> logger,
        IPasswordHasher<User> passwordHasher)
    {
        _dbContext = dbContext;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == model.UsernameOrEmail || u.Email == model.UsernameOrEmail);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);
        }

        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(7)
                : DateTimeOffset.UtcNow.AddMinutes(30)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            authProperties);

        _logger.LogInformation("User {Username} signed in.", user.Username);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        _logger.LogInformation("User {UserName} signed out.", User.Identity?.Name);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
