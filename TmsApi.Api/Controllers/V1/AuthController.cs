using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Services;
using TmsApi.Infrastructure.Persistence;

namespace TmsApi.Api.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<TmsUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly TokenService _tokenService;
    private readonly TmsDbContext _context;
    public AuthController(
        UserManager<TmsUser> userManager,
        RoleManager<IdentityRole> roleManager,
        TokenService tokenService,
        TmsDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _context = context;
    }

    public record RegisterRequest(
        string Email,
        string Password,
        string FirstName,
        string LastName,
        string Role);

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        var existingUser =
            await _userManager.FindByEmailAsync(request.Email);

        if (existingUser != null)
        {
            // Prevent account enumeration by returning
            // a generic response.
            return Ok(
                new
                {
                    message = "Registration request received."
                });
        }

        var user = new TmsUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        var result = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .Select(e => e.Description);

            return BadRequest(
                new
                {
                    errors
                });
        }

        // Ensure requested role exists.
        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(
                new IdentityRole(request.Role));
        }

        await _userManager.AddToRoleAsync(
            user,
            request.Role);

        return Ok(
            new
            {
                message = "Registration successful."
            });
    }

    public record LoginRequest(
        string Email,
        string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        var user =
            await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Unauthorized(
                new
                {
                    detail = "Invalid credentials."
                });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(
                StatusCodes.Status423Locked,
                new
                {
                    detail =
                        "Account locked due to multiple failed login attempts. Try again in 15 minutes."
                });
        }

        var validPassword =
            await _userManager.CheckPasswordAsync(
                user,
                request.Password);

        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);

            return Unauthorized(
                new
                {
                    detail = "Invalid credentials."
                });
        }

        // Reset failed attempt counter on successful login.
            await _userManager.ResetAccessFailedCountAsync(user);
            // Get the user's roles for JWT claims.
            var roles = await _userManager.GetRolesAsync(user);
            // Generate the short-lived access token.
            var accessToken = _tokenService.GenerateJwt(user, roles);
            // Issue the initial refresh token.
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString("N"),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                accessToken,
                refreshToken = refreshToken.Token
            });

    }

    public record RefreshRequest(string RefreshToken);

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken);

        if (storedToken == null)
        {
            return Unauthorized(new
            {
                detail = "Invalid refresh token."
            });
        }

        // Token theft detection:
        // A refresh token can only be used once.
        // If an already-used token is submitted again,
        // revoke every refresh token belonging to that user.
        if (storedToken.IsUsed)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId)
                .ToListAsync();

            foreach (var token in userTokens)
            {
                token.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            return Unauthorized(new
            {
                detail =
                    "Token theft detected. All user sessions revoked."
            });
        }

        // Reject expired or explicitly revoked tokens.
        if (storedToken.IsRevoked ||
            storedToken.ExpiresAt < DateTime.UtcNow)
        {
            return Unauthorized(new
            {
                detail = "Refresh token expired or revoked."
            });
        }

        // Mark the current refresh token as used.
        storedToken.IsUsed = true;

        // Create a brand-new refresh token.
        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);

        // Get the user associated with the refresh token.
        var user = await _userManager.FindByIdAsync(
            storedToken.UserId);

        if (user == null)
        {
            return Unauthorized(new
            {
                detail = "User associated with refresh token was not found."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        // Generate a new access token.
        var newAccessToken =
            _tokenService.GenerateJwt(user, roles);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken.Token
        });
    }

}