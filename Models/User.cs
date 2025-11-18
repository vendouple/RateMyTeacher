using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RateMyTeacher.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MinLength(2)]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MinLength(60)]
    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Role { get; set; } = "Student";

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }

    [MaxLength(64)]
    public string TimeZone { get; set; } = "UTC";

    [MaxLength(8)]
    public string Locale { get; set; } = "en";

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public bool MustChangePassword { get; set; }

    [ForeignKey(nameof(DeletedById))]
    public User? DeletedBy { get; set; }

    public Teacher? TeacherProfile { get; set; }
    public Student? StudentProfile { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}".Trim();
}
