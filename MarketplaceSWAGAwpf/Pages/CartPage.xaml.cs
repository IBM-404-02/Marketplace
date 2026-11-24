using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceSWAGAwpf.Models;
using MarketplaceSWAGAwpf.Pages;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class CartPage : Page
    {
        decimal total = 0;

        public CartPage()
        {
            InitializeComponent();
            LoadCart();
        }

        private async void LoadCart()
        {
            var cart = await Core.GetCartAsync();
            if (cart != null)
            {
                LBCart.ItemsSource = cart.Items;
                total = cart.Total;
                TBTotal.Text = $"Итого: {total:N2} ₽";
            }

        }

        private async void BtnDec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int itemId)
            {
                var item = LBCart.ItemsSource.Cast<CartItemDto>().FirstOrDefault(c => c.Id == itemId);
                if (item != null && item.Quantity > 1)
                {

                    await Core.UpdateCartItemAsync(itemId, item.Quantity - 1);
                    LoadCart();

                }
                else if (item != null && item.Quantity == 1)
                {
                    var result = MessageBox.Show("Удалить товар из корзины?", "Подтверждение", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                    {

                        await Core.RemoveFromCartAsync(itemId);
                        LoadCart();

                    }
                }
            }
        }

        private async void BtnInc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int itemId)
            {
                var item = LBCart.ItemsSource.Cast<CartItemDto>().FirstOrDefault(c => c.Id == itemId);
                if (item != null)
                {

                    await Core.UpdateCartItemAsync(itemId, item.Quantity + 1);
                    LoadCart();

                }
            }
        }

        private async void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int itemId)
            {
                var result = MessageBox.Show("Удалить товар из корзины?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {

                    await Core.RemoveFromCartAsync(itemId);
                    LoadCart();

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

        private async void BTNClear_Click(object sender, RoutedEventArgs e)
        {
            if (LBCart.Items.Count == 0)
            {
                MessageBox.Show("Корзина уже пуста!");
                return;
            }
            var result = MessageBox.Show("Очистить всю корзину?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {

                var items = LBCart.ItemsSource.Cast<CartItemDto>().ToList();
                foreach (var item in items)
                {
                    await Core.RemoveFromCartAsync(item.Id);
                }
                LoadCart();

            }
        }
    }
}