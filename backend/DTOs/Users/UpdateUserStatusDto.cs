using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Users;

public class UpdateUserStatusDto
{
    [Required]
    [RegularExpression(
        "^(Active|Inactive)$",
        ErrorMessage = "Status must be Active or Inactive."
    )]
    public string Status { get; set; } = string.Empty;
}