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
    public partial class AddEditPickupPointPage : Page
    {
        private readonly PickupPointDto _editingPoint;

        public AddEditPickupPointPage(PickupPointDto point)
        {
            InitializeComponent();
            _editingPoint = point;

            if (_editingPoint != null)
            {
                TitleText.Text = "Редактировать пункт выдачи";
                AddressTextBox.Text = _editingPoint.Address;
                WorkingHoursTextBox.Text = _editingPoint.WorkingHours;
                IsActiveCheckBox.IsChecked = _editingPoint.IsActive;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                ErrorText.Text = "Адрес не может быть пустым";
                return;
            }

            string address = AddressTextBox.Text.Trim();
            string? workingHours = string.IsNullOrWhiteSpace(WorkingHoursTextBox.Text) ? null : WorkingHoursTextBox.Text.Trim();
            bool isActive = IsActiveCheckBox.IsChecked ?? true;

            if (_editingPoint == null)
            {
                await Core.AdminAddPickupPointAsync(address, workingHours);
            }
            else
            {
                await Core.AdminUpdatePickupPointAsync(_editingPoint.Id, address, workingHours, isActive);
            }

            await Lists.UpdateAsync();
            NavigationService.Navigate(new AdminPickupPointsPage());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminPickupPointsPage());
        }
    }
}