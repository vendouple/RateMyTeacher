using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateMyTeacher.Models.Lessons;

public class LessonSummaryForm
{
    [DisplayName("Teacher")]
    public int? TeacherId { get; set; }

    [Required]
    [DisplayName("Lesson notes")]
    [MinLength(20, ErrorMessage = "Please include at least 20 characters of lesson context.")]
    [MaxLength(2000, ErrorMessage = "Lesson notes must be 2,000 characters or fewer.")]
    public string LessonNotes { get; set; } = string.Empty;
}

public sealed record LessonSummarySection(string Title, IReadOnlyList<string> Points);

public sealed record LessonSummaryDisplayModel(
    int Id,
    DateTime GeneratedAt,
    string Model,
    string LessonNotes,
    IReadOnlyList<LessonSummarySection> Sections);

public sealed record LessonSummaryPageViewModel(
    LessonSummaryForm Form,
    LessonSummaryDisplayModel? GeneratedSummary,
    IReadOnlyList<LessonSummaryDisplayModel> History,
    IReadOnlyList<TeacherOption> TeacherOptions,
    bool RequireTeacherSelection);

public sealed record TeacherOption(int Id, string Name);
