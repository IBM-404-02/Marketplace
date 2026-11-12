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


app.MapGet("/api/categories", async (MarketplaceDbContext db) =>
{
    var categories = await db.Categories
        .OrderBy(c => c.Name)
        .Select(c => new { c.Id, c.Name })
        .ToListAsync();

    return Results.Json(categories);
});


app.MapGet("/api/products", async (MarketplaceDbContext db,
    string? search, int? categoryId, decimal? minPrice, decimal? maxPrice) =>
{
    var query = db.Products.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
        query = query.Where(p => p.Name.Contains(search));

    if (categoryId != null)
        query = query.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == categoryId));

    if (minPrice != null)
        query = query.Where(p => p.Price >= minPrice);

    if (maxPrice != null)
        query = query.Where(p => p.Price <= maxPrice);

    var products = await query
        .OrderBy(p => p.Name)
        .Select(p => new
        {
            p.Id,
            p.Name,
            p.Price,
            p.ImagePath,
            p.StockQuantity,
            p.IsAvailable,
            CanBuy = p.IsAvailable && p.StockQuantity > 0,
            Categories = p.ProductCategories.Select(pc => pc.Category.Name).ToList(),
            Rating = p.Reviews.Average(r => (double?)r.Rating),
            ReviewsCount = p.Reviews.Count()
        })
        .ToListAsync();

    return Results.Json(products);
});

app.MapGet("/api/products/{id}", async (MarketplaceDbContext db, int id) =>
{
    var product = await db.Products
        .Where(p => p.Id == id)
        .Select(p => new
        {
            p.Id,
            p.Name,
            p.Description,
            p.Price,
            p.ImagePath,
            p.StockQuantity,
            p.PackQuantity,
            p.IsAvailable,
            CanBuy = p.IsAvailable && p.StockQuantity > 0,
            Categories = p.ProductCategories
                .Select(pc => new { pc.Category.Id, pc.Category.Name })
                .ToList(),
            Rating = p.Reviews.Average(r => (double?)r.Rating),
            ReviewsCount = p.Reviews.Count()
        })
        .FirstOrDefaultAsync();

    if (product == null)
        return Results.NotFound(new { message = "Товар не найден" });

    return Results.Json(product);
});
app.MapGet("/api/reviews", async (MarketplaceDbContext db, int productId) =>
{
    var reviews = await db.Reviews
        .Where(r => r.ProductId == productId)
        .OrderByDescending(r => r.CreatedAt)
        .Select(r => new
        {
            r.Id,
            r.Rating,
            r.Text,
            r.CreatedAt,
            UserLogin = r.User.Login
        })
        .ToListAsync();

    return Results.Json(reviews);
});

app.MapPost("/api/reviews", async (MarketplaceDbContext db, ClaimsPrincipal user, ReviewRequest request) =>
{
    int userId = int.Parse(user.FindFirst("sub")!.Value);

    if (request.Rating < 1 || request.Rating > 5)
        return Results.BadRequest(new { message = "Оценка должна быть от 1 до 5" });

    bool productExists = await db.Products.AnyAsync(p => p.Id == request.ProductId);
    if (!productExists)
        return Results.NotFound(new { message = "Товар не найден" });

    bool alreadyReviewed = await db.Reviews
        .AnyAsync(r => r.UserId == userId && r.ProductId == request.ProductId);
    if (alreadyReviewed)
        return Results.Conflict(new { message = "Вы уже оставили отзыв на этот товар" });

    var review = new Review
    {
        UserId = userId,
        ProductId = request.ProductId,
        Rating = (byte)request.Rating,
        Text = request.Text
    };

    db.Reviews.Add(review);
    await db.SaveChangesAsync();

    return Results.Created($"/api/reviews/{review.Id}", new { review.Id, review.Rating, review.Text });
}).RequireAuthorization();

app.MapDelete("/api/reviews/{id}", async (MarketplaceDbContext db, int id) =>
{
    var review = await db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
    if (review == null)
        return Results.NotFound(new { message = "Отзыв не найден" });

    db.Reviews.Remove(review);
    await db.SaveChangesAsync();

    return Results.Json(new { message = "Отзыв удалён" });
}).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));


app.MapPost("/api/products", async (MarketplaceDbContext db, ProductRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { message = "Название обязательно" });

    if (request.Price < 0 || request.StockQuantity < 0)
        return Results.BadRequest(new { message = "Цена и остаток не могут быть отрицательными" });

    var categoryIds = request.CategoryIds.Distinct().ToList();
    int foundCategories = await db.Categories.CountAsync(c => categoryIds.Contains(c.Id));
    if (categoryIds.Count == 0 || foundCategories != categoryIds.Count)
        return Results.BadRequest(new { message = "Укажите существующие категории" });

    var product = new Product
    {
        Name = request.Name,
        Description = request.Description,
        Price = request.Price,
        ImagePath = request.ImagePath,
        StockQuantity = request.StockQuantity,
        PackQuantity = request.PackQuantity,
        IsAvailable = request.IsAvailable
    };

    foreach (var categoryId in categoryIds)
        product.ProductCategories.Add(new ProductCategory { CategoryId = categoryId });

    db.Products.Add(product);
    await db.SaveChangesAsync();

    return Results.Created($"/api/products/{product.Id}", new { product.Id, product.Name });
}).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));

app.MapPut("/api/products", async (MarketplaceDbContext db, ProductRequest request) =>
{
    var product = await db.Products
        .Include(p => p.ProductCategories)
        .FirstOrDefaultAsync(p => p.Id == request.Id);

    if (product == null)
        return Results.NotFound(new { message = "Товар не найден" });

    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { message = "Название обязательно" });

    if (request.Price < 0 || request.StockQuantity < 0)
        return Results.BadRequest(new { message = "Цена и остаток не могут быть отрицательными" });

    var categoryIds = request.CategoryIds.Distinct().ToList();
    int foundCategories = await db.Categories.CountAsync(c => categoryIds.Contains(c.Id));
    if (categoryIds.Count == 0 || foundCategories != categoryIds.Count)
        return Results.BadRequest(new { message = "Укажите существующие категории" });

    product.Name = request.Name;
    product.Description = request.Description;
    product.Price = request.Price;
    product.ImagePath = request.ImagePath;
    product.StockQuantity = request.StockQuantity;
    product.PackQuantity = request.PackQuantity;
    product.IsAvailable = request.IsAvailable;

    var toRemove = product.ProductCategories
        .Where(pc => !categoryIds.Contains(pc.CategoryId))
        .ToList();
    db.ProductCategories.RemoveRange(toRemove);

    foreach (var categoryId in categoryIds)
    {
        if (!product.ProductCategories.Any(pc => pc.CategoryId == categoryId))
            product.ProductCategories.Add(new ProductCategory { CategoryId = categoryId });
    }

    await db.SaveChangesAsync();

    return Results.Json(new { product.Id, product.Name, message = "Товар обновлён" });
}).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));


app.MapPost("/api/categories", async (MarketplaceDbContext db, CategoryRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { message = "Название обязательно" });

    if (await db.Categories.AnyAsync(c => c.Name == request.Name))
        return Results.Conflict(new { message = "Такая категория уже есть" });

    var category = new Category { Name = request.Name };
    db.Categories.Add(category);
    await db.SaveChangesAsync();

    return Results.Created($"/api/categories/{category.Id}", new { category.Id, category.Name });
}).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));

app.MapPut("/api/categories", async (MarketplaceDbContext db, CategoryRequest request) =>
{
    var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == request.Id);
    if (category == null)
        return Results.NotFound(new { message = "Категория не найдена" });

    if (string.IsNullOrWhiteSpace(request.Name))
        return Results.BadRequest(new { message = "Название обязательно" });

    if (await db.Categories.AnyAsync(c => c.Name == request.Name && c.Id != request.Id))
        return Results.Conflict(new { message = "Такая категория уже есть" });

    category.Name = request.Name;
    await db.SaveChangesAsync();

    return Results.Json(new { category.Id, category.Name });
}).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));

app.MapGet("/api/pickup-points", async (MarketplaceDbContext db, bool? all) =>
{
    var query = db.PickupPoints.AsQueryable();

    if (all != true)
        query = query.Where(p => p.IsActive);

    var points = await query
        .Select(p => new { p.Id, p.Address, p.WorkingHours, p.IsActive })
        .ToListAsync();

    return Results.Json(points);
});

app.MapPost("/api/pickup-points", async (MarketplaceDbContext db, PickupPointRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Address))
        return Results.BadRequest(new { message = "Адрес обязателен" });

    var point = new PickupPoint
    {
        Address = request.Address,
        WorkingHours = request.WorkingHours,
        IsActive = true
    };

    db.PickupPoints.Add(point);
    await db.SaveChangesAsync();

    return Results.Created($"/api/pickup-points/{point.Id}", new { point.Id, point.Address });
}).RequireAuthorization(policy => policy.RequireAuthenticatedUser().RequireRole("Admin"));

app.MapPut("/api/pickup-points", async (MarketplaceDbContext db, PickupPointRequest request) =>
{
    var point = await db.PickupPoints.FirstOrDefaultAsync(p => p.Id == request.Id);
    if (point == null)
        return Results.NotFound(new { message = "Пункт выдачи не найден" });

    if (string.IsNullOrWhiteSpace(request.Address))
        return Results.BadRequest(new { message = "Адрес обязателен" });

    point.Address = request.Address;
    point.WorkingHours = request.WorkingHours;
    point.IsActive = request.IsActive;
    await db.SaveChangesAsync();

    return Results.Json(new { point.Id, point.Address, point.IsActive });
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
record ReviewRequest(int ProductId, int Rating, string? Text);
record ProductRequest(int Id, string Name, string? Description, decimal Price, string? ImagePath,
    int StockQuantity, int? PackQuantity, bool IsAvailable, List<int> CategoryIds);
record CategoryRequest(int Id, string Name);
record PickupPointRequest(int Id, string Address, string? WorkingHours, bool IsActive);

