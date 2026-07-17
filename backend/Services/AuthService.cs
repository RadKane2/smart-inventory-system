using backend.Data;
using backend.DTOs;
using backend.Entities;
using backend.Security;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtService _jwtService;
    private readonly IConfiguration _configuration;

    public AuthService(
        ApplicationDbContext context,
        JwtService jwtService,
        IConfiguration configuration)
    {
        _context = context;
        _jwtService = jwtService;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> RegisterAsync(
        RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _context.Users
            .AnyAsync(user => user.Email == normalizedEmail);

        if (emailExists)
        {
            throw new InvalidOperationException(
                "A user with this email already exists."
            );
        }

        var customerRole = await _context.Roles
            .FirstOrDefaultAsync(role => role.RoleName == "Customer");

        if (customerRole is null)
        {
            throw new InvalidOperationException(
                "The Customer role was not found."
            );
        }

        var user = new User
        {
            Name = request.Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password
            ),
            RoleId = customerRole.RoleId,
            Role = customerRole,
            Status = "Active",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return CreateLoginResponse(user);
    }

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(user => user.Role)
            .FirstOrDefaultAsync(
                user => user.Email == normalizedEmail
            );

        if (user is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password."
            );
        }

        var passwordIsValid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash
        );

        if (!passwordIsValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password."
            );
        }

        if (!string.Equals(
                user.Status,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException(
                "The user account is not active."
            );
        }

        user.LastLogin = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return CreateLoginResponse(user);
    }

    private LoginResponseDto CreateLoginResponse(User user)
    {
        var expirationMinutes =
            _configuration.GetValue<int>(
                "Jwt:ExpirationMinutes"
            );

        if (expirationMinutes <= 0)
        {
            expirationMinutes = 60;
        }

        return new LoginResponseDto
        {
            Token = _jwtService.GenerateToken(user),
            Expiration = DateTime.UtcNow.AddMinutes(
                expirationMinutes
            ),
            UserId = user.UserId,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role.RoleName
        };
    }
}