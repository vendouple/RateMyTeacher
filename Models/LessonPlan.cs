using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models;

public class LessonPlan
{
    public int Id { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [ForeignKey(nameof(TeacherId))]
    public Teacher Teacher { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string GradeLevel { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string TopicFocus { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? StudentNeeds { get; set; }

    public int? DurationMinutes { get; set; }

    [Required]
    public string SectionsJson { get; set; } = string.Empty;

    [Required]
    public string ResourcesJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
