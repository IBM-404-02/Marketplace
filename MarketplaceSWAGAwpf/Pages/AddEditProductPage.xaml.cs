using MarketplaceSWAGAwpf.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MarketplaceSWAGAwpf.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditProductPage.xaml
    /// </summary>
    public partial class AddEditProductPage : Page
    {
        private readonly ProductDto _editingProduct;

        public AddEditProductPage(ProductDto product)
        {
            InitializeComponent();

            CategoryComboBox.ItemsSource = Lists.Categories;
            _editingProduct = product;

            this.Loaded += AddEditProductPage_Loaded;
        }

        private async void AddEditProductPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_editingProduct != null)
            {
                TitleText.Text = "Редактировать товар";
                NameTextBox.Text = _editingProduct.Name;
                PriceTextBox.Text = _editingProduct.Price.ToString();
                StockTextBox.Text = _editingProduct.StockQuantity.ToString();
                ImageTextBox.Text = _editingProduct.ImagePath;
                IsAvailableCheckBox.IsChecked = _editingProduct.IsAvailable;

                var details = await Core.GetProductAsync(_editingProduct.Id);
                if (details != null)
                {
                    DescriptionTextBox.Text = details.Description;
                }

                var currentCategory = Lists.Categories.FirstOrDefault(c => _editingProduct.Categories.Contains(c.Name));
                if (currentCategory != null)
                {
                    CategoryComboBox.SelectedValue = currentCategory.Id;
                }
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            ErrorText.Text = "";

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ErrorText.Text = "Название не может быть пустым";
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price <= 0)
            {
                ErrorText.Text = "Введите корректную цену (число больше 0)";
                return;
            }

            if (!int.TryParse(StockTextBox.Text, out int stock) || stock < 0)
            {
                ErrorText.Text = "Введите корректное количество (целое число >= 0)";
                return;
            }

            if (CategoryComboBox.SelectedItem == null)
            {
                ErrorText.Text = "Выберите категорию";
                return;
            }

            int categoryId = (int)CategoryComboBox.SelectedValue;
            var categoryIds = new List<int> { categoryId };

            if (_editingProduct == null)
            {
                await Core.AdminAddProductAsync(
                    NameTextBox.Text.Trim(),
                    DescriptionTextBox.Text.Trim(),
                    price,
                    ImageTextBox.Text.Trim(),
                    stock,
                    1,
                    IsAvailableCheckBox.IsChecked ?? true,
                    categoryIds);
            }
            else
            {
                await Core.AdminUpdateProductAsync(
                    _editingProduct.Id,
                    NameTextBox.Text.Trim(),
                    DescriptionTextBox.Text.Trim(),
                    price,
                    ImageTextBox.Text.Trim(),
                    stock,
                    1,
                    IsAvailableCheckBox.IsChecked ?? true,
                    categoryIds);
            }

            NavigationService.Navigate(new AdminProductsPage());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdminProductsPage());
        }
    }
}