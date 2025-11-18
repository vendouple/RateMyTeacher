using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using RateMyTeacher.Models.Enums;
using RateMyTeacher.Services.Models;

namespace RateMyTeacher.Models.Settings;

public class AiGlobalSettingsForm
{
    [DisplayName("AI features enabled")]
    public bool IsGloballyEnabled { get; set; } = true;

    [DisplayName("Global default mode")]
    [Required]
    public AiInteractionMode GlobalMode { get; set; } = AiInteractionMode.Explain;

    [DisplayName("Department fallback mode")]
    [Required]
    public AiInteractionMode DepartmentMode { get; set; } = AiInteractionMode.Explain;

    [DisplayName("Class fallback mode")]
    [Required]
    public AiInteractionMode ClassMode { get; set; } = AiInteractionMode.Explain;
}

public class AiScopeForm
{
    public int? Id { get; set; }

    [Required]
    [DisplayName("Scope type")]
    public AiControlScope Scope { get; set; }

    [DisplayName("Scope identifier")]
    public int? ScopeId { get; set; }

    [DisplayName("AI enabled for scope")]
    public bool IsEnabled { get; set; } = true;

    [DisplayName("Mode")]
    [Required]
    public AiInteractionMode Mode { get; set; } = AiInteractionMode.Explain;

    [DisplayName("Notes")]
    [StringLength(250)]
    public string? Notes { get; set; }
}

public class AiUsageLogFilterModel
{
    [DisplayName("User ID")]
    public int? UserId { get; set; }

    [DisplayName("Class ID")]
    public int? ClassId { get; set; }

    [DisplayName("Mode")]
    public AiInteractionMode? Mode { get; set; }

    [DataType(DataType.Date)]
    [DisplayName("From")]
    public DateTime? From { get; set; }

    [DataType(DataType.Date)]
    [DisplayName("To")]
    public DateTime? To { get; set; }

    [Range(10, 200)]
    [DisplayName("Rows")]
    public int Take { get; set; } = 50;

    public AiUsageLogQuery ToQuery() => new(UserId, ClassId, Mode, From, To, Take);
}

public sealed record AiScopeViewModel(
    int Id,
    AiControlScope Scope,
    int? ScopeId,
    bool IsEnabled,
    AiInteractionMode Mode,
    string? Notes,
    DateTime ModifiedAt,
    string? ModifiedBy);

public sealed record AiControlsPageViewModel(
    AiGlobalSettingsForm GlobalSettings,
    IReadOnlyList<AiScopeViewModel> ScopeSettings,
    IReadOnlyList<AiUsageLogDto> UsageLogs,
    AiUsageLogFilterModel Filter,
    AiScopeForm ScopeForm);
