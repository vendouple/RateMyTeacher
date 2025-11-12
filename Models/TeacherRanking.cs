namespace RateMyTeacher.Models;

public class TeacherRanking
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public int SemesterId { get; set; }
    public int Rank { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public decimal BonusAmount { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

    public Teacher Teacher { get; set; } = null!;
    public Semester Semester { get; set; } = null!;
}
