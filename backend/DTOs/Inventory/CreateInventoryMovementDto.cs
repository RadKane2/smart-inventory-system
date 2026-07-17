using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Inventory;

public class CreateInventoryMovementDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [RegularExpression(
        "^(Entry|Exit)$",
        ErrorMessage = "Type must be Entry or Exit."
    )]
    public string Type { get; set; } = string.Empty;

    [Range(
        1,
        int.MaxValue,
        ErrorMessage = "Quantity must be greater than zero."
    )]
    public int Quantity { get; set; }
}