namespace RateMyTeacher.Models;

public class AISummary
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public int? ClassId { get; set; }
    public string LessonNotes { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string Model { get; set; } = "gemini-2.5-flash";

    public Teacher Teacher { get; set; } = null!;
}
