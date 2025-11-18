using System.ComponentModel.DataAnnotations;
using RateMyTeacher.Models.Enums;

namespace RateMyTeacher.Models;

public class AIControlSetting
{
    public int Id { get; set; }

    [Required]
    public AiControlScope Scope { get; set; }

    /// <summary>
    /// Optional foreign key identifier depending on scope. Null when scope is Global.
    /// </summary>
    public int? ScopeId { get; set; }

    public bool IsEnabled { get; set; } = true;

    [Required]
    public AiInteractionMode Mode { get; set; } = AiInteractionMode.Explain;

    [MaxLength(250)]
    public string? Notes { get; set; }

    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    public int? ModifiedById { get; set; }
    public User? ModifiedBy { get; set; }
}
