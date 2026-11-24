using System.Windows;
using System.Windows.Controls;
using MarketplaceSWAGAwpf.Pages;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private async void BTNEnter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBLogin.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            bool ok = await Core.LoginAsync(TBLogin.Text.Trim(), PasswordBox.Password);

            if (ok)
            {
                MessageBox.Show($"Добро пожаловать, {Core.CurrentUser?.Login}!");

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.UpdateMenuVisibility();
                }

                if (Core.IsAdmin)
                    NavigationService?.Navigate(new AdminPage());
                else
                    NavigationService?.Navigate(new AccountPage());
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль!");
            }
        }

        private void BTNReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }
    }
}