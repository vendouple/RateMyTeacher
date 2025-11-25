using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateMyTeacher.Models.Lessons;

public class LessonPlanForm
{
    [DisplayName("Teacher")]
    public int? TeacherId { get; set; }

    [Required]
    [DisplayName("Subject or course")]
    [StringLength(100)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [DisplayName("Grade level")]
    [StringLength(50)]
    public string GradeLevel { get; set; } = string.Empty;

    [Required]
    [DisplayName("Topics or objectives")]
    [MinLength(10)]
    [MaxLength(500)]
    public string TopicFocus { get; set; } = string.Empty;

    [DisplayName("Learner needs or notes")]
    [MaxLength(500)]
    public string? StudentNeeds { get; set; }

    [DisplayName("Minutes available")]
    [Range(10, 180)]
    public int? DurationMinutes { get; set; }
}

public sealed record LessonPlanSection(string Title, IReadOnlyList<string> Steps);

public sealed record LessonPlanDisplayModel(
    int Id,
    DateTime CreatedAt,
    string Subject,
    string GradeLevel,
    string TopicFocus,
    string? StudentNeeds,
    int? DurationMinutes,
    IReadOnlyList<LessonPlanSection> Sections,
    IReadOnlyList<string> Resources);

public sealed record LessonPlanningPageViewModel(
    LessonPlanForm Form,
    LessonPlanDisplayModel? GeneratedPlan,
    IReadOnlyList<LessonPlanDisplayModel> History,
    IReadOnlyList<TeacherOption> TeacherOptions,
    bool RequireTeacherSelection);

public sealed record LessonPlanRequest(
    int TeacherId,
    int RequestedByUserId,
    string Subject,
    string GradeLevel,
    string TopicFocus,
    string? StudentNeeds,
    int? DurationMinutes);
