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
    /// Логика взаимодействия для AdminProductsPage.xaml
    /// </summary>
    public partial class AdminProductsPage : Page
    {
        public AdminProductsPage()
        {
            InitializeComponent();
            LoadData();
        }

        private async void LoadData()
        {
            var products = await Core.GetProductsAsync();
            ProductsDataGrid.ItemsSource = products
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditProductPage(null));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is ProductDto product)
            {
                NavigationService.Navigate(new AddEditProductPage(product));
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования");
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is ProductDto product)
            {
                var result = MessageBox.Show($"Удалить товар '{product.Name}'?", "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    await Core.AdminDeleteProductAsync(product.Id);
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления");
            }
        }
    }
}