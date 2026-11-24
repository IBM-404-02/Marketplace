using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarketplaceSWAGAwpf.Models;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class FavoritesPage : Page
    {
        public FavoritesPage()
        {
            InitializeComponent();
            CBSort.SelectedIndex = 0;
            LoadFavorites();
        }

        private async void LoadFavorites()
        {
            if (Core.CurrentUser == null) return;

            var favorites = await Core.GetFavoritesAsync();

            var viewItems = favorites.Select(f => new FavoriteView
            {
                FavoriteId = f.Id,
                Id = f.ProductId,
                Name = f.ProductName,
                Price = f.Price,
                ImagePath = f.ImagePath,
                IsAvailable = f.CanBuy,
                StockQuantity = f.CanBuy ? 1 : 0,
                AddedAt = f.AddedAt
            });

            IEnumerable<FavoriteView> sortedFavorites;
            switch (CBSort.SelectedIndex)
            {
                case 1: 
                    sortedFavorites = viewItems.OrderBy(f => f.Price); 
                    break;
                case 2: 
                    sortedFavorites = viewItems.OrderByDescending(f => f.Price); 
                    break;
                case 3: 
                    sortedFavorites = viewItems.OrderBy(f => f.Name);
                    break;
                case 4: 
                    sortedFavorites = viewItems.OrderByDescending(f => f.Name); 
                    break;
                default: 
                    sortedFavorites = viewItems.OrderByDescending(f => f.AddedAt); 
                    break;
            }

            LBFavorites.ItemsSource = sortedFavorites.ToList();
        }

        private async void BtnToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int productId)
            {
                await Core.AddToCartAsync(productId, 1);
                MessageBox.Show("Товар добавлен в корзину!");
            }
        }

        private async void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int favoriteId)
            {
                var result = MessageBox.Show("Удалить товар из избранного?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    await Core.RemoveFromFavoritesAsync(favoriteId);
                    LoadFavorites();
                }
            }
        }

        private void LBFavorites_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LBFavorites.SelectedItem is FavoriteView selectedProduct)
            {
                NavigationService?.Navigate(new ProductDetailsPage(selectedProduct.Id));
            }
        }

        private void CBSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadFavorites();
        }
    }

    public class FavoriteView
    {
        public int FavoriteId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public bool IsAvailable { get; set; }
        public int StockQuantity { get; set; }
        public DateTime AddedAt { get; set; }
        public string StockText => !IsAvailable ? "Недоступен для покупки" : (StockQuantity == 0 ? "Нет в наличии" : $"В наличии: {StockQuantity} шт.");
    }
}