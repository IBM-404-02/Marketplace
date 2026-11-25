using MarketplaceSWAGAwpf.Pages;
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

namespace MarketplaceSWAGAwpf
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new CataloguePage());
        }

        public void UpdateMenuVisibility()
        {
            if (Core.CurrentUser != null)
            {
                BTNcart.Visibility = Visibility.Visible;
                BTNfavorites.Visibility = Visibility.Visible;
                BTNorders.Visibility = Visibility.Visible;
                BTNLogout.Visibility = Visibility.Visible;

                if (Core.IsAdmin)
                {
                    BTNadmin.Visibility = Visibility.Visible;
                }
                else
                {
                    BTNadmin.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                BTNcart.Visibility = Visibility.Collapsed;
                BTNfavorites.Visibility = Visibility.Collapsed;
                BTNorders.Visibility = Visibility.Collapsed;
                BTNadmin.Visibility = Visibility.Collapsed;
                BTNLogout.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите закрыть приложение?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }

        private void BTNcatalogue_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CataloguePage());
        }

        private void BTNcart_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CartPage());
        }

        private void BTNfavorites_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new FavoritesPage());
        }

        private void BTNorders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersPage());
        }

        private void BTNaccount_Click(object sender, RoutedEventArgs e)
        {
            if (Core.CurrentUser == null)
            {
                MainFrame.Navigate(new LoginPage());
            }
            else
            {
                MainFrame.Navigate(new AccountPage());
            }
        }

        private void BTNadmin_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminPage());
        }

        private void BTNLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Core.Logout();
                UpdateMenuVisibility();
                while (MainFrame.CanGoBack)
                {
                    MainFrame.NavigationService.RemoveBackEntry();
                }
                MainFrame.Navigate(new CataloguePage());
            }
        }

        private void BTNBack_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame != null && MainFrame.CanGoBack)
            {
               MainFrame.GoBack();
            }
        }

    }
}
