using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        private void BTNEnter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBLogin.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                TBError.Text = "Введите логин и пароль";
                return;
            }

            string hashedPassword = Core.GetHash(PasswordBox.Password);

            var user = Core.Context.Users
                .FirstOrDefault(u => u.Login == TBLogin.Text.Trim() && u.PasswordHash == hashedPassword);

            if (user == null)
            {
                TBError.Text = "Неверный логин или пароль!";
                return;
            }

            if (Core.CurrentUser != null)
            {
                TBError.Text = "Сначала выйдите из текущего аккаунта";
                return;
            }
            Core.CurrentUser = user;

            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.UpdateMenuVisibility();
            }

            TBError.Text = "";
            MessageBox.Show($"Добро пожаловать, {user.Login}!");

            string roleName = user.Roles?.Name ??
                              Core.Context.Roles.FirstOrDefault(r => r.Id == user.RoleId)?.Name ?? "";

            if (roleName.ToUpper() == "ADMIN")
            {
                NavigationService?.Navigate(new AdminPage());
            }
            else
            {
                NavigationService?.Navigate(new AccountPage());
            }
        }

        private void BTNReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegisterPage());
        }
   }
}
