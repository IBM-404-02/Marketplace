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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
        }

        private void GoToProducts_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminProductsPage());
        }

        private void GoToCategories_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminCategoriesPage());
        }

        private void GoToPickupPoints_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminPickupPointsPage());
        }

        private void GoToOrders_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminOrdersPage());
        }
    }
}
