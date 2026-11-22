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
    /// Логика взаимодействия для OrdersPage.xaml
    /// </summary>
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders(true);
        }

        private void LoadOrders(bool newestFirst)
        {
            var orders = Lists.Orders
                .Where(o => o.UserId == Core.CurrentUser.Id);

            if (newestFirst)
            {
                orders = orders.OrderByDescending(o => o.OrderDate);
            }
            else
            {
                orders = orders.OrderBy(o => o.OrderDate);
            }

            var viewItems = new List<OrderView>();
            foreach (var order in orders)
            {
                var status = Core.Context.OrderStatuses.FirstOrDefault(s => s.Id == order.StatusId);
                var items = Core.Context.OrderItems.Where(oi => oi.OrderId == order.Id).ToList();

                decimal total = items.Sum(oi => oi.Quantity * oi.PriceAtPurchase);

                viewItems.Add(new OrderView
                {
                    Id = order.Id,
                    OrderDate = order.OrderDate,
                    StatusName = status?.Name ?? "Неизвестен",
                    TotalSum = total
                });
            }

            LBOrders.ItemsSource = viewItems;
        }

        private void BtnNewest_Click(object sender, RoutedEventArgs e) => LoadOrders(true);
        private void BtnOldest_Click(object sender, RoutedEventArgs e) => LoadOrders(false);

        private void LBOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LBOrders.SelectedItem is OrderView selectedOrder)
            {
                NavigationService?.Navigate(new OrderDetailsPage(selectedOrder.Id));
            }
        }
    }
    public class OrderView
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public string StatusName { get; set; }
        public decimal TotalSum { get; set; }
    }
}
