using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json.Serialization;
using MarketplaceApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddDbContext<MarketplaceDbContext>();
var hasher = new PasswordHasher<User>();
var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));

var validation = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "MarketplaceApi",

    ValidateAudience = true,
    ValidAudience = "MarketplaceClient",

    ValidateIssuerSigningKey = true,
    IssuerSigningKey = key,

    ValidateLifetime = true,
    ClockSkew = TimeSpan.Zero,

    NameClaimType = "name",
    RoleClaimType = "role"
};

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = validation;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/register", async (MarketplaceDbContext db, RegisterRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
        return Results.BadRequest(new { message = "Логин и пароль обязательны" });

    var existing = await db.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Login == request.Login);

    if (existing != null)
    {
        var check = hasher.VerifyHashedPassword(existing, existing.PasswordHash, request.Password);

        if (check == PasswordVerificationResult.Failed)
            return Results.Conflict(new { message = "Этот логин уже занят" });

        return Results.Ok(new
        {
            access_token = CreateToken(existing, existing.Role.Name),
            token_type = "Bearer"
        });
    }

    if (request.Password != request.ConfirmPassword)
        return Results.BadRequest(new { message = "Пароли не совпадают" });

    var userRole = await db.Roles.FirstAsync(r => r.Name == "User");

    var user = new User
    {
        Login = request.Login,
        RoleId = userRole.Id
    };
    user.PasswordHash = hasher.HashPassword(user, request.Password);

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created("/api/me", new
    {
        access_token = CreateToken(user, userRole.Name),
        token_type = "Bearer"
    });
}).AllowAnonymous();

app.MapPost("/api/auth/login", async (MarketplaceDbContext db, LoginRequest request) =>
{
    var user = await db.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Login == request.Login);

    if (user == null || string.IsNullOrWhiteSpace(request.Password))
        return Results.Challenge();

    var result = hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

    if (result == PasswordVerificationResult.Failed)
        return Results.Challenge();

    return Results.Ok(new
    {
        access_token = CreateToken(user, user.Role.Name),
        token_type = "Bearer"
    });
}).AllowAnonymous();


app.MapGet("/api/me", (ClaimsPrincipal user) =>
{
    return Results.Ok(new
    {
        id = user.FindFirst("sub")?.Value,
        login = user.Identity?.Name,
        role = user.FindFirst("role")?.Value
    });
}).RequireAuthorization();

app.MapGet("/api/admin/check", () =>
{
    return Results.Ok(new { message = "Вы администратор" });
}).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));

app.Run();


string CreateToken(User user, string role)
{
    var claims = new[]
    {
        new Claim("sub", user.Id.ToString()),
        new Claim("name", user.Login),
        new Claim("role", role)
    };

    var token = new JwtSecurityToken(
        issuer: "MarketplaceApi",
        audience: "MarketplaceClient",
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

    return new JwtSecurityTokenHandler().WriteToken(token);
}


record RegisterRequest(string Login, string Password, string ConfirmPassword);
record LoginRequest(string Login, string Password);