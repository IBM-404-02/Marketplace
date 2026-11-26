using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MarketplaceSWAGAwpf.Models;

//Классы для перевода данных из json в cs. Они точно повторяют структуру jsonов
public class LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "";
}

public class MeDto
{
    public string? Id { get; set; }
    public string? Login { get; set; }
    public string? Role { get; set; }
}

public class MessageDto
{
    public string? Message { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public bool CanBuy { get; set; }
    public List<string> Categories { get; set; } = new();
    public double? Rating { get; set; }
    public int ReviewsCount { get; set; }

    public string CategoriesText => string.Join(", ", Categories);

    public string RatingText => Rating == null ? "Нет оценок" : $"★ {Rating:0.0} ({ReviewsCount} отз.)";

    public string StockText => !IsAvailable ? "Недоступен для покупки" : StockQuantity == 0 ? "Нет в наличии" : $"В наличии: {StockQuantity} шт.";
}

public class ProductDetailsDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? ImagePath { get; set; }
    public int StockQuantity { get; set; }
    public int? PackQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public bool CanBuy { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
    public double? Rating { get; set; }
    public int ReviewsCount { get; set; }

    public string CategoriesText => string.Join(", ", Categories.Select(c => c.Name));

    public string RatingText => Rating == null ? "Нет оценок" : $"★ {Rating:0.0} ({ReviewsCount} отз.)";

    public string StockText => !IsAvailable ? "Недоступен для покупки" : StockQuantity == 0 ? "Нет в наличии" : $"В наличии: {StockQuantity} шт.";

    public string PackText => PackQuantity == null ? "" : $"В упаковке: {PackQuantity} шт.";
}

public class ReviewDto
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string? Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserLogin { get; set; } = "";

    [JsonIgnore]
    public bool CanDelete { get; set; }

    public string Stars => new string('★', Rating) + new string('☆', 5 - Rating);
}

public class CartDto
{
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ImagePath { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Sum { get; set; }
    public bool CanBuy { get; set; }

    public bool CannotBuy => !CanBuy;
}

public class FavoriteDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ImagePath { get; set; }
    public decimal Price { get; set; }
    public bool CanBuy { get; set; }
    public DateTime AddedAt { get; set; }

    public bool CannotBuy => !CanBuy;
}

public class PickupPointDto
{
    public int Id { get; set; }
    public string Address { get; set; } = "";
    public string? WorkingHours { get; set; }
    public bool IsActive { get; set; }

    public string Display => string.IsNullOrWhiteSpace(WorkingHours) ? Address : $"{Address} ({WorkingHours})";

    public string AdminDisplay => IsActive ? Display : $"{Display} — выключен";
}

public class DeliveryMethodDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class StatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string UserLogin { get; set; } = "";
    public string Status { get; set; } = "";
    public string DeliveryMethod { get; set; } = "";
    public int ItemsCount { get; set; }
    public decimal Total { get; set; }

    public string Title => $"Заказ №{Id} от {OrderDate:dd.MM.yyyy HH:mm}";
    public string Subtitle => $"{Status} · {ItemsCount} шт. · {Total:N2} ₽";
}

public class OrderDetailsDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string UserLogin { get; set; } = "";
    public string Status { get; set; } = "";
    public string DeliveryMethod { get; set; } = "";
    public string? PickupPoint { get; set; }
    public string? DeliveryAddress { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }

    public string Title => $"Заказ №{Id} от {OrderDate:dd.MM.yyyy HH:mm}";
    public string CustomerText => $"Покупатель: {UserLogin}";
    public string StatusText => $"Статус: {Status}";

    public string DeliveryText => PickupPoint != null ? $"{DeliveryMethod}: {PickupPoint}" : $"{DeliveryMethod}: {DeliveryAddress}";

    public string TotalText => $"Итого: {Total:N2} ₽";
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public decimal Sum { get; set; }
}

public class OrderCreatedDto
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public string? Message { get; set; }
}
