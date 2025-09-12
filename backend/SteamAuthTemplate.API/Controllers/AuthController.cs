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
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
    public async Task<IActionResult> Callback()
    {
        var authResult = await HttpContext.AuthenticateAsync("Steam");
        var principal = authResult?.Principal ?? User;

        if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
            return Unauthorized();

        var steamURL = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if(string.IsNullOrEmpty(steamURL))
            return Unauthorized(new { error = "Steam authentication failed: No Steam ID found" });

        string steamId = null;
        if (!string.IsNullOrEmpty(steamURL))
        {
            var match = Regex.Match(steamURL, @"\d{17,20}$");
            if (match.Success)
                steamId = match.Value;
        }

        if (string.IsNullOrEmpty(steamId))
            return Unauthorized(new { error = "Steam authentication failed: Invalid Steam ID format" });


        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);

        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
               new Claim(ClaimTypes.NameIdentifier, steamId),
               new Claim("steamid", steamId)
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        });

        var jwt = tokenHandler.WriteToken(token);
        var encoded = Uri.EscapeDataString(jwt);
        var frontend = _config["Frontend:Url"] ?? "http://localhost:4200"; // Default URL of Angular app for local dev

        return Redirect($"{frontend.TrimEnd('/')}/auth/callback?token={encoded}");
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetMe()
    {

        if (User?.Identity == null || !User.Identity.IsAuthenticated)
            return Unauthorized(new { error = "Not authenticated" });

        var steamId = User.FindFirstValue("steamid");


        if (string.IsNullOrEmpty(steamId))
            return Unauthorized(new { error = "Steam ID not found" });


        return Ok(new { steamId });
    }
}
