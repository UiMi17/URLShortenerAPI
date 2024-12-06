using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using URLShortenerAPI.Data;
using URLShortenerAPI.Models;
using URLShortenerAPI.Services;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtService _jwtService;
    private readonly AppDbContext _context;

    public AuthController(UserService userService, JwtService jwtService, AppDbContext dbcontext)
    {
        _userService = userService;
        _jwtService = jwtService;
        _context = dbcontext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!await _userService.RegisterAsync(model.Email, model.Username, model.Password))
            return BadRequest("User with the same email already exists.");

        return Created("", new { Message = "Registration successful." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userService.AuthenticateAsync(model.Email, model.Password);
        if (user == null)
            return Unauthorized("Invalid email or password.");

        var token = _jwtService.GenerateToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        await _userService.SaveRefreshTokenAsync(user.Id, refreshToken);

        return Ok(new { user, Token = token, RefreshToken = refreshToken });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); ;
        if (userId == null)
            return Unauthorized("User not authorized.");

        await _userService.RemoveRefreshTokenAsync(int.Parse(userId.ToString()));

        return Ok(new { Message = "Logged out successfully." });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenModel model)
    {
        var userId = await _userService.ValidateRefreshTokenAsync(model.RefreshToken);
        if (userId == null)
            return Unauthorized("Invalid refresh token.");

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
            return Unauthorized("User not found.");

        var token = _jwtService.GenerateToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        await _userService.SaveRefreshTokenAsync(user.Id, newRefreshToken);

        return Ok(new { Token = token, RefreshToken = newRefreshToken });
    }
}
