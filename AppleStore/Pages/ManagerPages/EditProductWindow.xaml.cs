using System;
using System.Data.SqlClient;
using System.Windows;
using AppleStore.Models;

namespace AppleStore
{
    public partial class EditProductWindow : Window
    {
        private Product _product;

        public EditProductWindow(Product product)
        {
            InitializeComponent();
            _product = product;
            LoadProductData();
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        private void LoadProductData()
        {
            txtProductName.Text = _product.ProductName;
            txtPrice.Text = _product.Price.ToString();
            txtStock.Text = _product.StockQuantity.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock))
            {
                MessageBox.Show("Введите корректное количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string query = "UPDATE Products SET Price = @Price, StockQuantity = @Stock WHERE ProdID = @ProductID";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Price", price),
                    new SqlParameter("@Stock", stock),
                    new SqlParameter("@ProductID", _product.ProdID)
                };
                DatabaseHelper.ExecuteNonQuery(query, parameters);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}