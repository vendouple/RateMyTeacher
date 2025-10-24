namespace RateMyTeacher.Models;

public class Semester
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string AcademicYear { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }

    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<TeacherRanking> Rankings { get; set; } = new List<TeacherRanking>();
}
