using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Linq;

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

// Console log the claims for debugging
        Console.WriteLine("Authenticated user with Steam ID: " + steamId);
        
        // Issue JWT for Angular
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
               new Claim(ClaimTypes.NameIdentifier, steamId ?? ""),
               new Claim("steamid", steamId ?? "")
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });

        return Redirect($"http://localhost:4200/auth/callback?token={tokenHandler.WriteToken(token)}");
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetMe()
    {
        Console.WriteLine("GetMe called");

        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { error = "Not authenticated" });

        // Print User as JSON to the console using System.Text.Json
        var userInfo = new
        {
            IsAuthenticated = User.Identity.IsAuthenticated,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        };
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(userInfo));

        return Ok(new { steamId = 123123 });

// TODO: Fix this to return the actual Steam ID from the JWT
        // var steamId = User.FindFirstValue("steamid");
        // if (string.IsNullOrEmpty(steamId))
        //     return Unauthorized(new { error = "Steam ID not found" });

        // return Ok(new { steamId });
    }
}
