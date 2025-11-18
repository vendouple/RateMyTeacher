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

    [DisplayName("Teacher AI mode")]
    [Required]
    public TeacherAiMode TeacherMode { get; set; } = TeacherAiMode.Guided;

    [DisplayName("Student AI mode")]
    [Required]
    public StudentAiMode StudentMode { get; set; } = StudentAiMode.Learning;
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

public sealed record AiControlsPageViewModel(
    AiGlobalSettingsForm GlobalSettings,
    IReadOnlyList<AiUsageLogDto> UsageLogs,
    AiUsageLogFilterModel Filter);
