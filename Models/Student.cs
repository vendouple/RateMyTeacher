using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models;

public class Student
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string StudentNumber { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? GradeLevel { get; set; }

    [MaxLength(120)]
    public string? Homeroom { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
}
