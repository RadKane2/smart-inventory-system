namespace backend.Entities;

public class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public User? User { get; set; }
}