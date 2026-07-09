using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AppleStore.Models;

namespace AppleStore.Pages.ManagerPages
{
    public partial class ManagerProductsPage : Page
    {
        private Border selectedTile = null;
        private Product selectedProduct = null;

        public ManagerProductsPage()
        {
            InitializeComponent();
            LoadProducts();

            btnRefreshProducts.Click += (s, e) => LoadProducts();
            btnEditProduct.Click += EditProduct_Click;
        }

        private void LoadProducts()
        {
            try
            {
                string query = @"SELECT p.ProdID, p.ProductName, c.CategoryName, p.Price, p.StockQuantity, p.CatID, p.Foto, c.Warranty
                       FROM Products p INNER JOIN Categories c ON p.CatID = c.CatID";
                var products = DatabaseHelper.ExecuteReader(query, reader => new Product
                {
                    ProdID = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    CategoryName = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    StockQuantity = reader.GetInt32(4),
                    CatID = reader.GetInt32(5),
                    Foto = reader.IsDBNull(6) ? "picture.png" : reader.GetString(6)
                });
                ProductsGrid.ItemsSource = products;
                UpdateTileView(products);
                RestoreSelection();
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}"); }
        }

        private void UpdateTileView(dynamic products)
        {
            tilePanel.Children.Clear();

            foreach (var product in products)
            {
                var tile = CreateProductTile(product);
                tilePanel.Children.Add(tile);
            }
        }

        private Border CreateProductTile(dynamic product)
        {
            var border = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Background = System.Windows.Media.Brushes.White,
                Width = 210,
                Height = 290,
                Margin = new Thickness(8),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = product
            };

            border.MouseLeftButtonDown += (s, e) =>
            {
                SelectTile(border, product);
            };

            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var img = new System.Windows.Controls.Image
            {
                Width = 130,
                Height = 130,
                Stretch = System.Windows.Media.Stretch.Uniform,
                Margin = new Thickness(8),
                Source = GetImageSource(product.Foto)
            };
            Grid.SetRow(img, 0);
            grid.Children.Add(img);

            var txtName = new TextBlock
            {
                Text = product.ProductName,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13,
                Margin = new Thickness(10, 4, 10, 0),
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = 36
            };
            Grid.SetRow(txtName, 1);
            grid.Children.Add(txtName);

            var txtCategory = new TextBlock
            {
                Text = product.CategoryName,
                FontSize = 11,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(10, 2, 10, 0)
            };
            Grid.SetRow(txtCategory, 2);
            grid.Children.Add(txtCategory);

            var txtStock = new TextBlock
            {
                Text = $"Остаток: {product.StockQuantity} шт.",
                FontSize = 11,
                Foreground = System.Windows.Media.Brushes.Gray,
                Margin = new Thickness(10, 2, 10, 0)
            };
            Grid.SetRow(txtStock, 3);
            grid.Children.Add(txtStock);

            var txtPrice = new TextBlock
            {
                Text = $"{product.Price:N2} ₽",
                FontWeight = FontWeights.Bold,
                FontSize = 17,
                Foreground = System.Windows.Media.Brushes.Black,
                Margin = new Thickness(10, 8, 10, 12),
                VerticalAlignment = System.Windows.VerticalAlignment.Bottom
            };
            Grid.SetRow(txtPrice, 4);
            grid.Children.Add(txtPrice);

            border.Child = grid;
            return border;
        }

        private void SelectTile(Border border, dynamic product)
        {
            ClearTileSelection();
            border.BorderBrush = System.Windows.Media.Brushes.Black;
            border.BorderThickness = new Thickness(3);
            border.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F0F0F0");
            selectedTile = border;
            selectedProduct = product;
            if (ProductsGrid != null)
                ProductsGrid.SelectedItem = product;
        }

        private void RestoreSelection()
        {
            if (selectedProduct != null && tilePanel != null && tilePanel.Children.Count > 0)
            {
                foreach (Border tile in tilePanel.Children)
                {
                    var product = tile.Tag as Product;
                    if (product != null && product.ProdID == selectedProduct.ProdID)
                    {
                        ClearTileSelection();
                        tile.BorderBrush = System.Windows.Media.Brushes.Black;
                        tile.BorderThickness = new Thickness(3);
                        tile.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#F0F0F0");
                        selectedTile = tile;
                        if (ProductsGrid != null)
                            ProductsGrid.SelectedItem = product;
                        break;
                    }
                }
            }
        }

        private System.Windows.Media.ImageSource GetImageSource(string foto)
        {
            try
            {
                string imagePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", foto);
                if (System.IO.File.Exists(imagePath))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch { }
            return null;
        }

        private void ClearTileSelection()
        {
            if (selectedTile != null)
            {
                selectedTile.BorderBrush = System.Windows.Media.Brushes.LightGray;
                selectedTile.BorderThickness = new Thickness(1);
                selectedTile.Background = System.Windows.Media.Brushes.White;
                selectedTile = null;
            }
        }

        private void ShowTableView()
        {
            if (ProductsGrid != null)
                ProductsGrid.Visibility = Visibility.Visible;
            if (tileScrollViewer != null)
                tileScrollViewer.Visibility = Visibility.Collapsed;

            if (ProductsGrid != null)
                ProductsGrid.SelectedItem = selectedProduct;
        }

        private void ShowTileView()
        {
            if (ProductsGrid != null)
                ProductsGrid.Visibility = Visibility.Collapsed;
            if (tileScrollViewer != null)
                tileScrollViewer.Visibility = Visibility.Visible;

            RestoreSelection();
        }

        private void RbTableView_Checked(object sender, RoutedEventArgs e)
        {
            ShowTableView();
        }

        private void RbTileView_Checked(object sender, RoutedEventArgs e)
        {
            ShowTileView();
        }

        private void ProductsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsGrid != null && ProductsGrid.SelectedItem != null && tilePanel != null && tilePanel.Children.Count > 0)
            {
                var selected = ProductsGrid.SelectedItem as Product;
                if (selected != null)
                {
                    selectedProduct = selected;

                    foreach (Border tile in tilePanel.Children)
                    {
                        var product = tile.Tag as Product;
                        if (product != null && product.ProdID == selected.ProdID)
                        {
                            SelectTile(tile, product);
                            break;
                        }
                    }
                }
            }
        }

        private Product GetSelectedProduct()
        {
            if (selectedProduct != null)
                return selectedProduct;

            if (ProductsGrid != null && ProductsGrid.SelectedItem is Product selected)
                return selected;

            if (selectedTile != null && selectedTile.Tag is Product product)
                return product;

            return null;
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetSelectedProduct();
            if (selected != null)
            {
                var dialog = new EditProductWindow(selected);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    LoadProducts();
                    MessageBox.Show("Товар успешно обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else MessageBox.Show("Выберите товар", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}