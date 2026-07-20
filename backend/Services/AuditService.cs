using backend.Data;
using backend.DTOs.Audit;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateLogAsync(
        int userId,
        string action,
        string entityName,
        int entityId,
        string description)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);

        await _context.SaveChangesAsync();
        
        
    }

    public async Task<List<AuditLogResponseDto>> GetAllAsync()
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(log => log.CreatedAt)
            .Select(log => new AuditLogResponseDto
            {
                AuditLogId = log.AuditLogId,
                UserId = log.UserId,
                UserName = log.User.Name,
                Action = log.Action,
                EntityName = log.EntityName,
                EntityId = log.EntityId,
                Description = log.Description,
                CreatedAt = log.CreatedAt
            })
            .ToListAsync();
    }
}