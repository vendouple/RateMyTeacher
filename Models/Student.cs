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

    [MaxLength(50)]
    public string? StudentNumber { get; set; }

    [Required]
    [Range(1, 12)]
    public int GradeLevel { get; set; } = 9;

    [MaxLength(15)]
    [RegularExpression("^\\+?[0-9]{10,15}$", ErrorMessage = "Parent contact must be a valid phone number.")]
    public string? ParentContact { get; set; }

    [MaxLength(120)]
    public string? Homeroom { get; set; }

    public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}
