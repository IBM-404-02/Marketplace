using System;
using System.Windows;
using System.Windows.Controls;
using MarketplaceSWAGAwpf.Pages;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }

        private bool CheckPSW(string password1, string password2)
        {
            if (password1.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                return false;
            }

            bool hasDigit = false;
            bool onlyEnglish = true;

            foreach (char c in password1)
            {
                if (char.IsDigit(c)) hasDigit = true;
                else if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))) onlyEnglish = false;
            }

            if (!onlyEnglish)
            {
                MessageBox.Show("Используйте только английскую раскладку!");
                return false;
            }
            if (!hasDigit)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!");
                return false;
            }
            if (password1 != password2)
            {
                MessageBox.Show("Пароли не совпадают!");
                return false;
            }
            return true;
        }

        private async void regButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBLog.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) ||
                string.IsNullOrEmpty(passBxScnd.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            if (!CheckPSW(passBxFrst.Password, passBxScnd.Password))
            {
                return;
            }
            bool success = await Core.RegisterAsync(TBLog.Text.Trim(), passBxFrst.Password, passBxScnd.Password);

            if (success)
            {
                MessageBox.Show("Пользователь успешно зарегистрирован!");
                TBLog.Clear();
                passBxFrst.Clear();
                passBxScnd.Clear();
                NavigationService?.Navigate(new LoginPage());
            }
            else
            {
                MessageBox.Show("Пользователь с таким логином уже существует или произошла ошибка!");

            }
        }
    }
}