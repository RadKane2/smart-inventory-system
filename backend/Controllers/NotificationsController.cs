using System.Security.Claims;
using backend.DTOs.Notifications;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _notificationService;

    public NotificationsController(
        NotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<
        ActionResult<List<NotificationResponseDto>>
    > GetAll([FromQuery] bool unreadOnly = false)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        var notifications =
            await _notificationService.GetAllAsync(
                userId,
                unreadOnly
            );

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<
        ActionResult<UnreadNotificationCountDto>
    > GetUnreadCount()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        var response =
            await _notificationService
                .GetUnreadCountAsync(userId);

        return Ok(response);
    }

    [HttpGet("{notificationId:int}")]
    public async Task<ActionResult<NotificationResponseDto>>
        GetById(int notificationId)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        var notification =
            await _notificationService.GetByIdAsync(
                notificationId,
                userId
            );

        if (notification is null)
        {
            return NotFound(new
            {
                message = "Notification was not found."
            });
        }

        return Ok(notification);
    }

    [HttpPatch("{notificationId:int}/read")]
    public async Task<ActionResult<NotificationResponseDto>>
        MarkAsRead(int notificationId)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        var notification =
            await _notificationService.MarkAsReadAsync(
                notificationId,
                userId
            );

        if (notification is null)
        {
            return NotFound(new
            {
                message = "Notification was not found."
            });
        }

        return Ok(notification);
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            return Unauthorized(new
            {
                message =
                    "The authenticated user identifier is invalid."
            });
        }

        var updatedCount =
            await _notificationService
                .MarkAllAsReadAsync(userId);

        return Ok(new
        {
            message = "Notifications were marked as read.",
            updatedCount
        });
    }

    private bool TryGetAuthenticatedUserId(out int userId)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        return int.TryParse(userIdValue, out userId);
    }
}