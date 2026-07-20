using backend.Data;
using backend.DTOs.Orders;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class OrderService
{
    private readonly ApplicationDbContext _context;
    private readonly AuditService _auditService;

    public OrderService(
        ApplicationDbContext context,
        AuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<OrderResponseDto>> GetAllAsync()
    {
        return await _context.Orders
            .AsNoTracking()
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new OrderResponseDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                CustomerName = order.User.Name,
                Total = order.Total,
                Status = order.Status,
                CreatedAt = order.CreatedAt,

                Items = order.OrderDetails
                    .Select(detail => new OrderDetailResponseDto
                    {
                        OrderDetailId = detail.OrderDetailId,
                        ProductId = detail.ProductId,
                        ProductName = detail.Product.Name,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        Subtotal =
                            detail.Quantity * detail.UnitPrice
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<List<OrderResponseDto>> GetByUserAsync(
        int userId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAt)
            .Select(order => new OrderResponseDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                CustomerName = order.User.Name,
                Total = order.Total,
                Status = order.Status,
                CreatedAt = order.CreatedAt,

                Items = order.OrderDetails
                    .Select(detail => new OrderDetailResponseDto
                    {
                        OrderDetailId = detail.OrderDetailId,
                        ProductId = detail.ProductId,
                        ProductName = detail.Product.Name,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        Subtotal =
                            detail.Quantity * detail.UnitPrice
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<OrderResponseDto?> GetByIdAsync(
        int orderId,
        int userId,
        bool isAdmin)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(order => order.OrderId == orderId);

        if (!isAdmin)
        {
            query = query.Where(order =>
                order.UserId == userId
            );
        }

        return await query
            .Select(order => new OrderResponseDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                CustomerName = order.User.Name,
                Total = order.Total,
                Status = order.Status,
                CreatedAt = order.CreatedAt,

                Items = order.OrderDetails
                    .Select(detail => new OrderDetailResponseDto
                    {
                        OrderDetailId = detail.OrderDetailId,
                        ProductId = detail.ProductId,
                        ProductName = detail.Product.Name,
                        Quantity = detail.Quantity,
                        UnitPrice = detail.UnitPrice,
                        Subtotal =
                            detail.Quantity * detail.UnitPrice
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<OrderResponseDto> CreateAsync(
        CreateOrderDto request,
        int userId)
    {
        if (request.Items is null ||
            request.Items.Count == 0)
        {
            throw new ArgumentException(
                "The order must contain at least one product."
            );
        }

        var customer = await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user =>
                user.UserId == userId
            );

        if (customer is null)
        {
            throw new UnauthorizedAccessException(
                "The authenticated user was not found."
            );
        }

        if (!string.Equals(
                customer.Status,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The user account is not active."
            );
        }

        /*
         * Si un producto aparece varias veces en la solicitud,
         * las cantidades se agrupan.
         */
        var groupedItems = request.Items
            .GroupBy(item => item.ProductId)
            .Select(group => new
            {
                ProductId = group.Key,
                Quantity = group.Sum(item => item.Quantity)
            })
            .ToList();

        if (groupedItems.Any(item =>
                item.ProductId <= 0 ||
                item.Quantity <= 0))
        {
            throw new ArgumentException(
                "All product IDs and quantities must be greater than zero."
            );
        }

        var productIds = groupedItems
            .Select(item => item.ProductId)
            .ToList();

        var products = await _context.Products
            .Where(product =>
                productIds.Contains(product.ProductId)
            )
            .ToListAsync();

        var foundProductIds = products
            .Select(product => product.ProductId)
            .ToList();

        var missingProductIds = productIds
            .Except(foundProductIds)
            .ToList();

        if (missingProductIds.Count > 0)
        {
            throw new KeyNotFoundException(
                "One or more selected products were not found. " +
                $"Product IDs: {string.Join(", ", missingProductIds)}."
            );
        }

        foreach (var item in groupedItems)
        {
            var product = products.First(product =>
                product.ProductId == item.ProductId
            );

            if (!string.Equals(
                    product.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"Product '{product.Name}' is not active."
                );
            }

            if (product.Stock < item.Quantity)
            {
                throw new InvalidOperationException(
                    $"Insufficient stock for product " +
                    $"'{product.Name}'. Available: " +
                    $"{product.Stock}. Requested: " +
                    $"{item.Quantity}."
                );
            }
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var order = new Order
            {
                UserId = userId,
                Total = 0,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            decimal orderTotal = 0;

            foreach (var item in groupedItems)
            {
                var product = products.First(product =>
                    product.ProductId == item.ProductId
                );

                var subtotal =
                    product.Price * item.Quantity;

                orderTotal += subtotal;

                var orderDetail = new OrderDetail
                {
                    ProductId = product.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                order.OrderDetails.Add(orderDetail);

                product.Stock -= item.Quantity;

                var inventoryMovement =
                    new InventoryMovement
                    {
                        ProductId = product.ProductId,
                        UserId = userId,
                        Type = "Exit",
                        Quantity = item.Quantity,
                        StockAfterMovement = product.Stock,
                        Date = DateTime.UtcNow
                    };

                _context.InventoryMovements.Add(
                    inventoryMovement
                );
            }

            order.Total = orderTotal;

            _context.Orders.Add(order);

            await CreateLowStockNotificationsAsync(products);

            /*
             * Guarda:
             * - La orden
             * - Los detalles
             * - Los movimientos
             * - Los cambios de stock
             * - Las notificaciones
             */
            await _context.SaveChangesAsync();

            await _auditService.CreateLogAsync(
                userId,
                "Create",
                "Order",
                order.OrderId,
                $"Created order #{order.OrderId} " +
                $"with {groupedItems.Count} different products. " +
                $"Total: {order.Total:F2}."
            );

            await transaction.CommitAsync();

            return await GetByIdInternalAsync(order.OrderId)
                ?? throw new InvalidOperationException(
                    "The created order could not be retrieved."
                );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<OrderResponseDto?> UpdateStatusAsync(
        int orderId,
        UpdateOrderStatusDto request,
        int userId)
    {
        var order = await _context.Orders
            .Include(order => order.OrderDetails)
            .ThenInclude(detail => detail.Product)
            .FirstOrDefaultAsync(order =>
                order.OrderId == orderId
            );

        if (order is null)
        {
            return null;
        }

        var requestedStatus = NormalizeStatus(request.Status);

        ValidateStatusTransition(
            order.Status,
            requestedStatus
        );

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            if (requestedStatus == "Cancelled")
            {
                foreach (var detail in order.OrderDetails)
                {
                    detail.Product.Stock += detail.Quantity;

                    var movement = new InventoryMovement
                    {
                        ProductId = detail.ProductId,
                        UserId = userId,
                        Type = "Entry",
                        Quantity = detail.Quantity,
                        StockAfterMovement =
                            detail.Product.Stock,
                        Date = DateTime.UtcNow
                    };

                    _context.InventoryMovements.Add(movement);
                }
            }

            var previousStatus = order.Status;

            order.Status = requestedStatus;

            await _context.SaveChangesAsync();

            await _auditService.CreateLogAsync(
                userId,
                "UpdateStatus",
                "Order",
                order.OrderId,
                $"Changed order #{order.OrderId} status " +
                $"from '{previousStatus}' to " +
                $"'{requestedStatus}'."
            );

            await transaction.CommitAsync();

            return await GetByIdInternalAsync(order.OrderId)
                ?? throw new InvalidOperationException(
                    "The updated order could not be retrieved."
                );
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static string NormalizeStatus(string status)
    {
        var normalizedStatus = status
            .Trim()
            .ToLowerInvariant();

        return normalizedStatus switch
        {
            "pending" => "Pending",
            "processing" => "Processing",
            "shipped" => "Shipped",
            "delivered" => "Delivered",
            "cancelled" => "Cancelled",

            _ => throw new ArgumentException(
                "The order status is invalid."
            )
        };
    }

    private static void ValidateStatusTransition(
        string currentStatus,
        string requestedStatus)
    {
        if (currentStatus == requestedStatus)
        {
            throw new InvalidOperationException(
                $"The order is already in status " +
                $"'{currentStatus}'."
            );
        }

        var transitionIsValid =
            currentStatus switch
            {
                "Pending" =>
                    requestedStatus is
                        "Processing" or
                        "Cancelled",

                "Processing" =>
                    requestedStatus is
                        "Shipped" or
                        "Cancelled",

                "Shipped" =>
                    requestedStatus == "Delivered",

                "Delivered" => false,

                "Cancelled" => false,

                _ => false
            };

        if (!transitionIsValid)
        {
            throw new InvalidOperationException(
                $"The order cannot change from " +
                $"'{currentStatus}' to " +
                $"'{requestedStatus}'."
            );
        }
    }

    private async Task CreateLowStockNotificationsAsync(
        List<Product> products)
    {
        var lowStockProducts = products
            .Where(product =>
                product.Stock <= product.MinimumStock
            )
            .ToList();

        if (lowStockProducts.Count == 0)
        {
            return;
        }

        var notificationRecipientIds =
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

        foreach (var product in lowStockProducts)
        {
            foreach (var recipientUserId
                     in notificationRecipientIds)
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
    }

    private async Task<OrderResponseDto?>
        GetByIdInternalAsync(int orderId)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(order =>
                order.OrderId == orderId
            )
            .Select(order => new OrderResponseDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                CustomerName = order.User.Name,
                Total = order.Total,
                Status = order.Status,
                CreatedAt = order.CreatedAt,

                Items = order.OrderDetails
                    .Select(detail =>
                        new OrderDetailResponseDto
                        {
                            OrderDetailId =
                                detail.OrderDetailId,

                            ProductId =
                                detail.ProductId,

                            ProductName =
                                detail.Product.Name,

                            Quantity =
                                detail.Quantity,

                            UnitPrice =
                                detail.UnitPrice,

                            Subtotal =
                                detail.Quantity *
                                detail.UnitPrice
                        })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }
}