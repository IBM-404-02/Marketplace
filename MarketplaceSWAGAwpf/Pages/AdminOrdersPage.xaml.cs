using MarketplaceSWAGAwpf.Models;
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
    /// Логика взаимодействия для AdminOrdersPage.xaml
    /// </summary>
    public partial class AdminOrdersPage : Page
    {

        private List<OrderDto> _allOrders = new();

        public AdminOrdersPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            _allOrders = await Core.GetOrdersAsync(all: true);

            StatusFilterComboBox.Items.Clear();
            StatusFilterComboBox.Items.Add(new StatusDto { Id = 0, Name = "Все" });
            foreach (var status in Lists.Statuses)
            {
                StatusFilterComboBox.Items.Add(status);
            }
            StatusFilterComboBox.DisplayMemberPath = "Name";
            StatusFilterComboBox.SelectedValuePath = "Id";
            StatusFilterComboBox.SelectedIndex = 0;

            ShowOrders(_allOrders);
        }

        private void ShowOrders(List<OrderDto> orders)
        {
            OrdersDataGrid.ItemsSource = orders;
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StatusFilterComboBox.SelectedValue == null)
                return;

            int statusId = (int)StatusFilterComboBox.SelectedValue;

            if (statusId == 0)
            {
                ShowOrders(_allOrders);
            }
            else
            {
                var statusName = Lists.Statuses.FirstOrDefault(s => s.Id == statusId)?.Name;
                var filtered = _allOrders.Where(o => o.Status == statusName).ToList();
                ShowOrders(filtered);
            }
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            StatusFilterComboBox.SelectedIndex = 0;
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersDataGrid.SelectedItem is OrderDto order)
            {
                NavigationService.Navigate(new AdminOrderDetailsPage(order.Id));
            }
            else
            {
                MessageBox.Show("Выберите заказ");
            }
        }
    }
}