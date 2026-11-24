using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceSWAGAwpf.Models;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class ProductDetailsPage : Page
    {
        private ProductDetailsDto _product;
        private List<ReviewDto> _reviews;

        public ProductDetailsPage(int productId)
        {
            InitializeComponent();
            LoadProduct(productId);
        }

        private async void LoadProduct(int productId)
        {
            _product = await Core.GetProductAsync(productId);
            if (_product == null)
            {
                MessageBox.Show("Товар не найден!");
                return;
            }

            TBName.Text = _product.Name;
            TBPrice.Text = $"{_product.Price:N2} ₽";
            TBCategory.Text = $"Категория: {_product.CategoriesText}";
            TBStock.Text = _product.StockText;
            TBStock.Foreground = _product.CanBuy ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            TBPack.Text = _product.PackText;
            TBRating.Text = _product.RatingText;
            TBDescription.Text = string.IsNullOrWhiteSpace(_product.Description) ? "Описание отсутствует" : _product.Description;

            ImgProduct.DataContext = _product;

            _reviews = await Core.GetReviewsAsync(productId);
            LBReviews.ItemsSource = _reviews.OrderByDescending(r => r.CreatedAt).ToList();

            if (Core.CurrentUser != null)
            {
                PanelActions.Visibility = Visibility.Visible;
                PanelAddReview.Visibility = Visibility.Visible;
                BTNToCartPage.Visibility = Visibility.Visible;

                bool hasReview = _reviews.Any(r => r.UserLogin == Core.CurrentUser.Login);
                if (hasReview)
                {
                    BTNAddReview.IsEnabled = false;
                    BTNAddReview.Content = "Вы уже оставили отзыв";
                }

                var favorites = await Core.GetFavoritesAsync();
                bool inFavorites = favorites.Any(f => f.ProductId == _product.Id);
                BTNToFavorites.Content = inFavorites ? "Удалить из избранного" : "В избранное";
            }
            else
            {
                PanelActions.Visibility = Visibility.Collapsed;
                PanelAddReview.Visibility = Visibility.Collapsed;
                BTNToCartPage.Visibility = Visibility.Collapsed;
            }
        }

        private async void BTNToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TBQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество!");
                return;
            }

            if (!_product.CanBuy)
            {
                MessageBox.Show("Товар недоступен для покупки!");
                return;
            }

            await Core.AddToCartAsync(_product.Id, quantity);
            MessageBox.Show($"Товар добавлен в корзину (количество: {quantity})!");
        }

        private async void BTNToFavorites_Click(object sender, RoutedEventArgs e)
        {
            var favorites = await Core.GetFavoritesAsync();
            var favorite = favorites.FirstOrDefault(f => f.ProductId == _product.Id);

            if (favorite != null)
            {
                await Core.RemoveFromFavoritesAsync(favorite.Id);
                MessageBox.Show("Товар удалён из избранного!");
                BTNToFavorites.Content = "В избранное";
            }
            else
            {
                await Core.AddToFavoritesAsync(_product.Id);
                MessageBox.Show("Товар добавлен в избранное!");
                BTNToFavorites.Content = "Удалить из избранного";
            }
        }

        private async void BTNAddReview_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TBRatingAdd.Text, out int rating) || rating < 1 || rating > 5)
            {
                MessageBox.Show("Введите оценку от 1 до 5!");
                return;
            }
            string text = TBReviewText.Text.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                MessageBox.Show("Введите текст отзыва!");
                return;
            }

            await Core.AddReviewAsync(_product.Id, rating, text);
            MessageBox.Show("Отзыв добавлен!");
            BTNAddReview.IsEnabled = false;
            BTNAddReview.Content = "Вы уже оставили отзыв";
            LoadProduct(_product.Id);
        }

        private void BTNToCartPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CartPage());
        }

        private void BTNFastOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TBQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество!");
                return;
            }

            if (!_product.CanBuy)
            {
                MessageBox.Show("Товар недоступен для покупки!");
                return;
            }
            NavigationService?.Navigate(new CheckoutPage(quantity * _product.Price, _product.Id, quantity));
        }
    }
}