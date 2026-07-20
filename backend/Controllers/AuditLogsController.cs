using backend.DTOs.Audit;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AuditLogsController : ControllerBase
{
    private readonly AuditService _auditService;

    public AuditLogsController(
        AuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuditLogResponseDto>>>
        GetAll()
    {
        var logs = await _auditService.GetAllAsync();

        return Ok(logs);
    }
}