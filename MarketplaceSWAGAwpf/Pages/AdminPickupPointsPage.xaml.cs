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
    /// Логика взаимодействия для AdminPickupPointsPage.xaml
    /// </summary>
    public partial class AdminPickupPointsPage : Page
    {
        public AdminPickupPointsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            var points = await Core.AdminGetPickupPointsAsync();
            PickupPointsDataGrid.ItemsSource = points;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditPickupPointPage(null));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (PickupPointsDataGrid.SelectedItem is PickupPointDto point)
            {
                NavigationService.Navigate(new AddEditPickupPointPage(point));
            }
            else
            {
                MessageBox.Show("Выберите пункт выдачи");
            }
        }
    }
}