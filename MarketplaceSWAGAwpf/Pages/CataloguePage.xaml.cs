using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MarketplaceSWAGAwpf.Models;
using MarketplaceSWAGAwpf.Pages;

namespace MarketplaceSWAGAwpf.Pages
{
    public partial class CataloguePage : Page
    {
        private List<ProductDto> _allProducts = new();

        public CataloguePage()
        {
            InitializeComponent();
            LoadCategories();
            CBSort.SelectedIndex = 0;
            LoadProducts();
        }

        private void LoadCategories()
        {
            var displayCategories = new List<CategoryDto>
            {
                new CategoryDto { Id = 0, Name = "Все категории" }
            };
            displayCategories.AddRange(Lists.Categories);

            CBFilter.ItemsSource = displayCategories;
            CBFilter.DisplayMemberPath = "Name";
            CBFilter.SelectedValuePath = "Id";
            CBFilter.SelectedIndex = 0;
        }

        private async void LoadProducts()
        {
            int? catId = null;
            if (CBFilter.SelectedItem is CategoryDto sel && sel.Id > 0)
                catId = sel.Id;

            decimal? min = null, max = null;
            if (decimal.TryParse(TBMinPrice.Text.Replace(',', '.'), out decimal mn))
                min = mn;
            if (decimal.TryParse(TBMaxPrice.Text.Replace(',', '.'), out decimal mx))
                max = mx;

            _allProducts = await Core.GetProductsAsync(
                TBSearch.Text.Trim(), catId, min, max);
            LBCatalogue.ItemsSource = _allProducts;

            ApplySort();

        }

        private void ApplySort()
        {
            IEnumerable<ProductDto> sorted = _allProducts;

            switch (CBSort.SelectedIndex)
            {
                case 1:
                    sorted = sorted.OrderBy(p => p.Price);
                    break;
                case 2:
                    sorted = sorted.OrderByDescending(p => p.Price);
                    break;
                case 3:
                    sorted = sorted.OrderBy(p => p.Name);
                    break;
                case 4:
                    sorted = sorted.OrderByDescending(p => p.Name);
                    break;
                default:
                    sorted = sorted.OrderBy(p => p.Id);
                    break;
            }

            LBCatalogue.ItemsSource = sorted.ToList();
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts();
        }

        private void CBSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySort();
        }

        private void CBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts();
        }

        private void LBCatalogue_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LBCatalogue.SelectedItem is ProductDto product)
            {
                NavigationService?.Navigate(new ProductDetailsPage(product.Id));
            }
        }
    }
}