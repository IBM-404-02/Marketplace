using MarketplaceSWAGAwpf.Models;
using MarketplaceSWAGAwpf.Pages;
using System.Windows;
using System.Windows.Controls;

namespace MarketplaceSWAGAwpf
{
    public partial class AdminCategoriesPage : Page
    {
        public AdminCategoriesPage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            CategoriesDataGrid.ItemsSource = Lists.Categories;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditCategoryPage(null));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is CategoryDto category)
            {
                NavigationService.Navigate(new AddEditCategoryPage(category));
            }
            else
            {
                MessageBox.Show("Выберите категорию");
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriesDataGrid.SelectedItem is CategoryDto category)
            {
                var result = MessageBox.Show($"Удалить категорию '{category.Name}'?",
                    "Подтверждение", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    await Core.AdminDeleteCategoryAsync(category.Id);
                    await Lists.UpdateAsync();
                    LoadData();
                }
            }
            else
            {
                MessageBox.Show("Выберите категорию");
            }
        }
    }
}