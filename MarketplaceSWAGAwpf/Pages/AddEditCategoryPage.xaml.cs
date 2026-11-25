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
    public partial class AddEditCategoryPage : Page
    {
        private readonly CategoryDto _editingCategory;

        public AddEditCategoryPage(CategoryDto category)
        {
            InitializeComponent();
            _editingCategory = category;
            if (_editingCategory != null)
            {
                NameTextBox.Text = _editingCategory.Name;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Название не может быть пустым");
                return;
            }

            string name = NameTextBox.Text.Trim();

            if (_editingCategory == null)
            {
                await Core.AdminAddCategoryAsync(name);
            }
            else
            {
                await Core.AdminUpdateCategoryAsync(_editingCategory.Id, name);
            }

            await Lists.UpdateAsync();
            NavigationService.Navigate(new AdminCategoriesPage());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminCategoriesPage());
        }
    }
}