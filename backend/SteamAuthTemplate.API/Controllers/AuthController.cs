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
        // Try to get the authentication result for the Steam scheme first.
        // This ensures we read the claims issued by the Steam handler even if the cookie
        // authentication hasn't been populated to HttpContext.User yet.
        var authResult = await HttpContext.AuthenticateAsync("Steam");
        var principal = authResult?.Principal ?? User;

        if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
            return Unauthorized();

        // Try multiple claim names to find the Steam ID (some handlers use NameIdentifier)
        var steamId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? principal.FindFirstValue("steamid")
                      ?? principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
                      ?? principal.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("steam"))?.Value;

        // Use console logging for knowing who has the steam id
        Console.WriteLine("1", principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Console.WriteLine("2", principal.FindFirstValue("steamid"));
        Console.WriteLine("3", principal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"));
        Console.WriteLine("4", principal.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("steam"))?.Value);
        Console.WriteLine("Extracted Steam ID: " + steamId);



        // Normalize steam id: some OpenID responses return the full profile URL, extract numeric id
        if (!string.IsNullOrEmpty(steamId))
        {
            var match = Regex.Match(steamId, @"\d{17,20}$");
            if (match.Success)
                steamId = match.Value;
        }

        // Log all claims if steamId is not found for easier debugging
        if (string.IsNullOrEmpty(steamId))
        {
            var claimsDebug = principal.Claims.Select(c => new { c.Type, c.Value });
            Console.WriteLine("Steam ID not found in claims. Claims: " + System.Text.Json.JsonSerializer.Serialize(claimsDebug));
            return Unauthorized(new { error = "Steam ID not found in authentication result" });
        }

        Console.WriteLine("Authenticated user with Steam ID: " + steamId);

        // Issue JWT for Angular
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
        var frontend = _config["Frontend:Url"] ?? "http://localhost:4200";

        return Redirect($"{frontend.TrimEnd('/')}/auth/callback?token={encoded}");
    }

    [HttpGet("me")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public IActionResult GetMe()
    {
        Console.WriteLine("GetMe called");

        if (User?.Identity == null || !User.Identity.IsAuthenticated)
            return Unauthorized(new { error = "Not authenticated" });

        // Attempt to read steam id from JWT claims
        var steamId = User.FindFirstValue("steamid")
                      ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? User.Claims.FirstOrDefault(c => c.Type.ToLower().Contains("steam"))?.Value;

        // Normalize steam id in case it contains the OpenID URL
        if (!string.IsNullOrEmpty(steamId))
        {
            var match = Regex.Match(steamId, @"\d{17,20}$");
            if (match.Success)
                steamId = match.Value;
        }

        if (string.IsNullOrEmpty(steamId))
            return Unauthorized(new { error = "Steam ID not found" });

        // Print User as JSON to the console using System.Text.Json
        var userInfo = new
        {
            IsAuthenticated = User.Identity.IsAuthenticated,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        };
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(userInfo));

        return Ok(new { steamId });
    }
}
