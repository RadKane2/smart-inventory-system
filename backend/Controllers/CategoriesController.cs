using backend.DTOs.Categories;
using backend.Services;
using System.Security.Claims;
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
            var userIdValue =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var category = await _categoryService.CreateAsync(
                request,
                userId
            );

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
            var userIdValue =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var category = await _categoryService.UpdateAsync(
            categoryId,
            request,
            userId
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
            var userIdValue = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (!int.TryParse(userIdValue, out var userId))
            {
                return Unauthorized(new
                {
                    message =
                        "The authenticated user identifier is invalid."
                });
            }

            var deleted = await _categoryService.DeleteAsync(
                categoryId,
                userId
            );

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