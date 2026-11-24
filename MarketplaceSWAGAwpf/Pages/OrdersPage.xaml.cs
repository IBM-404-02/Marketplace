using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarketplaceSWAGAwpf.Models;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders(true);
        }

        private async void LoadOrders(bool newestFirst)
        {
            var orders = await Core.GetOrdersAsync(false, !newestFirst);

            var viewItems = orders.Select(o => new OrderView
            {
                Id = o.Id,
                OrderDate = o.OrderDate,
                StatusName = o.Status,
                TotalSum = o.Total
            }).ToList();

            LBOrders.ItemsSource = viewItems;
        }

        private void BtnNewest_Click(object sender, RoutedEventArgs e) 
        {
            LoadOrders(true);
        }
        private void BtnOldest_Click(object sender, RoutedEventArgs e)
        {
            LoadOrders(false);
        }

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