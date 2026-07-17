using backend.Data;
using backend.DTOs.Categories;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            })
            .ToListAsync();
    }

    public async Task<CategoryResponseDto?> GetByIdAsync(int categoryId)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(category => category.CategoryId == categoryId)
            .Select(category => new CategoryResponseDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            })
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryResponseDto> CreateAsync(
        CreateCategoryDto request)
    {
        var normalizedName = request.Name.Trim();

        var categoryExists = await _context.Categories
            .AnyAsync(category =>
                category.Name.ToLower() == normalizedName.ToLower()
            );

        if (categoryExists)
        {
            throw new InvalidOperationException(
                "A category with this name already exists."
            );
        }

        var category = new Category
        {
            Name = normalizedName,
            Description = request.Description.Trim()
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return MapToResponse(category);
    }

    public async Task<CategoryResponseDto?> UpdateAsync(
        int categoryId,
        UpdateCategoryDto request)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(category =>
                category.CategoryId == categoryId
            );

        if (category is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();

        var categoryExists = await _context.Categories
            .AnyAsync(existingCategory =>
                existingCategory.CategoryId != categoryId &&
                existingCategory.Name.ToLower() ==
                normalizedName.ToLower()
            );

        if (categoryExists)
        {
            throw new InvalidOperationException(
                "A category with this name already exists."
            );
        }

        category.Name = normalizedName;
        category.Description = request.Description.Trim();

        await _context.SaveChangesAsync();

        return MapToResponse(category);
    }

    public async Task<bool> DeleteAsync(int categoryId)
    {
        var category = await _context.Categories
            .Include(category => category.Products)
            .FirstOrDefaultAsync(category =>
                category.CategoryId == categoryId
            );

        if (category is null)
        {
            return false;
        }

        if (category.Products.Count > 0)
        {
            throw new InvalidOperationException(
                "The category cannot be deleted because it contains products."
            );
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return true;
    }

    private static CategoryResponseDto MapToResponse(Category category)
    {
        return new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            Name = category.Name,
            Description = category.Description
        };
    }
}