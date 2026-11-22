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
    /// Логика взаимодействия для ProductDetailsPage.xaml
    /// </summary>
    public partial class ProductDetailsPage : Page
    {
        private Products _product;

        public ProductDetailsPage(int productId)
        {
            InitializeComponent();
            LoadProduct(productId);
        }

        private void LoadProduct(int productId)
        {
            _product = Core.Context.Products
                .FirstOrDefault(p => p.Id == productId);

            if (_product == null)
            {
                MessageBox.Show("Товар не найден!");
                return;
            }

            TBName.Text = _product.Name;
            TBPrice.Text = $"{_product.Price:N2} ₽";

            var category = _product.ProductCategories.FirstOrDefault()?.Categories;
            TBCategory.Text = category != null ? $"Категория: {category.Name}" : "Категория не указана";

            if (!_product.IsAvailable)
            {
                TBStock.Text = "Товар недоступен для покупки";
                TBStock.Foreground = System.Windows.Media.Brushes.Red;
            }
            else if (_product.StockQuantity == 0)
            {
                TBStock.Text = "Нет в наличии";
                TBStock.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                TBStock.Text = $"В наличии: {_product.StockQuantity} шт.";
                TBStock.Foreground = System.Windows.Media.Brushes.Green;
            }

            if (_product.PackQuantity.HasValue)
            {
                TBPack.Text = $"В упаковке: {_product.PackQuantity.Value} шт.";
            }
            else
            {
                TBPack.Text = "";
            }

            var reviews = _product.Reviews.ToList();
            if (reviews.Count > 0)
            {
                double avg = reviews.Average(r => r.Rating);
                TBRating.Text = $"Рейтинг: {avg:0.0} ({reviews.Count} отзывов)";
            }
            else
            {
                TBRating.Text = "Рейтинг: нет оценок";
            }

            TBDescription.Text = string.IsNullOrWhiteSpace(_product.Description) ? "Описание отсутствует" : _product.Description;

            ImgProduct.DataContext = _product;

            LBReviews.ItemsSource = reviews.OrderByDescending(r => r.CreatedAt).ToList();

            if (Core.CurrentUser != null)
            {
                PanelActions.Visibility = Visibility.Visible;
                PanelAddReview.Visibility = Visibility.Visible;
                BTNToCartPage.Visibility = Visibility.Visible;

                bool hasReview = reviews.Any(r => r.UserId == Core.CurrentUser.Id);
                if (hasReview)
                {
                    BTNAddReview.IsEnabled = false;
                    BTNAddReview.Content = "Вы уже оставили отзыв";
                }

                bool inFavorites = Core.Context.Favorites
                    .Any(f => f.UserId == Core.CurrentUser.Id && f.ProductId == _product.Id);
                if (inFavorites)
                {
                    BTNToFavorites.Content = "Удалить из избранного";
                }
                else
                {
                    BTNToFavorites.Content = "В избранное";
                }
            }
            else
            {
                PanelActions.Visibility = Visibility.Collapsed;
                PanelAddReview.Visibility = Visibility.Collapsed;
                BTNToCartPage.Visibility = Visibility.Collapsed;
            }
        }

        private void BTNToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(TBQuantity.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите корректное количество!");
                return;
            }

            if (!_product.IsAvailable || _product.StockQuantity == 0)
            {
                MessageBox.Show("Товар недоступен для покупки!");
                return;
            }

            try
            {
                var existingItem = Core.Context.CartItems
                    .FirstOrDefault(c => c.UserId == Core.CurrentUser.Id && c.ProductId == _product.Id);

                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                }
                else
                {
                    Core.Context.CartItems.Add(new CartItems
                    {
                        UserId = Core.CurrentUser.Id,
                        ProductId = _product.Id,
                        Quantity = quantity
                    });
                }

                Core.Context.SaveChanges();
                MessageBox.Show($"Товар добавлен в корзину (количество: {quantity})!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void BTNToFavorites_Click(object sender, RoutedEventArgs e)
        {
            var favorite = Core.Context.Favorites
                .FirstOrDefault(f => f.UserId == Core.CurrentUser.Id && f.ProductId == _product.Id);

            if (favorite != null)
            {
                Core.Context.Favorites.Remove(favorite);
                Core.Context.SaveChanges();
                MessageBox.Show("Товар удалён из избранного!");

                BTNToFavorites.Content = "В избранное";
                BTNToFavorites.IsEnabled = true;
            }
            else
            {
                Core.Context.Favorites.Add(new Favorites
                {
                    UserId = Core.CurrentUser.Id,
                    ProductId = _product.Id
                });

                Core.Context.SaveChanges();
                MessageBox.Show("Товар добавлен в избранное!");

                BTNToFavorites.Content = "Удалить из избранного";
                BTNToFavorites.IsEnabled = true;
            }
        }


        private void BTNAddReview_Click(object sender, RoutedEventArgs e)
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

            Core.Context.Reviews.Add(new Reviews
            {
                UserId = Core.CurrentUser.Id,
                ProductId = _product.Id,
                Rating = (byte)rating,
                Text = text
            });

            Core.Context.SaveChanges();
            MessageBox.Show("Отзыв добавлен!");

            BTNAddReview.IsEnabled = false;
            BTNAddReview.Content = "Вы уже оставили отзыв";

            LoadProduct(_product.Id);
        }

        private void BTNToCartPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CartPage());
        }
    }
}
