using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace SteamAuthTemplate.Api.Controllers;

[ApiController]
[Route("auth/steam")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback))
        }, "Steam");
    }

    [HttpGet("callback")]
    public IActionResult Callback()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();

        var steamId = User.FindFirstValue("steamid");

        // Issue JWT for Angular
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("steamid", steamId ?? "")
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });

        return Redirect($"http://localhost:4200/auth/callback?token={tokenHandler.WriteToken(token)}");
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult GetSteamId()
    {
        var steamId = User.FindFirstValue("steamid");
        if (string.IsNullOrEmpty(steamId))
            return Unauthorized();

        return Ok(new { steamId });
    }
}
