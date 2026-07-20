using backend.Data;
using backend.DTOs.Categories;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class CategoryService
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _auditService;

    public CategoryService(
        ApplicationDbContext context,
        AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
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
        CreateCategoryDto request,
        int userId)
    {
        var normalizedName = request.Name.Trim();

        var categoryExists = await _context.Categories
            .AnyAsync(category =>
                category.Name.ToLower() ==
                normalizedName.ToLower());

        if (categoryExists)
        {
            throw new InvalidOperationException(
                "A category with this name already exists.");
        }

        var category = new Category
        {
            Name = normalizedName,
            Description = request.Description.Trim()
        };

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        await _auditService.CreateLogAsync(
            userId,
            "Create",
            "Category",
            category.CategoryId,
            $"Created category '{category.Name}'."
        );

        return MapToResponse(category);
    }

    public async Task<CategoryResponseDto?> UpdateAsync(
        int categoryId,
        UpdateCategoryDto request,
        int userId)

    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(category =>
                category.CategoryId == categoryId);

        if (category is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();

        var categoryExists = await _context.Categories
            .AnyAsync(existingCategory =>
                existingCategory.CategoryId != categoryId &&
                existingCategory.Name.ToLower() ==
                normalizedName.ToLower());

        if (categoryExists)
        {
            throw new InvalidOperationException(
                "A category with this name already exists.");
        }

        category.Name = normalizedName;
        category.Description = request.Description.Trim();

        await _context.SaveChangesAsync();

        await _auditService.CreateLogAsync(
            userId,
            "Update",
            "Category",
            category.CategoryId,
            $"Updated category '{category.Name}'."
        );

        return MapToResponse(category);
    }

    public async Task<bool> DeleteAsync(
        int categoryId,
        int userId)
    {
        var category = await _context.Categories
            .Include(category => category.Products)
            .FirstOrDefaultAsync(category =>
                category.CategoryId == categoryId);

        if (category is null)
        {
            return false;
        }

        if (category.Products.Count > 0)
        {
            throw new InvalidOperationException(
                "The category cannot be deleted because it contains products.");
        }

        // Guardamos estos datos antes de eliminar la categoría.
        var deletedCategoryId = category.CategoryId;
        var deletedCategoryName = category.Name;

        _context.Categories.Remove(category);

        // Primero se elimina la categoría en SQL Server.
        await _context.SaveChangesAsync();

        // Después se registra quién eliminó la categoría.
        await _auditService.CreateLogAsync(
            userId,
            "Delete",
            "Category",
            deletedCategoryId,
            $"Deleted category '{deletedCategoryName}'."
        );

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