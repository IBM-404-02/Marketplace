using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MarketplaceSWAGAwpf.Models;

namespace MarketplaceSWAGAwpf
{
    public static class Core
    {

        public static string ApiUrl = "http://localhost:5299/api";

        private static readonly HttpClient _client = new HttpClient();

        public static MeDto? CurrentUser { get; private set; }
        public static string? Token { get; private set; }

        public static bool IsAuthorized => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.Role == "Admin";

        static Core()
        {
            _client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static void SetToken(string token)
        {
            Token = token;
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private static void ClearAuth()
        {
            Token = null;
            CurrentUser = null;
            _client.DefaultRequestHeaders.Authorization = null;
        }

        private static async Task<T?> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(ApiUrl + url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private static async Task<T?> PostAsync<T>(string url, object? body = null)
        {
            var json = body == null ? "" : JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(ApiUrl + url, content);
            response.EnsureSuccessStatusCode();
            var resultJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(resultJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private static async Task<T?> PutAsync<T>(string url, object? body = null)
        {
            var json = body == null ? "" : JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(ApiUrl + url, content);
            response.EnsureSuccessStatusCode();
            var resultJson = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(resultJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private static async Task DeleteAsync(string url)
        {
            var response = await _client.DeleteAsync(ApiUrl + url);
            response.EnsureSuccessStatusCode();
        }

        public static async Task<bool> LoginAsync(string login, string password)
        {
            try
            {
                var result = await PostAsync<LoginResponse>("/auth/login",
                    new { login, password });

                if (result != null && !string.IsNullOrEmpty(result.AccessToken))
                {
                    SetToken(result.AccessToken);
                    CurrentUser = await GetAsync<MeDto>("/me");
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> RegisterAsync(string login, string password, string confirmPassword)
        {
            try
            {
                await PostAsync<LoginResponse>("/auth/register",
                    new { login, password, confirmPassword });
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void Logout()
        {
            ClearAuth();
        }

        public static async Task<List<ProductDto>> GetProductsAsync(
            string? search = null, int? categoryId = null,
            decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search))
                query.Add($"search={Uri.EscapeDataString(search)}");
            if (categoryId.HasValue)
                query.Add($"categoryId={categoryId.Value}");
            if (minPrice.HasValue)
                query.Add($"minPrice={minPrice.Value}");
            if (maxPrice.HasValue)
                query.Add($"maxPrice={maxPrice.Value}");

            var url = "/products" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await GetAsync<List<ProductDto>>(url) ?? new List<ProductDto>();
        }

        public static async Task<ProductDetailsDto?> GetProductAsync(int id)
        {
            return await GetAsync<ProductDetailsDto>($"/products/{id}");
        }

        public static async Task<CartDto?> GetCartAsync()
        {
            return await GetAsync<CartDto>("/cart");
        }

        public static async Task AddToCartAsync(int productId, int quantity = 1)
        {
            await PostAsync<CartItemDto>("/cart", new { productId, quantity });
        }

        public static async Task UpdateCartItemAsync(int itemId, int quantity)
        {
            await PutAsync<CartItemDto>("/cart", new { id = itemId, quantity });
        }

        public static async Task RemoveFromCartAsync(int itemId)
        {
            await DeleteAsync($"/cart/{itemId}");
        }

        public static async Task<List<FavoriteDto>> GetFavoritesAsync()
        {
            return await GetAsync<List<FavoriteDto>>("/favorites") ?? new List<FavoriteDto>();
        }

        public static async Task AddToFavoritesAsync(int productId)
        {
            await PostAsync<FavoriteDto>("/favorites", new { productId });
        }

        public static async Task RemoveFromFavoritesAsync(int favoriteId)
        {
            await DeleteAsync($"/favorites/{favoriteId}");
        }

        public static async Task<OrderCreatedDto?> CreateOrderAsync(
            int deliveryMethodId, int? pickupPointId = null, string? address = null,
            int? productId = null, int? quantity = null)
        {
            return await PostAsync<OrderCreatedDto>("/orders",
                new { deliveryMethodId, pickupPointId, deliveryAddress = address, productId, quantity });
        }

        public static async Task<List<OrderDto>> GetOrdersAsync(bool all = false, bool sortAsc = false)
        {
            var query = new List<string>();
            if (all) query.Add("all=true");
            if (sortAsc) query.Add("sort=asc");
            var url = "/orders" + (query.Count > 0 ? "?" + string.Join("&", query) : "");
            return await GetAsync<List<OrderDto>>(url) ?? new List<OrderDto>();
        }

        public static async Task<OrderDetailsDto?> GetOrderAsync(int id)
        {
            return await GetAsync<OrderDetailsDto>($"/orders/{id}");
        }

        public static async Task UpdateOrderStatusAsync(int orderId, int statusId)
        {
            await PutAsync<object>("/orders", new { id = orderId, statusId });
        }

        public static async Task<List<ReviewDto>> GetReviewsAsync(int productId)
        {
            return await GetAsync<List<ReviewDto>>($"/reviews?productId={productId}") ?? new List<ReviewDto>();
        }

        public static async Task AddReviewAsync(int productId, int rating, string? text)
        {
            await PostAsync<ReviewDto>("/reviews", new { productId, rating, text });
        }

        public static async Task DeleteReviewAsync(int reviewId)
        {
            await DeleteAsync($"/reviews/{reviewId}");
        }

        public static async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            return await GetAsync<List<CategoryDto>>("/categories") ?? new List<CategoryDto>();
        }

        public static async Task<List<PickupPointDto>> GetPickupPointsAsync(bool all = false)
        {
            var url = all ? "/pickup-points?all=true" : "/pickup-points";
            return await GetAsync<List<PickupPointDto>>(url) ?? new List<PickupPointDto>();
        }

        public static async Task<List<DeliveryMethodDto>> GetDeliveryMethodsAsync()
        {
            return await GetAsync<List<DeliveryMethodDto>>("/delivery-methods") ?? new List<DeliveryMethodDto>();
        }

        public static async Task<List<StatusDto>> GetStatusesAsync()
        {
            return await GetAsync<List<StatusDto>>("/order-statuses") ?? new List<StatusDto>();
        }

        public static async Task AddProductAsync(object product)
        {
            await PostAsync<object>("/products", product);
        }

        public static async Task UpdateProductAsync(object product)
        {
            await PutAsync<object>("/products", product);
        }

        public static async Task AddCategoryAsync(string name)
        {
            await PostAsync<CategoryDto>("/categories", new { name });
        }

        public static async Task UpdateCategoryAsync(int id, string name)
        {
            await PutAsync<CategoryDto>("/categories", new { id, name });
        }

        public static async Task AddPickupPointAsync(object point)
        {
            await PostAsync<PickupPointDto>("/pickup-points", point);
        }

        public static async Task UpdatePickupPointAsync(object point)
        {
            await PutAsync<PickupPointDto>("/pickup-points", point);
        }
    }
}