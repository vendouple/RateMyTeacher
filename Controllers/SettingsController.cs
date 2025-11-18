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
        SystemSetting.Keys.Ai.GlobalMode,
        SystemSetting.Keys.Ai.DepartmentDefaultMode,
        SystemSetting.Keys.Ai.ClassDefaultMode
    };

    private readonly ApplicationDbContext _dbContext;
    private readonly IAIUsageService _aiUsageService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ApplicationDbContext dbContext,
        IAIUsageService aiUsageService,
        ILogger<SettingsController> logger)
    {
        _dbContext = dbContext;
        _aiUsageService = aiUsageService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> AiControls([FromQuery] AiUsageLogFilterModel filter, [FromQuery] int? scopeEntryId, CancellationToken cancellationToken)
    {
        var scopeForm = await LoadScopeFormAsync(scopeEntryId, cancellationToken);
        var viewModel = await BuildViewModelAsync(filter, scopeForm, null, cancellationToken);
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateGlobal(AiGlobalSettingsForm form, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var filter = new AiUsageLogFilterModel();
            var vm = await BuildViewModelAsync(filter, null, form, cancellationToken);
            return View("AiControls", vm);
        }

        await PersistGlobalSettingsAsync(form, cancellationToken);
        TempData["StatusMessage"] = "AI global settings updated.";
        return RedirectToAction(nameof(AiControls));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpsertScope(AiScopeForm form, CancellationToken cancellationToken)
    {
        if (form.Scope != AiControlScope.Global && form.ScopeId is null)
        {
            ModelState.AddModelError(nameof(AiScopeForm.ScopeId), "Scope identifier is required for department or class overrides.");
        }

        if (form.Scope == AiControlScope.Global)
        {
            form.ScopeId = null;
        }

        if (!ModelState.IsValid)
        {
            var filter = new AiUsageLogFilterModel();
            var vm = await BuildViewModelAsync(filter, form, null, cancellationToken);
            return View("AiControls", vm);
        }

        await UpsertScopeEntityAsync(form, cancellationToken);
        TempData["StatusMessage"] = form.Id.HasValue
            ? "Scope override updated."
            : "Scope override created.";
        return RedirectToAction(nameof(AiControls));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteScope(int id, CancellationToken cancellationToken)
    {
        var scopeSetting = await _dbContext.AIControlSettings
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (scopeSetting is null)
        {
            TempData["StatusMessage"] = "Scope entry no longer exists.";
            return RedirectToAction(nameof(AiControls));
        }

        if (scopeSetting.Scope == AiControlScope.Global)
        {
            TempData["StatusMessage"] = "Global scope cannot be deleted.";
            return RedirectToAction(nameof(AiControls));
        }

        _dbContext.AIControlSettings.Remove(scopeSetting);
        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData["StatusMessage"] = "Scope override deleted.";
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
        AiScopeForm? scopeForm,
        AiGlobalSettingsForm? globalForm,
        CancellationToken cancellationToken)
    {
        filter ??= new AiUsageLogFilterModel();
        scopeForm ??= new AiScopeForm();
        var globalSettings = globalForm ?? await LoadGlobalSettingsAsync(cancellationToken);

        var scopeSettings = await _dbContext.AIControlSettings
            .AsNoTracking()
            .Include(s => s.ModifiedBy)
            .OrderBy(s => s.Scope)
            .ThenBy(s => s.ScopeId ?? 0)
            .Select(s => new AiScopeViewModel(
                s.Id,
                s.Scope,
                s.ScopeId,
                s.IsEnabled,
                s.Mode,
                s.Notes,
                s.ModifiedAt,
                s.ModifiedBy != null ? s.ModifiedBy.FullName : null))
            .ToListAsync(cancellationToken);

        var usageLogs = await _aiUsageService.GetLogsAsync(filter.ToQuery(), cancellationToken);

        return new AiControlsPageViewModel(globalSettings, scopeSettings, usageLogs, filter, scopeForm);
    }

    private async Task<AiScopeForm?> LoadScopeFormAsync(int? scopeEntryId, CancellationToken cancellationToken)
    {
        if (scopeEntryId is null)
        {
            return null;
        }

        var entity = await _dbContext.AIControlSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == scopeEntryId.Value, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        return new AiScopeForm
        {
            Id = entity.Id,
            Scope = entity.Scope,
            ScopeId = entity.ScopeId,
            IsEnabled = entity.IsEnabled,
            Mode = entity.Mode,
            Notes = entity.Notes
        };
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
            GlobalMode = ParseMode(settings, SystemSetting.Keys.Ai.GlobalMode),
            DepartmentMode = ParseMode(settings, SystemSetting.Keys.Ai.DepartmentDefaultMode),
            ClassMode = ParseMode(settings, SystemSetting.Keys.Ai.ClassDefaultMode)
        };
    }

    private async Task PersistGlobalSettingsAsync(AiGlobalSettingsForm form, CancellationToken cancellationToken)
    {
        var upserts = new Dictionary<string, string>
        {
            [SystemSetting.Keys.Ai.GlobalEnabled] = form.IsGloballyEnabled.ToString(),
            [SystemSetting.Keys.Ai.GlobalMode] = form.GlobalMode.ToString(),
            [SystemSetting.Keys.Ai.DepartmentDefaultMode] = form.DepartmentMode.ToString(),
            [SystemSetting.Keys.Ai.ClassDefaultMode] = form.ClassMode.ToString()
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
        globalSetting.Mode = form.GlobalMode;
        globalSetting.ModifiedAt = DateTime.UtcNow;
        globalSetting.ModifiedById = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task UpsertScopeEntityAsync(AiScopeForm form, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var normalizedScopeId = form.Scope == AiControlScope.Global ? null : form.ScopeId;
        AIControlSetting? entity = null;

        if (form.Id.HasValue)
        {
            entity = await _dbContext.AIControlSettings
                .FirstOrDefaultAsync(s => s.Id == form.Id.Value, cancellationToken);

            if (entity is null)
            {
                _logger.LogWarning("Scope entry {ScopeId} not found for update.", form.Id);
            }
        }
        else
        {
            entity = await _dbContext.AIControlSettings
                .FirstOrDefaultAsync(s => s.Scope == form.Scope && s.ScopeId == normalizedScopeId, cancellationToken);
        }

        if (entity is null)
        {
            entity = new AIControlSetting();
            _dbContext.AIControlSettings.Add(entity);
        }

        entity.Scope = form.Scope;
        entity.ScopeId = normalizedScopeId;
        entity.IsEnabled = form.IsEnabled;
        entity.Mode = form.Mode;
        entity.Notes = form.Notes;
        entity.ModifiedAt = DateTime.UtcNow;
        entity.ModifiedById = currentUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool ParseBool(IReadOnlyDictionary<string, string> settings, string key, bool defaultValue)
    {
        return settings.TryGetValue(key, out var value) && bool.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    private static AiInteractionMode ParseMode(IReadOnlyDictionary<string, string> settings, string key)
    {
        if (settings.TryGetValue(key, out var stored) && Enum.TryParse<AiInteractionMode>(stored, out var mode))
        {
            return mode;
        }

        return AiInteractionMode.Explain;
    }

    private int? GetCurrentUserId()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(idClaim, out var userId) ? userId : null;
    }
}
