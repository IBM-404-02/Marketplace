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
    //класс для хранения общего HttpClient для запросов. Плюсом хранит текущего пользователя и некоторые "стандартные" функции
    public static class Core
    {
        public static string ApiUrl = "http://localhost:5299/api"; //адресс сервера api
        private static readonly HttpClient _client = new HttpClient(); //общий HttpClient
        public static MeDto? CurrentUser { get; private set; } //текущий юзер
        public static string? Token { get; private set; } //токен для авторизованных запросов
        public static bool IsAuthorized => CurrentUser != null;
        public static bool IsAdmin => CurrentUser?.Role == "Admin";

        static Core()
        {
            //обозначение какой тип данных хотим получать (json)
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static void SetToken(string token)
        {
            //токен текущего юзера добавляется в запрос автоматом
            Token = token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        private static void ClearAuth()
        {
            //очистка токена и юзера после выхода
            Token = null;
            CurrentUser = null;
            _client.DefaultRequestHeaders.Authorization = null;
        }

        private static async Task<T?> GetAsync<T>(string url) //GET-запрос 
        {
            var response = await _client.GetAsync(ApiUrl + url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private static async Task<T?> PostAsync<T>(string url, object? body = null) //POST-запрос
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

        private static async Task<T?> PutAsync<T>(string url, object? body = null) //PUT-запрос
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

        private static async Task DeleteAsync(string url) //DELETE-запрос
        {
            var response = await _client.DeleteAsync(ApiUrl + url);
            response.EnsureSuccessStatusCode();
        }

        //методы, которые вызываются на страницах. Типа оберток обычных запросов POST/PUT/DELETE/GET
        public static async Task<bool> LoginAsync(string login, string password)
        {
            try
            {
                var result = await PostAsync<LoginResponse>("/auth/login", new { login, password });
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
                await PostAsync<LoginResponse>("/auth/register", new { login, password, confirmPassword });
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
            {
                query.Add($"search={Uri.EscapeDataString(search)}");
            }
            if (categoryId.HasValue)
            {
                query.Add($"categoryId={categoryId.Value}");
            }
            if (minPrice.HasValue)
            {
                query.Add($"minPrice={minPrice.Value}");
            }
            if (maxPrice.HasValue)
            {
                query.Add($"maxPrice={maxPrice.Value}");
            }
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
            return await PostAsync<OrderCreatedDto>("/orders", new { deliveryMethodId, pickupPointId, deliveryAddress = address, productId, quantity });
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

        // Отзывы
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

        // Справочники
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

        // Админ: товары
        // POST /api/products — создание товара
        // Сервер ждёт ProductRequest: Id, Name, Description, Price, ImagePath, StockQuantity, PackQuantity, IsAvailable, CategoryIds
        public static async Task AdminAddProductAsync(
            string name, string? description, decimal price, string? imagePath,
            int stockQuantity, int? packQuantity, bool isAvailable, List<int> categoryIds)
        {
            await PostAsync<object>("/products", new
            {
                id = 0,
                name,
                description,
                price,
                imagePath,
                stockQuantity,
                packQuantity,
                isAvailable,
                categoryIds
            });
        }

        // PUT /api/products — обновление товара (id в теле, не в URL)
        public static async Task AdminUpdateProductAsync(
            int id, string name, string? description, decimal price, string? imagePath,
            int stockQuantity, int? packQuantity, bool isAvailable, List<int> categoryIds)
        {
            await PutAsync<object>("/products", new
            {
                id,
                name,
                description,
                price,
                imagePath,
                stockQuantity,
                packQuantity,
                isAvailable,
                categoryIds
            });
        }

        public static async Task AdminDeleteProductAsync(int id)
        {
            await DeleteAsync($"/products/{id}");
        }

        public static async Task AdminAddCategoryAsync(string name)
        {
            await PostAsync<CategoryDto>("/categories", new { id = 0, name });
        }

        public static async Task AdminUpdateCategoryAsync(int id, string name)
        {
            await PutAsync<CategoryDto>("/categories", new { id, name });
        }

        public static async Task AdminDeleteCategoryAsync(int id)
        {
            await DeleteAsync($"/categories/{id}");
        }

        public static async Task UpdateOrderStatusAsync(int orderId, int statusId)
        {
            await PutAsync<object>("/orders", new { id = orderId, statusId });
        }

        public static async Task<List<PickupPointDto>> AdminGetPickupPointsAsync()
        {
            return await GetAsync<List<PickupPointDto>>("/pickup-points?all=true") ?? new List<PickupPointDto>();
        }

        public static async Task AdminAddPickupPointAsync(string address, string? workingHours)
        {
            await PostAsync<object>("/pickup-points", new
            {
                id = 0,
                address,
                workingHours,
                isActive = true
            });
        }

        public static async Task AdminUpdatePickupPointAsync(int id, string address, string? workingHours, bool isActive)
        {
            await PutAsync<object>("/pickup-points", new
            {
                id,
                address,
                workingHours,
                isActive
            });
        }
    }
}