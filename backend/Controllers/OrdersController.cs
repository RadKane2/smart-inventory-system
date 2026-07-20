using System.Security.Claims;
using backend.DTOs.Orders;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<OrderResponseDto>>> GetAll()
    {
        var orders = await _orderService.GetAllAsync();

        return Ok(orders);
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("my-orders")]
    public async Task<ActionResult<List<OrderResponseDto>>> GetMyOrders()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        var orders = await _orderService.GetByUserAsync(userId);

        return Ok(orders);
    }

    [Authorize(Roles = "Admin,Customer")]
    [HttpGet("{orderId:int}")]
    public async Task<ActionResult<OrderResponseDto>> GetById(
        int orderId)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        var isAdmin = User.IsInRole("Admin");

        var order = await _orderService.GetByIdAsync(
            orderId,
            userId,
            isAdmin
        );

        if (order is null)
        {
            return NotFound(new
            {
                message = "Order was not found."
            });
        }

        return Ok(order);
    }

    [Authorize(Roles = "Customer")]
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> Create(
        CreateOrderDto request)
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
            var order = await _orderService.CreateAsync(
                request,
                userId
            );

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    orderId = order.OrderId
                },
                order
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

    [Authorize(Roles = "Admin")]
    [HttpPatch("{orderId:int}/status")]
    public async Task<ActionResult<OrderResponseDto>> UpdateStatus(
        int orderId,
        UpdateOrderStatusDto request)
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
            var order = await _orderService.UpdateStatusAsync(
                orderId,
                request,
                userId
            );

            if (order is null)
            {
                return NotFound(new
                {
                    message = "Order was not found."
                });
            }

            return Ok(order);
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

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        return int.TryParse(userIdValue, out userId);
    }
}
