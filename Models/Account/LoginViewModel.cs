using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RateMyTeacher.Models.Account;

public class LoginViewModel
{
    [Required]
    [DisplayName("Username or email")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
