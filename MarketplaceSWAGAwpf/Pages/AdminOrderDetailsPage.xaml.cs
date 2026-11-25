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
    /// Логика взаимодействия для AdminOrderDetailsPage.xaml
    /// </summary>
    public partial class AdminOrderDetailsPage : Page
    {
        private readonly int _orderId;

        public AdminOrderDetailsPage(int orderId)
        {
            InitializeComponent();
            _orderId = orderId;
            this.Loaded += AdminOrderDetailsPage_Loaded;
        }

        private async void AdminOrderDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            var order = await Core.GetOrderAsync(_orderId);
            if (order == null)
            {
                ErrorText.Text = "Заказ не найден";
                return;
            }

            TitleText.Text = order.Title;
            CustomerText.Text = order.CustomerText;
            StatusText.Text = order.StatusText;
            DeliveryText.Text = order.DeliveryText;
            TotalText.Text = order.TotalText;

            ItemsDataGrid.ItemsSource = order.Items;

            StatusComboBox.ItemsSource = Lists.Statuses;
            StatusComboBox.DisplayMemberPath = "Name";
            StatusComboBox.SelectedValuePath = "Id";

            var currentStatus = Lists.Statuses.FirstOrDefault(s => s.Name == order.Status);
            if (currentStatus != null)
            {
                StatusComboBox.SelectedValue = currentStatus.Id;
            }
        }

        private async void SaveStatus_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (StatusComboBox.SelectedValue == null)
            {
                ErrorText.Text = "Выберите статус";
                return;
            }

            int newStatusId = (int)StatusComboBox.SelectedValue;

            var order = await Core.GetOrderAsync(_orderId);
            var currentStatus = Lists.Statuses.FirstOrDefault(s => s.Name == order.Status);
            if (currentStatus != null && currentStatus.Id == newStatusId)
            {
                ErrorText.Text = "Этот статус уже установлен";
                return;
            }

            await Core.UpdateOrderStatusAsync(_orderId, newStatusId);

            MessageBox.Show("Статус изменён");
            NavigationService.Navigate(new AdminOrdersPage());
        }
    }
}