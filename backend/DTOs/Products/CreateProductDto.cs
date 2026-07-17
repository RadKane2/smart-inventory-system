using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Products;

public class CreateProductDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 999999999)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int MinimumStock { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Url]
    public string? ImageUrl { get; set; }
}
