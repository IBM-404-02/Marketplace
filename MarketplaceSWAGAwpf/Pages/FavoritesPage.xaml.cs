using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MarketplaceSWAGAwpf.Pages
{
    /// <summary>
    /// Логика взаимодействия для FavoritesPage.xaml
    /// </summary>

    public partial class FavoritesPage : Page
    {
        public FavoritesPage()
        {
            InitializeComponent();
            CBSort.SelectedIndex = 0;
            LoadFavorites();
        }

        private void LoadFavorites()
        {
            if (Core.CurrentUser == null) return;

            var favoritesQuery = Core.Context.Favorites
                .Where(f => f.UserId == Core.CurrentUser.Id)
                .Select(f => new FavoriteView
                {
                    FavoriteId = f.Id,
                    Id = f.ProductId,
                    Name = f.Products.Name,
                    Price = f.Products.Price,
                    ImagePath = f.Products.ImagePath,
                    IsAvailable = f.Products.IsAvailable,
                    StockQuantity = f.Products.StockQuantity,
                    AddedAt = f.AddedAt
                });

            IEnumerable<FavoriteView> sortedFavorites;
            switch (CBSort.SelectedIndex)
            {
                case 1:
                    sortedFavorites = favoritesQuery.OrderBy(f => f.Price);
                    break;
                case 2:
                    sortedFavorites = favoritesQuery.OrderByDescending(f => f.Price);
                    break;
                case 3:
                    sortedFavorites = favoritesQuery.OrderBy(f => f.Name);
                    break;
                case 4:
                    sortedFavorites = favoritesQuery.OrderByDescending(f => f.Name);
                    break;
                default:
                    sortedFavorites = favoritesQuery.OrderByDescending(f => f.AddedAt);
                    break;
            }

            LBFavorites.ItemsSource = sortedFavorites.ToList();
        }

        private void BtnToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int productId)
            {
                var product = Core.Context.Products.FirstOrDefault(p => p.Id == productId);
                if (product == null) return;

                if (!product.IsAvailable || product.StockQuantity == 0)
                {
                    MessageBox.Show("Этот товар сейчас недоступен для покупки!");
                    return;
                }

                var existingItem = Core.Context.CartItems
                    .FirstOrDefault(c => c.UserId == Core.CurrentUser.Id && c.ProductId == productId);

                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    Core.Context.CartItems.Add(new CartItems
                    {
                        UserId = Core.CurrentUser.Id,
                        ProductId = productId,
                        Quantity = 1
                    });
                }

                Core.Context.SaveChanges();
                MessageBox.Show("Товар добавлен в корзину!");
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int favoriteId)
            {
                var result = MessageBox.Show("Удалить товар из избранного?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var favItem = Core.Context.Favorites.FirstOrDefault(f => f.Id == favoriteId);
                    if (favItem != null)
                    {
                        Core.Context.Favorites.Remove(favItem);
                        Core.Context.SaveChanges();

                        LoadFavorites();
                    }
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
