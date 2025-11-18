using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateMyTeacher.Data;
using RateMyTeacher.Models;
using RateMyTeacher.Models.Enums;
using RateMyTeacher.Models.Settings;
using RateMyTeacher.Services;

namespace RateMyTeacher.Controllers;

[Authorize(Roles = "Admin")]
public class SettingsController : Controller
{
    private static readonly string[] AiSettingKeys =
    {
        SystemSetting.Keys.Ai.GlobalEnabled,
        SystemSetting.Keys.Ai.TeacherMode,
        SystemSetting.Keys.Ai.StudentMode
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly IAIUsageService _aiUsageService;

    public SettingsController(
        ApplicationDbContext dbContext,
        IAIUsageService aiUsageService)
    {
        _dbContext = dbContext;
        _aiUsageService = aiUsageService;
    }

    [HttpGet]
    public async Task<IActionResult> AiControls([FromQuery] AiUsageLogFilterModel filter, CancellationToken cancellationToken)
    {
        var viewModel = await BuildViewModelAsync(filter, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateGlobal(AiGlobalSettingsForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var filter = new AiUsageLogFilterModel();
            var vm = await BuildViewModelAsync(filter, cancellationToken, form);
            return View("AiControls", vm);
        }

        await PersistGlobalSettingsAsync(form, cancellationToken);
        TempData["StatusMessage"] = "AI global settings updated.";
        return RedirectToAction(nameof(AiControls));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkLogViewed(int id, CancellationToken cancellationToken)
    {
        await _aiUsageService.MarkViewedAsync(id, cancellationToken);
        return RedirectToAction(nameof(AiControls));
    }

    private async Task<AiControlsPageViewModel> BuildViewModelAsync(
        AiUsageLogFilterModel? filter,
        CancellationToken cancellationToken,
        AiGlobalSettingsForm? formOverride = null)
    {
        filter ??= new AiUsageLogFilterModel();
        var globalSettings = formOverride ?? await LoadGlobalSettingsAsync(cancellationToken);
        var usageLogs = await _aiUsageService.GetLogsAsync(filter.ToQuery(), cancellationToken);

        return new AiControlsPageViewModel(globalSettings, usageLogs, filter);
    }

    private async Task<AiGlobalSettingsForm> LoadGlobalSettingsAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.SystemSettings
            .AsNoTracking()
            .Where(s => AiSettingKeys.Contains(s.Key))
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        return new AiGlobalSettingsForm
        {
            IsGloballyEnabled = ParseBool(settings, SystemSetting.Keys.Ai.GlobalEnabled, defaultValue: true),
            TeacherMode = ParseTeacherMode(settings, SystemSetting.Keys.Ai.TeacherMode),
            StudentMode = ParseStudentMode(settings, SystemSetting.Keys.Ai.StudentMode)
        };
    }

    private async Task PersistGlobalSettingsAsync(AiGlobalSettingsForm form, CancellationToken cancellationToken)
    {
        var upserts = new Dictionary<string, string>
        {
            [SystemSetting.Keys.Ai.GlobalEnabled] = form.IsGloballyEnabled.ToString(),
            [SystemSetting.Keys.Ai.TeacherMode] = form.TeacherMode.ToString(),
            [SystemSetting.Keys.Ai.StudentMode] = form.StudentMode.ToString()
        };

        var existing = await _dbContext.SystemSettings
            .Where(s => AiSettingKeys.Contains(s.Key))
            .ToListAsync(cancellationToken);

        foreach (var (key, value) in upserts)
        {
            var setting = existing.FirstOrDefault(s => s.Key == key);
            if (setting is null)
            {
                _dbContext.SystemSettings.Add(new SystemSetting
                {
                    Key = key,
                    Value = value,
                    ModifiedAt = DateTime.UtcNow
                });
            }
            else
            {
                setting.Value = value;
                setting.ModifiedAt = DateTime.UtcNow;
            }
        }

        var currentUserId = GetCurrentUserId();
        var globalSetting = await _dbContext.AIControlSettings
            .FirstOrDefaultAsync(s => s.Scope == AiControlScope.Global, cancellationToken);

        if (globalSetting is null)
        {
            globalSetting = new AIControlSetting
            {
                Scope = AiControlScope.Global
            };
            _dbContext.AIControlSettings.Add(globalSetting);
        }

        globalSetting.IsEnabled = form.IsGloballyEnabled;
        globalSetting.Mode = form.TeacherMode == TeacherAiMode.Unrestricted
            ? AiInteractionMode.ShowAnswer
            : form.TeacherMode == TeacherAiMode.Guided
                ? AiInteractionMode.Guide
                : AiInteractionMode.Explain;
        globalSetting.ModifiedAt = DateTime.UtcNow;
        globalSetting.ModifiedById = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool ParseBool(IReadOnlyDictionary<string, string> settings, string key, bool defaultValue)
    {
        return settings.TryGetValue(key, out var value) && bool.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    private static TeacherAiMode ParseTeacherMode(IReadOnlyDictionary<string, string> settings, string key)
    {
        if (settings.TryGetValue(key, out var stored) && Enum.TryParse<TeacherAiMode>(stored, out var mode))
        {
            return mode;
        }

        return TeacherAiMode.Guided;
    }

    private static StudentAiMode ParseStudentMode(IReadOnlyDictionary<string, string> settings, string key)
    {
        if (settings.TryGetValue(key, out var stored) && Enum.TryParse<StudentAiMode>(stored, out var mode))
        {
            return mode;
        }

        return StudentAiMode.Learning;
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}
