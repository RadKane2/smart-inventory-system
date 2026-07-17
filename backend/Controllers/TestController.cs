using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [Authorize]
    [HttpGet("authenticated")]
    public IActionResult Authenticated()
    {
        return Ok(new
        {
            message = "The JWT is valid.",
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            ),
            name = User.FindFirstValue(
                ClaimTypes.Name
            ),
            email = User.FindFirstValue(
                ClaimTypes.Email
            ),
            role = User.FindFirstValue(
                ClaimTypes.Role
            )
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            message = "You have Admin access."
        });
    }

    [Authorize(Roles = "Customer")]
    [HttpGet("customer")]
    public IActionResult CustomerOnly()
    {
        return Ok(new
        {
            message = "You have Customer access."
        });
    }
}