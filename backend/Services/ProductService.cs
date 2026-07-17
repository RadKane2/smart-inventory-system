using backend.Data;
using backend.DTOs.Products;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class ProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductResponseDto>> GetAllAsync(
        string? search,
        int? categoryId,
        bool includeInactive)
    {
        var query = _context.Products
            .AsNoTracking()
            .Include(product => product.Category)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(product =>
                product.Status == "Active"
            );
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();

            query = query.Where(product =>
                product.Name.ToLower().Contains(normalizedSearch) ||
                product.Description.ToLower().Contains(normalizedSearch)
            );
        }

        if (categoryId.HasValue)
        {
            query = query.Where(product =>
                product.CategoryId == categoryId.Value
            );
        }

        return await query
            .OrderBy(product => product.Name)
            .Select(product => new ProductResponseDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                MinimumStock = product.MinimumStock,
                IsLowStock =
                    product.Stock <= product.MinimumStock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(
        int productId)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(product =>
                product.ProductId == productId
            )
            .Select(product => new ProductResponseDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                MinimumStock = product.MinimumStock,
                IsLowStock =
                    product.Stock <= product.MinimumStock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                ImageUrl = product.ImageUrl,
                Status = product.Status,
                CreatedAt = product.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProductResponseDto> CreateAsync(
        CreateProductDto request)
    {
        var categoryExists = await _context.Categories
            .AnyAsync(category =>
                category.CategoryId == request.CategoryId
            );

        if (!categoryExists)
        {
            throw new KeyNotFoundException(
                "The selected category was not found."
            );
        }

        var normalizedName = request.Name.Trim();

        var productExists = await _context.Products
            .AnyAsync(product =>
                product.Name.ToLower() ==
                normalizedName.ToLower()
            );

        if (productExists)
        {
            throw new InvalidOperationException(
                "A product with this name already exists."
            );
        }

        var product = new Product
        {
            Name = normalizedName,
            Description = request.Description.Trim(),
            Price = request.Price,
            Stock = 0,
            MinimumStock = request.MinimumStock,
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl?.Trim(),
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.ProductId)
            ?? throw new InvalidOperationException(
                "The product could not be retrieved."
            );
    }

    public async Task<ProductResponseDto?> UpdateAsync(
        int productId,
        UpdateProductDto request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(product =>
                product.ProductId == productId
            );

        if (product is null)
        {
            return null;
        }

        var categoryExists = await _context.Categories
            .AnyAsync(category =>
                category.CategoryId == request.CategoryId
            );

        if (!categoryExists)
        {
            throw new KeyNotFoundException(
                "The selected category was not found."
            );
        }

        var normalizedName = request.Name.Trim();

        var duplicateProduct = await _context.Products
            .AnyAsync(existingProduct =>
                existingProduct.ProductId != productId &&
                existingProduct.Name.ToLower() ==
                normalizedName.ToLower()
            );

        if (duplicateProduct)
        {
            throw new InvalidOperationException(
                "A product with this name already exists."
            );
        }

        var normalizedStatus = request.Status.Trim();

        if (normalizedStatus != "Active" &&
            normalizedStatus != "Inactive")
        {
            throw new ArgumentException(
                "Status must be Active or Inactive."
            );
        }

        product.Name = normalizedName;
        product.Description = request.Description.Trim();
        product.Price = request.Price;
        product.MinimumStock = request.MinimumStock;
        product.CategoryId = request.CategoryId;
        product.ImageUrl = request.ImageUrl?.Trim();
        product.Status = normalizedStatus;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.ProductId);
    }

    public async Task<bool> DeactivateAsync(int productId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(product =>
                product.ProductId == productId
            );

        if (product is null)
        {
            return false;
        }

        product.Status = "Inactive";

        await _context.SaveChangesAsync();

        return true;
    }
}
