using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using AppleStore.Models;

namespace AppleStore.Pages.AdminPages
{
    public partial class ProductEditWindow : Window
    {
        public string ProductName { get; private set; }
        public int CategoryId { get; private set; }
        public decimal Price { get; private set; }
        public int Stock { get; private set; }
        public string Foto { get; private set; }

        public ProductEditWindow(string name = "", int catId = 1, decimal price = 0, int stock = 0, string foto = "")
        {
            InitializeComponent();
            LoadCategories();

            txtName.Text = name;
            txtPrice.Text = price.ToString();
            txtStock.Text = stock.ToString();
            txtFoto.Text = foto;

            if (catId > 0)
            {
                for (int i = 0; i < cmbCategory.Items.Count; i++)
                {
                    var item = cmbCategory.Items[i] as Category;
                    if (item != null && item.CatID == catId)
                    {
                        cmbCategory.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void LoadCategories()
        {
            try
            {
                string query = "SELECT CatID, CategoryName, Warranty FROM Categories ORDER BY CatID";
                var categories = DatabaseHelper.ExecuteReader(query, reader => new Category
                {
                    CatID = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    Warranty = reader.IsDBNull(2) ? 0 : reader.GetInt32(2)
                });

                foreach (var cat in categories)
                {
                    cmbCategory.Items.Add(cat);
                }
                cmbCategory.DisplayMemberPath = "CategoryName";
                cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void btnSelectFoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Title = "Выберите фото товара";
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            if (dialog.ShowDialog() == true)
            {
                string fileName = System.IO.Path.GetFileName(dialog.FileName);
                string projectPath = AppDomain.CurrentDomain.BaseDirectory;
                string resourcesPath = System.IO.Path.Combine(projectPath, "Resources");

                try
                {
                    if (!Directory.Exists(resourcesPath))
                    {
                        Directory.CreateDirectory(resourcesPath);
                    }

                    string destPath = System.IO.Path.Combine(resourcesPath, fileName);
                    File.Copy(dialog.FileName, destPath, true);
                    txtFoto.Text = fileName;

                    MessageBox.Show($"Фото скопировано!\nПапка: {resourcesPath}\nИмя файла: {fileName}", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка копирования файла: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (txtName.Text.Length < 3)
            {
                MessageBox.Show("Название должно содержать минимум 3 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (cmbCategory.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbCategory.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (больше 0)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Введите корректное количество (не отрицательное)", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtStock.Focus();
                return;
            }

            ProductName = txtName.Text.Trim();
            CategoryId = (cmbCategory.SelectedItem as Category).CatID;
            Price = price;
            Stock = stock;
            Foto = string.IsNullOrWhiteSpace(txtFoto.Text) ? "picture.png" : txtFoto.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}