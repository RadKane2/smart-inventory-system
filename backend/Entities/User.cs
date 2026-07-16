namespace backend.Entities;

public class User
{
    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int RoleId { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? LastLogin { get; set; }

    public Role Role { get; set; } = null!;

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    public ICollection<InventoryMovement> InventoryMovements { get; set; }
        = new List<InventoryMovement>();

    public ICollection<Notification> Notifications { get; set; }
        = new List<Notification>();

    public ICollection<AuditLog> AuditLogs { get; set; }
        = new List<AuditLog>();
}