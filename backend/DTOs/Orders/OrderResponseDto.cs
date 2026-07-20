namespace backend.DTOs.Orders;

public class OrderResponseDto
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<OrderDetailResponseDto> Items { get; set; } = [];
}