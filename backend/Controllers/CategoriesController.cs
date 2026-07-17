using backend.DTOs.Categories;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<CategoryResponseDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();

        return Ok(categories);
    }

    [AllowAnonymous]
    [HttpGet("{categoryId:int}")]
    public async Task<ActionResult<CategoryResponseDto>> GetById(
        int categoryId)
    {
        var category = await _categoryService.GetByIdAsync(categoryId);

        if (category is null)
        {
            return NotFound(new
            {
                message = "Category was not found."
            });
        }

        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryResponseDto>> Create(
        CreateCategoryDto request)
    {
        try
        {
            var category = await _categoryService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { categoryId = category.CategoryId },
                category
            );
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{categoryId:int}")]
    public async Task<ActionResult<CategoryResponseDto>> Update(
        int categoryId,
        UpdateCategoryDto request)
    {
        try
        {
            var category = await _categoryService.UpdateAsync(
                categoryId,
                request
            );

            if (category is null)
            {
                return NotFound(new
                {
                    message = "Category was not found."
                });
            }

            return Ok(category);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{categoryId:int}")]
    public async Task<IActionResult> Delete(int categoryId)
    {
        try
        {
            var deleted = await _categoryService.DeleteAsync(categoryId);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Category was not found."
                });
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }
}