using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TutorFlow.API.DTOs;
using TutorFlow.Infrastructure;
using TutorFlow.Core.Enums;
using TutorFlow.Core.Entities;

namespace TutorFlow.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<ApplicationUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    /// <summary>Register a new user (Tutor, Student, or Parent)</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto dto)
    {
        if (!Enum.TryParse<UserRole>(dto.Role, ignoreCase: true, out var role))
            return BadRequest(new { message = "Invalid role. Use: Tutor, Student, or Parent." });

        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            UserName = dto.Email,
            Role = role
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        var token = GenerateJwt(user);
        return Ok(BuildResponse(user, token));
    }

    /// <summary>Login and receive a JWT token</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid email or password." });

        var token = GenerateJwt(user);
        return Ok(BuildResponse(user, token));
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private string GenerateJwt(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static AuthResponseDto BuildResponse(ApplicationUser user, string token) => new(
        Token: token,
        Email: user.Email!,
        FullName: $"{user.FirstName} {user.LastName}",
        Role: user.Role.ToString(),
        Expiry: DateTime.UtcNow.AddDays(7)
    );
}
