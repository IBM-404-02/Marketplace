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
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        decimal total = 0;
        public CartPage()
        {
            InitializeComponent();
            LoadCart();
        }

        private void LoadCart()
        {
            var cartItems = Core.Context.CartItems
                .Where(c => c.UserId == Core.CurrentUser.Id)
                .ToList();

            LBCart.ItemsSource = cartItems;

            total = cartItems.Sum(c => c.Products.Price * c.Quantity);
            TBTotal.Text = $"Итого: {total:N2} ₽";
        }

        private void BtnDec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int itemId)
            {
                var item = Core.Context.CartItems.FirstOrDefault(c => c.Id == itemId);
                if (item != null && item.Quantity > 1)
                {
                    item.Quantity--;
                    Core.Context.SaveChanges();
                    LoadCart();
                }
            }
        }

        private void BtnInc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int itemId)
            {
                var item = Core.Context.CartItems.FirstOrDefault(c => c.Id == itemId);
                if (item != null)
                {
                    if (item.Products.StockQuantity > item.Quantity)
                    {
                        item.Quantity++;
                        Core.Context.SaveChanges();
                        LoadCart();
                    }
                    else
                    {
                        MessageBox.Show("Нельзя добавить больше, чем есть в наличии!");
                    }
                }
            }
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int itemId)
            {
                var result = MessageBox.Show("Удалить товар из корзины?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    var item = Core.Context.CartItems.FirstOrDefault(c => c.Id == itemId);
                    if (item != null)
                    {
                        Core.Context.CartItems.Remove(item);
                        Core.Context.SaveChanges();
                        LoadCart();
                    }
                }
            }
        }

        private void BTNCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (LBCart.Items.Count == 0)
            {
                MessageBox.Show("Корзина пуста!");
                return;
            }

            NavigationService?.Navigate(new CheckoutPage(total));
        }

        private void BTNClear_Click(object sender, RoutedEventArgs e)
        {
            if (LBCart.Items.Count == 0)
            {
                MessageBox.Show("Корзина уже пуста!");
                return;
            }

            var result = MessageBox.Show("Очистить всю корзину?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                var cartItems = Core.Context.CartItems
                    .Where(c => c.UserId == Core.CurrentUser.Id)
                    .ToList();

                Core.Context.CartItems.RemoveRange(cartItems);
                Core.Context.SaveChanges();
                LoadCart();
            }
        }
    }
}