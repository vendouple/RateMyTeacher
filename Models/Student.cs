namespace RateMyTeacher.Models;

public class Student
{
    public int Id { get; set; }
    public int GradeLevel { get; set; }
    public string? ParentContact { get; set; }
    public DateTime EnrollmentDate { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
