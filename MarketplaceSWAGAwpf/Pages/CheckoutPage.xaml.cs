using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MarketplaceSWAGAwpf.Models;
using MarketplaceSWAGAwpf.Pages;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class CheckoutPage : Page
    {
        private decimal _total;
        private int? _singleProductId;
        private int _singleQuantity;

        public CheckoutPage(decimal total)
        {
            InitializeComponent();
            _total = total;
            _singleProductId = null;
            Load();
        }

        public CheckoutPage(decimal total, int productId, int quantity)
        {
            InitializeComponent();
            _total = total;
            _singleProductId = productId;
            _singleQuantity = quantity;
            Load();
        }

        private void Load()
        {
            TBTotal.Text = $"Итого к оплате: {_total:N2} ₽";
            var activePickupPoints = Lists.PickupPoints.Where(p => p.IsActive).ToList();
            CBPickupPoint.ItemsSource = activePickupPoints;
            CBPickupPoint.DisplayMemberPath = "Address";
            CBPickupPoint.SelectedValuePath = "Id";
            if (activePickupPoints.Count > 0)
            {
                CBPickupPoint.SelectedIndex = 0;
            }
            if (_total < 600)
            {
                RBDelivery.IsEnabled = false;
                RBDelivery.Content = "Доставка курьером (недоступно при сумме < 600 ₽)";
                TBDeliveryWarning.Visibility = Visibility.Visible;
            }
            else
            {
                RBDelivery.IsEnabled = true;
                RBDelivery.Content = "Доставка курьером";
                TBDeliveryWarning.Visibility = Visibility.Collapsed;
            }
        }

        private void RBPickup_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelPickup != null && PanelDelivery != null)
            {
                PanelPickup.Visibility = Visibility.Visible;
                PanelDelivery.Visibility = Visibility.Collapsed;
            }
        }

        private void RBDelivery_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelPickup != null && PanelDelivery != null)
            {
                PanelPickup.Visibility = Visibility.Collapsed;
                PanelDelivery.Visibility = Visibility.Visible;
            }
        }

        private async void BTNCheckout_Click(object sender, RoutedEventArgs e)
        {
            TBError.Text = "";
            int deliveryMethodId;
            int? pickupPointId = null;
            string deliveryAddress = null;

            if (RBPickup.IsChecked == true)
            {
                deliveryMethodId = 1;
                if (CBPickupPoint.SelectedItem == null)
                {
                    TBError.Text = "Выберите пункт выдачи!";
                    return;
                }
                pickupPointId = (int)CBPickupPoint.SelectedValue;
            }
            else if (RBDelivery.IsChecked == true)
            {
                deliveryMethodId = 2;
                if (_total < 600)
                {
                    TBError.Text = "Доставка недоступна при сумме менее 600 ₽!";
                    return;
                }
                deliveryAddress = TBAddress.Text.Trim();
                if (string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    TBError.Text = "Введите адрес доставки!";
                    return;
                }
            }
            else
            {
                TBError.Text = "Выберите способ получения!";
                return;
            }


            var result = await Core.CreateOrderAsync(
                deliveryMethodId,
                pickupPointId,
                deliveryAddress,
                _singleProductId,
                _singleProductId.HasValue ? _singleQuantity : (int?)null
            );

            if (result != null)
            {
                MessageBox.Show($"Заказ №{result.Id} успешно оформлен!\nСумма: {result.Total:N2} ₽");
                NavigationService?.Navigate(new OrdersPage());
            }
        }
    }
}