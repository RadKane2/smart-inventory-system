using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Users;

public class CreateUserDto
{
    [Required]
    [StringLength(
        100,
        MinimumLength = 2,
        ErrorMessage = "Name must contain between 2 and 100 characters."
    )]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress(
        ErrorMessage = "A valid email address is required."
    )]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(
        100,
        MinimumLength = 8,
        ErrorMessage = "Password must contain at least 8 characters."
    )]
    public string Password { get; set; } = string.Empty;

    [Required]
    [RegularExpression(
        "^(Admin|Employee)$",
        ErrorMessage = "Role must be Admin or Employee."
    )]
    public string Role { get; set; } = string.Empty;
}