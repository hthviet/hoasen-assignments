using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PT_WEB.Data;
using PT_WEB.Models;

namespace PT_WEB.Controllers.Api;

[ApiController]
[Route("api/auth")]
public class AuthApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _config;
    private readonly PasswordHasher<UserAccount> _hasher;

    public AuthApiController(ApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        _hasher = new PasswordHasher<UserAccount>();
    }

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string FullName, string Email, string Password);

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _context.UserAccounts.FirstOrDefaultAsync(u => u.Email == req.Email);
        if (user is null)
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
        if (result == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng." });

        return Ok(new { token = GenerateToken(user), userId = user.Id, fullName = user.FullName, email = user.Email });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _context.UserAccounts.AnyAsync(u => u.Email == req.Email))
            return BadRequest(new { message = "Email đã tồn tại." });

        var user = new UserAccount
        {
            FullName = req.FullName,
            Email = req.Email,
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, req.Password);
        _context.UserAccounts.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new { token = GenerateToken(user), userId = user.Id, fullName = user.FullName, email = user.Email });
    }

    private string GenerateToken(UserAccount user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
