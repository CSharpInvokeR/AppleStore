using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppleStore.Models;

namespace AppleStore.Pages.CustomerPages
{
    public partial class CatalogPage : Page
    {
        private ObservableCollection<Product> products = new ObservableCollection<Product>();
        private string currentCategory = "Все";

        public CatalogPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            try
            {
                string query = @"SELECT p.ProdID, p.ProductName, c.CategoryName, p.Price, p.StockQuantity, p.Foto 
                               FROM Products p INNER JOIN Categories c ON p.CatID = c.CatID 
                               WHERE p.StockQuantity > 0 ORDER BY p.ProdID";
                products.Clear();
                var list = DatabaseHelper.ExecuteReader(query, reader => new Product
                {
                    ProdID = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    CategoryName = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    StockQuantity = reader.GetInt32(4),
                    Foto = reader.IsDBNull(5) ? "" : reader.GetString(5)
                });
                foreach (var p in list) products.Add(p);
                LoadCategories();
                ApplyFilter();
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}"); }
        }

        private void LoadCategories()
        {
            var categories = products.Select(p => p.CategoryName).Distinct().OrderBy(c => c).ToList();
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("Все");
            foreach (var cat in categories) cmbCategory.Items.Add(cat);
            cmbCategory.SelectedIndex = 0;
        }

        private void ApplyFilter()
        {
            var filtered = products.AsEnumerable();
            if (currentCategory != "Все")
                filtered = filtered.Where(p => p.CategoryName == currentCategory);
            ProductsItemsControl.ItemsSource = filtered.ToList();
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentCategory = cmbCategory.SelectedItem?.ToString() ?? "Все";
            ApplyFilter();
        }

        private void btnResetFilters_Click(object sender, RoutedEventArgs e) => cmbCategory.SelectedIndex = 0;
        private void btnRefreshCatalog_Click(object sender, RoutedEventArgs e) => LoadProducts();

        private int GetProductStock(int productId)
        {
            string query = "SELECT StockQuantity FROM Products WHERE ProdID = @ProdID";
            SqlParameter[] param = { new SqlParameter("@ProdID", productId) };
            var result = DatabaseHelper.ExecuteScalar(query, param);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            int productId = (int)button.Tag;
            var product = products.FirstOrDefault(p => p.ProdID == productId);
            if (product == null) return;

            int currentStock = GetProductStock(productId);
            if (currentStock <= 0)
            {
                MessageBox.Show($"Товар '{product.ProductName}' закончился на складе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = CartPage.GetCartItems().FirstOrDefault(i => i.ProdID == productId);
            int currentQuantityInCart = existing?.Quantity ?? 0;
            int newTotalQuantity = currentQuantityInCart + 1;

            if (newTotalQuantity > currentStock)
            {
                MessageBox.Show($"Нельзя добавить больше. Доступно на складе: {currentStock} шт.\nВ корзине уже {currentQuantityInCart} шт.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            CartPage.AddToCart(productId, product.ProductName, product.Price);
            string message = existing != null
                ? $"Товар '{product.ProductName}' добавлен в корзину!\nТеперь в корзине: {newTotalQuantity} шт."
                : $"Товар '{product.ProductName}' добавлен в корзину!";
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}