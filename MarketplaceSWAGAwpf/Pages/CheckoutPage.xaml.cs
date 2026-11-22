using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
    /// Логика взаимодействия для CheckoutPage.xaml
    /// </summary>
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

        private void BTNCheckout_Click(object sender, RoutedEventArgs e)
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

                Orders newOrder = new Orders
                {
                    UserId = Core.CurrentUser.Id,
                    OrderDate = DateTime.Now,
                    DeliveryMethodId = deliveryMethodId,
                    PickupPointId = pickupPointId,
                    DeliveryAddress = deliveryAddress,
                    StatusId = 1
                };

                Core.Context.Orders.Add(newOrder);
                Core.Context.SaveChanges();

            if (_singleProductId.HasValue)
            {
                var product = Core.Context.Products.FirstOrDefault(p => p.Id == _singleProductId.Value);
                if (product != null)
                {
                    Core.Context.OrderItems.Add(new OrderItems
                    {
                        OrderId = newOrder.Id,
                        ProductId = product.Id,
                        Quantity = _singleQuantity,
                        PriceAtPurchase = product.Price
                    });

                    product.StockQuantity -= _singleQuantity;
                    if (product.StockQuantity < 0) product.StockQuantity = 0;
                }
            }
            else
            {
                var cartItems = Core.Context.CartItems.Where(c => c.UserId == Core.CurrentUser.Id).ToList();

                foreach (var cartItem in cartItems)
                {
                    Core.Context.OrderItems.Add(new OrderItems
                    {
                        OrderId = newOrder.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceAtPurchase = cartItem.Products.Price
                    });

                    var product = Core.Context.Products.FirstOrDefault(p => p.Id == cartItem.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= cartItem.Quantity;
                        if (product.StockQuantity < 0) product.StockQuantity = 0;
                    }
                }

                Core.Context.CartItems.RemoveRange(cartItems);
            }

            Core.Context.SaveChanges();

            MessageBox.Show($"Заказ №{newOrder.Id} успешно оформлен!\nСумма: {_total:N2} ₽");
                NavigationService?.Navigate(new OrdersPage());
        }
    }
}