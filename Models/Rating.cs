namespace RateMyTeacher.Models;

public class Rating
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int SemesterId { get; set; }
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Student Student { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
    public SentimentAnalysis? SentimentAnalysis { get; set; }
}
