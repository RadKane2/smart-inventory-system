using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Orders;

public class CreateOrderDto
{
    [Required]
    [MinLength(
        1,
        ErrorMessage = "The order must contain at least one product."
    )]
    public List<CreateOrderItemDto> Items { get; set; } = [];
}