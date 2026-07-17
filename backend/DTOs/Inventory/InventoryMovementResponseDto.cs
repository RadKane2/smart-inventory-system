namespace backend.DTOs.Inventory;

public class InventoryMovementResponseDto
{
    public int MovementId { get; set; }

    public int ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int StockAfterMovement { get; set; }

    public DateTime Date { get; set; }
}