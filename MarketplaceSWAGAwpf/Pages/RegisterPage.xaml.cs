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
    /// Логика взаимодействия для RegisterPage.xaml
    /// </summary>
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            CBRole.SelectedIndex = 0;
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
                if (char.IsDigit(c))
                {
                    hasDigit = true;
                }
                else if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                {
                    onlyEnglish = false;
                }
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

        private void regButton_Click(object sender, RoutedEventArgs e)
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

            var userExists = Core.Context.Users
                .AsNoTracking()
                .Any(u => u.Login == TBLog.Text.Trim());

            if (userExists)
            {
                MessageBox.Show("Пользователь с таким логином уже существует!");
                return;
            }

            int roleId = 1;
            if (CBRole.SelectedItem is ComboBoxItem selected && selected.Content?.ToString() == "Администратор")
            {
                roleId = 2;
            }

            try
            {
                Users newUser = new Users
                {
                    Login = TBLog.Text.Trim(),
                    PasswordHash = Core.GetHash(passBxFrst.Password),
                    RoleId = roleId
                };

                Core.Context.Users.Add(newUser);
                Core.Context.SaveChanges();

                MessageBox.Show("Пользователь успешно зарегистрирован!");
                 
                TBLog.Clear();
                passBxFrst.Clear();
                passBxScnd.Clear();
                CBRole.SelectedIndex = 0;

                NavigationService?.Navigate(new LoginPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении в базу данных: {ex.Message}");
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
