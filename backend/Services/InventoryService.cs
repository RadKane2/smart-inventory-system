using backend.Data;
using backend.DTOs.Inventory;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class InventoryService
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _auditService;

    public InventoryService(
        ApplicationDbContext context,
        AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<InventoryMovementResponseDto>> GetAllAsync(
        int? productId)
    {
        var query = _context.InventoryMovements
            .AsNoTracking()
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(movement =>
                movement.ProductId == productId.Value
            );
        }

        return await query
            .OrderByDescending(movement => movement.Date)
            .Select(movement =>
                new InventoryMovementResponseDto
                {
                    MovementId = movement.MovementId,
                    ProductId = movement.ProductId,
                    ProductName = movement.Product.Name,
                    UserId = movement.UserId,
                    UserName = movement.User.Name,
                    Type = movement.Type,
                    Quantity = movement.Quantity,
                    StockAfterMovement =
                        movement.StockAfterMovement,
                    Date = movement.Date
                })
            .ToListAsync();
    }

    public async Task<InventoryMovementResponseDto?> GetByIdAsync(
        int movementId)
    {
        return await _context.InventoryMovements
            .AsNoTracking()
            .Where(movement =>
                movement.MovementId == movementId
            )
            .Select(movement =>
                new InventoryMovementResponseDto
                {
                    MovementId = movement.MovementId,
                    ProductId = movement.ProductId,
                    ProductName = movement.Product.Name,
                    UserId = movement.UserId,
                    UserName = movement.User.Name,
                    Type = movement.Type,
                    Quantity = movement.Quantity,
                    StockAfterMovement =
                        movement.StockAfterMovement,
                    Date = movement.Date
                })
            .FirstOrDefaultAsync();
    }

    public async Task<InventoryMovementResponseDto> CreateAsync(
        CreateInventoryMovementDto request,
        int userId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(product =>
                product.ProductId == request.ProductId
            );

        if (product is null)
        {
            throw new KeyNotFoundException(
                "Product was not found."
            );
        }

        if (!string.Equals(
                product.Status,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Inventory movements cannot be created " +
                "for an inactive product."
            );
        }

        var userExists = await _context.Users
            .AnyAsync(user =>
                user.UserId == userId
            );

        if (!userExists)
        {
            throw new UnauthorizedAccessException(
                "Authenticated user was not found."
            );
        }

        var normalizedType = request.Type.Trim();

        if (string.Equals(
                normalizedType,
                "Entry",
                StringComparison.OrdinalIgnoreCase))
        {
            normalizedType = "Entry";

            product.Stock += request.Quantity;
        }
        else if (string.Equals(
                     normalizedType,
                     "Exit",
                     StringComparison.OrdinalIgnoreCase))
        {
            normalizedType = "Exit";

            if (product.Stock < request.Quantity)
            {
                throw new InvalidOperationException(
                    "Insufficient stock for this movement."
                );
            }

            product.Stock -= request.Quantity;
        }
        else
        {
            throw new ArgumentException(
                "Movement type must be Entry or Exit."
            );
        }

        var movement = new InventoryMovement
        {
            ProductId = product.ProductId,
            UserId = userId,
            Type = normalizedType,
            Quantity = request.Quantity,
            StockAfterMovement = product.Stock,
            Date = DateTime.UtcNow
        };

        _context.InventoryMovements.Add(movement);

        if (product.Stock <= product.MinimumStock)
        {
            var notificationRecipients =
                await _context.Users
                    .Where(user =>
                        user.Status == "Active" &&
                        (
                            user.Role.RoleName == "Admin" ||
                            user.Role.RoleName == "Employee"
                        )
                    )
                    .Select(user => user.UserId)
                    .ToListAsync();

            foreach (var recipientUserId
                     in notificationRecipients)
            {
                var notificationExists =
                    await _context.Notifications
                        .AnyAsync(notification =>
                            notification.UserId ==
                                recipientUserId &&
                            !notification.IsRead &&
                            notification.Title ==
                                "Low stock alert" &&
                            notification.Message.Contains(
                                $"'{product.Name}'"
                            )
                        );

                if (notificationExists)
                {
                    continue;
                }

                var notification = new Notification
                {
                    UserId = recipientUserId,
                    Title = "Low stock alert",
                    Message =
                        $"Product '{product.Name}' has " +
                        $"{product.Stock} units available. " +
                        $"Minimum stock: " +
                        $"{product.MinimumStock}.",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
            }
        }

        // Guarda la actualización de stock,
        // el movimiento y las notificaciones.
        await _context.SaveChangesAsync();

        // Registra la operación en AuditLogs.
        if (normalizedType == "Entry")
        {
            await _auditService.CreateLogAsync(
                userId,
                "InventoryEntry",
                "Product",
                product.ProductId,
                $"Added {request.Quantity} units to product " +
                $"'{product.Name}'. Stock after movement: " +
                $"{product.Stock}."
            );
        }
        else
        {
            await _auditService.CreateLogAsync(
                userId,
                "InventoryExit",
                "Product",
                product.ProductId,
                $"Removed {request.Quantity} units from product " +
                $"'{product.Name}'. Stock after movement: " +
                $"{product.Stock}."
            );
        }

        return await GetByIdAsync(movement.MovementId)
            ?? throw new InvalidOperationException(
                "The inventory movement could not be retrieved."
            );
    }
}