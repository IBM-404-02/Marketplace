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
                UpdateUI();

                // Показываем каталог при старте
                MainFrame.Navigate(new CataloguePage());
            }

            // Обновление интерфейса в зависимости от авторизации
            public void UpdateUI()
            {
                if (Core.IsAuthorized)
                {
                    // Пользователь авторизован
                    TbUser.Text = $"{Core.CurrentUser?.Login} ({Core.CurrentUser?.Role})";
                    BtnAccount.Visibility = Visibility.Visible;
                    BtnLogout.Visibility = Visibility.Visible;
                    BtnLogin.Visibility = Visibility.Collapsed;

                    // Показываем кнопки для авторизованных
                    BtnCart.Visibility = Visibility.Visible;
                    BtnFavorites.Visibility = Visibility.Visible;
                    BtnOrders.Visibility = Visibility.Visible;

                    // Показываем админку только админам
                    if (Core.IsAdmin)
                    {
                        BtnAdmin.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BtnAdmin.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    // Гость
                    TbUser.Text = "Гость";
                    BtnAccount.Visibility = Visibility.Collapsed;
                    BtnLogout.Visibility = Visibility.Collapsed;
                    BtnLogin.Visibility = Visibility.Visible;
                    BtnAdmin.Visibility = Visibility.Collapsed;

                    // Скрываем кнопки для авторизованных
                    BtnCart.Visibility = Visibility.Collapsed;
                    BtnFavorites.Visibility = Visibility.Collapsed;
                    BtnOrders.Visibility = Visibility.Collapsed;
                }
            }

            // Обработчики кнопок навигации
            private void BtnCatalogue_Click(object sender, RoutedEventArgs e) =>
                MainFrame.Navigate(new CataloguePage());

            private void BtnCart_Click(object sender, RoutedEventArgs e) =>
                MainFrame.Navigate(new CartPage());

            private void BtnFavorites_Click(object sender, RoutedEventArgs e) =>
                MainFrame.Navigate(new FavoritesPage());

            private void BtnOrders_Click(object sender, RoutedEventArgs e) =>
                MainFrame.Navigate(new OrdersPage());

            private void BtnAdmin_Click(object sender, RoutedEventArgs e) =>
                MainFrame.Navigate(new AdminPage());

            private void BtnAccount_Click(object sender, RoutedEventArgs e) =>
                MainFrame.Navigate(new AccountPage());

            private void BtnLogin_Click(object sender, RoutedEventArgs e) =>
                MainFrame.Navigate(new LoginPage());

            private void BtnLogout_Click(object sender, RoutedEventArgs e)
            {
                Core.Logout();
                UpdateUI();
                MainFrame.Navigate(new LoginPage());
            }
        }
}
