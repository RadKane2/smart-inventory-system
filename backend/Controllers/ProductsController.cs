using System.Security.Claims;
using backend.DTOs.Products;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ProductResponseDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? categoryId)
    {
        var products = await _productService.GetAllAsync(
            search,
            categoryId,
            includeInactive: false
        );

        return Ok(products);
    }

    [AllowAnonymous]
    [HttpGet("{productId:int}")]
    public async Task<ActionResult<ProductResponseDto>> GetById(
        int productId)
    {
        var product = await _productService.GetByIdAsync(
            productId
        );

        if (product is null)
        {
            return NotFound(new
            {
                message = "Product was not found."
            });
        }

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public async Task<ActionResult<List<ProductResponseDto>>>
        GetAllAdmin(
            [FromQuery] string? search,
            [FromQuery] int? categoryId)
    {
        var products = await _productService.GetAllAsync(
            search,
            categoryId,
            includeInactive: true
        );

        return Ok(products);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponseDto>> Create(
        CreateProductDto request)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        try
        {
            var product = await _productService.CreateAsync(
                request,
                userId
            );

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    productId = product.ProductId
                },
                product
            );
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
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
    [HttpPut("{productId:int}")]
    public async Task<ActionResult<ProductResponseDto>> Update(
        int productId,
        UpdateProductDto request)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        try
        {
            var product = await _productService.UpdateAsync(
                productId,
                request,
                userId
            );

            if (product is null)
            {
                return NotFound(new
                {
                    message = "Product was not found."
                });
            }

            return Ok(product);
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{productId:int}")]
    public async Task<IActionResult> Deactivate(
        int productId)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        var deactivated =
            await _productService.DeactivateAsync(
                productId,
                userId
            );

        if (!deactivated)
        {
            return NotFound(new
            {
                message = "Product was not found."
            });
        }

        return NoContent();
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        return int.TryParse(userIdValue, out userId);
    }
}