namespace RateMyTeacher.Models;

public class Teacher
{
    public int Id { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImage { get; set; }
    public DateTime HireDate { get; set; }
    public string? Department { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<TeacherRanking> Rankings { get; set; } = new List<TeacherRanking>();
    public ICollection<AISummary> Summaries { get; set; } = new List<AISummary>();
}
