using AspNet.Security.OpenId.Steam;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services
    .AddAuthentication(options =>
    {
        // Keep Cookie as the default for the Steam challenge flow, but register JWT for API calls
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = SteamAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddSteam()
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        // Validate JWT tokens issued by the Callback endpoint
        var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty);

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        // If you need to accept tokens via query string for specific scenarios, enable below.
        // options.Events = new JwtBearerEvents
        // {
        //     OnMessageReceived = context =>
        //     {
        //         var accessToken = context.Request.Query["access_token"];
        //         if (!string.IsNullOrEmpty(accessToken))
        //             context.Token = accessToken;
        //         return Task.CompletedTask;
        //     }
        // };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("AllowAngularDev");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
