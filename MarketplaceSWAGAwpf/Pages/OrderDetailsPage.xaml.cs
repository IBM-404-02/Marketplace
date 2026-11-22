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
    /// Логика взаимодействия для OrderDetailsPage.xaml
    /// </summary>
    public partial class OrderDetailsPage : Page
    {
        private int _orderId;

        public OrderDetailsPage(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            LoadOrder();
        }

        private void LoadOrder()
        {
            var order = Lists.Orders.FirstOrDefault(o => o.Id == _orderId);
            if (order == null)
            {
                MessageBox.Show("Заказ не найден!");
                return;
            }

            var status = Lists.OrderStatuses.FirstOrDefault(s => s.Id == order.StatusId);
            var pickupPoint = order.PickupPointId.HasValue ? Lists.PickupPoints.FirstOrDefault(p => p.Id == order.PickupPointId.Value) : null;

            TBTitle.Text = $"Заказ №{order.Id}";
            TBDate.Text = $"Дата оформления: {order.OrderDate:dd.MM.yyyy HH:mm}";
            TBStatus.Text = $"Статус: {status?.Name ?? "Неизвестен"}";

            if (order.DeliveryMethodId == 1)
            {
                TBDelivery.Text = $"Способ получения: Самовывоз из ПВЗ ({pickupPoint?.Address})";
            }
            else
            {
                TBDelivery.Text = $"Способ получения: Доставка по адресу ({order.DeliveryAddress})";
            }

            var orderItems = Lists.OrderItems.Where(oi => oi.OrderId == _orderId);

            var itemsToShow = new List<OrderItemView>();
            decimal total = 0;

            foreach (var oi in orderItems)
            {
                var product = Lists.Products.FirstOrDefault(p => p.Id == oi.ProductId);
                decimal sum = oi.Quantity * oi.PriceAtPurchase;
                total += sum;

                itemsToShow.Add(new OrderItemView
                {
                    ProductName = product?.Name ?? "Товар удален",
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase,
                    Sum = sum
                });
            }

            LBItems.ItemsSource = itemsToShow;
            TBTotal.Text = $"Итого: {total:N2} ₽";
        }
    }
    public class OrderItemView
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public decimal Sum { get; set; }
    }
}
