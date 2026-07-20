using backend.Data;
using backend.DTOs.Notifications;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class NotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationResponseDto>> GetAllAsync(
        int userId,
        bool unreadOnly)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.UserId == userId
            );

        if (unreadOnly)
        {
            query = query.Where(notification =>
                !notification.IsRead
            );
        }

        return await query
            .OrderByDescending(notification =>
                notification.CreatedAt
            )
            .Select(notification =>
                new NotificationResponseDto
                {
                    NotificationId =
                        notification.NotificationId,

                    Title = notification.Title,

                    Message = notification.Message,

                    IsRead = notification.IsRead,

                    CreatedAt = notification.CreatedAt
                })
            .ToListAsync();
    }

    public async Task<NotificationResponseDto?> GetByIdAsync(
        int notificationId,
        int userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(notification =>
                notification.NotificationId ==
                    notificationId &&
                notification.UserId == userId
            )
            .Select(notification =>
                new NotificationResponseDto
                {
                    NotificationId =
                        notification.NotificationId,

                    Title = notification.Title,

                    Message = notification.Message,

                    IsRead = notification.IsRead,

                    CreatedAt = notification.CreatedAt
                })
            .FirstOrDefaultAsync();
    }

    public async Task<UnreadNotificationCountDto>
        GetUnreadCountAsync(int userId)
    {
        var count = await _context.Notifications
            .CountAsync(notification =>
                notification.UserId == userId &&
                !notification.IsRead
            );

        return new UnreadNotificationCountDto
        {
            Count = count
        };
    }

    public async Task<NotificationResponseDto?> MarkAsReadAsync(
        int notificationId,
        int userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(notification =>
                notification.NotificationId ==
                    notificationId &&
                notification.UserId == userId
            );

        if (notification is null)
        {
            return null;
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;

            await _context.SaveChangesAsync();
        }

        return new NotificationResponseDto
        {
            NotificationId = notification.NotificationId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    public async Task<int> MarkAllAsReadAsync(int userId)
    {
        var unreadNotifications =
            await _context.Notifications
                .Where(notification =>
                    notification.UserId == userId &&
                    !notification.IsRead
                )
                .ToListAsync();

        if (unreadNotifications.Count == 0)
        {
            return 0;
        }

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return unreadNotifications.Count;
    }
}