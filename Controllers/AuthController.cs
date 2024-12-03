using Microsoft.AspNetCore.Mvc;
using URLShortenerAPI.Models;
using URLShortenerAPI.Services;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtService _jwtService;

    public AuthController(UserService userService, JwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        if (!await _userService.RegisterAsync(model.Email, model.Username, model.Password))
            return BadRequest("User with the same email or username already exists.");

        return Created("", new { Message = "Registration successful." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        var user = await _userService.AuthenticateAsync(model.Email, model.Password);
        if (user == null)
            return Unauthorized("Invalid username or password.");

        var token = _jwtService.GenerateToken(user);
        return Ok(new { Token = token });
    }
}
