using RateMyTeacher.Models.Enums;

namespace RateMyTeacher.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int? DeletedById { get; set; }
    public User? DeletedBy { get; set; }

    public Teacher? TeacherProfile { get; set; }
    public Student? StudentProfile { get; set; }
    public ICollection<AIUsageLog> AIUsageLogs { get; set; } = new List<AIUsageLog>();
}
