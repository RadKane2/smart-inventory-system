using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Users;

public class UpdateUserRoleDto
{
    [Required]
    [RegularExpression(
        "^(Admin|Employee|Customer)$",
        ErrorMessage =
            "Role must be Admin, Employee or Customer."
    )]
    public string Role { get; set; } = string.Empty;
}