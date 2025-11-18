using System.ComponentModel.DataAnnotations;

namespace RateMyTeacher.Models.UserManagement;

public sealed record UserSummaryViewModel(
    int Id,
    string Name,
    string Email,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool MustChangePassword
);

public class CreateUserViewModel
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string Role { get; set; } = "Teacher";
}

public class UpdateUserRoleViewModel
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MaxLength(32)]
    public string Role { get; set; } = string.Empty;
}

public class UserManagementIndexViewModel
{
    public IReadOnlyList<UserSummaryViewModel> Users { get; init; } = Array.Empty<UserSummaryViewModel>();
    public IReadOnlyList<string> AvailableRoles { get; init; } = Array.Empty<string>();
    public CreateUserViewModel CreateUser { get; init; } = new();
    public string? StatusMessage { get; init; }
}
