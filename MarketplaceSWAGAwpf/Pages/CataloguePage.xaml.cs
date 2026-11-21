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
    /// Логика взаимодействия для CataloguePage.xaml
    /// </summary>
    public partial class CataloguePage : Page
    {
        public CataloguePage()
        {
            InitializeComponent();
            LoadCategories();
            CBSort.SelectedIndex = 0;
            UpdateSearch();
        }

        private void LoadCategories()
        {
            var displayCategories = new List<Categories>
            {
                new Categories { Id = 0, Name = "Все категории" }
            };
            displayCategories.AddRange(Lists.Categories);

            CBFilter.ItemsSource = displayCategories;
            CBFilter.DisplayMemberPath = "Name";
            CBFilter.SelectedValuePath = "Id";
            CBFilter.SelectedIndex = 0;
        }

        private void UpdateSearch()
        { 
            IEnumerable<Products> filteredProducts = Lists.Products;

            if (!string.IsNullOrWhiteSpace(TBSearch.Text))
            {
                string searchText = TBSearch.Text.ToLower().Trim();
                filteredProducts = filteredProducts.Where(p => p.Name != null && p.Name.ToLower().Contains(searchText));
            }
            if (CBFilter.SelectedItem is Categories selectedCategory && selectedCategory.Id != 0)
            {
                filteredProducts = filteredProducts.Where(p => p.ProductCategories.Any(pc => pc.CategoryId == selectedCategory.Id));
            }

            if (decimal.TryParse(TBMinPrice.Text.Replace(',', '.'), out decimal minPrice))
            {
                filteredProducts = filteredProducts.Where(p => p.Price >= minPrice);
            }
            if (decimal.TryParse(TBMaxPrice.Text.Replace(',', '.'), out decimal maxPrice))
            {
                filteredProducts = filteredProducts.Where(p => p.Price <= maxPrice);
            }

            switch (CBSort.SelectedIndex)
            {
                case 1: 
                    filteredProducts = filteredProducts.OrderBy(p => p.Price); 
                    break;
                case 2: 
                    filteredProducts = filteredProducts.OrderByDescending(p => p.Price);
                    break;
                case 3: 
                    filteredProducts = filteredProducts.OrderBy(p => p.Name); 
                    break;
                case 4: 
                    filteredProducts = filteredProducts.OrderByDescending(p => p.Name); 
                    break;
                default:
                    filteredProducts = filteredProducts.OrderBy(p => p.Id); 
                    break;
            }

            LBCatalogue.ItemsSource = filteredProducts.ToList();
        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSearch();
        }

        private void CBSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSearch();
        }

        private void CBFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSearch();
        }

        private void LBCatalogue_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LBCatalogue.SelectedItem is Products product)
            {
                var main = (MainWindow)Application.Current.MainWindow;
                main.MainFrame.Navigate(new ProductDetailsPage(product.Id));
            }
        }
    }
}
