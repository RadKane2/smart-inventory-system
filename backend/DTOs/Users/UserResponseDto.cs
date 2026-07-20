namespace backend.DTOs.Users;

public class UserResponseDto
{
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }
}