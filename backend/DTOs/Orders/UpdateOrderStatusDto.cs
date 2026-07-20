using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Orders;

public class UpdateOrderStatusDto
{
    [Required]
    [RegularExpression(
        "^(Pending|Processing|Shipped|Delivered|Cancelled)$",
        ErrorMessage =
            "Status must be Pending, Processing, Shipped, " +
            "Delivered or Cancelled."
    )]
    public string Status { get; set; } = string.Empty;
}