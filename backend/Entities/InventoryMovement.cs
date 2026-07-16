namespace backend.Entities;

public class InventoryMovement
{
    public int MovementId { get; set; }

    public int ProductId { get; set; }

    public int UserId { get; set; }

    public int Quantity { get; set; }

    public string Type { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public Product Product { get; set; } = null!;

    public User User { get; set; } = null!;
}