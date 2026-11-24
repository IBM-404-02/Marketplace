using System;
using System.Windows;
using System.Windows.Controls;
using MarketplaceSWAGAwpf.Pages;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class AccountPage : Page
    {
        public AccountPage()
        {
            InitializeComponent();
            LoadUserData();
        }

        private void LoadUserData()
        {
            if (Core.CurrentUser == null)
            {
                NavigationService?.Navigate(new LoginPage());
                return;
            }

            TBLogin.Text = Core.CurrentUser.Login;
            TBRole.Text = Core.CurrentUser.Role;
        }
    }
}