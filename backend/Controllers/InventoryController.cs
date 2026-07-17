using System.Security.Claims;
using backend.DTOs.Inventory;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Employee")]
public class InventoryController : ControllerBase
{
    private readonly InventoryService _inventoryService;

    public InventoryController(
        InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet("movements")]
    public async Task<
        ActionResult<List<InventoryMovementResponseDto>>
    > GetAll([FromQuery] int? productId)
    {
        var movements =
            await _inventoryService.GetAllAsync(productId);

        return Ok(movements);
    }

    [HttpGet("movements/{movementId:int}")]
    public async Task<ActionResult<InventoryMovementResponseDto>>
        GetById(int movementId)
    {
        var movement =
            await _inventoryService.GetByIdAsync(movementId);

        if (movement is null)
        {
            return NotFound(new
            {
                message = "Inventory movement was not found."
            });
        }

        return Ok(movement);
    }

    [HttpPost("movements")]
    public async Task<ActionResult<InventoryMovementResponseDto>>
        Create(CreateInventoryMovementDto request)
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

        try
        {
            var movement =
                await _inventoryService.CreateAsync(
                    request,
                    userId
                );

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    movementId = movement.MovementId
                },
                movement
            );
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(new
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
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }
}