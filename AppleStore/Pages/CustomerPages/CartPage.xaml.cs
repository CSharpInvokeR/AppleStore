using System;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AppleStore.Models;

namespace AppleStore.Pages.CustomerPages
{
    public partial class CartPage : Page
    {
        private static ObservableCollection<CartItem> _cartItems = new ObservableCollection<CartItem>();

        public static ObservableCollection<CartItem> GetCartItems() => _cartItems;

        public static void LoadCartFromDatabase()
        {
            _cartItems.Clear();
            try
            {
                string query = @"SELECT ci.ProdID, ci.Quantity, p.ProductName, p.Price
                               FROM CartItems ci 
                               INNER JOIN Products p ON ci.ProdID = p.ProdID
                               WHERE ci.UserID = @UserID";
                SqlParameter[] param = { new SqlParameter("@UserID", UserSession.UserID) };
                var items = DatabaseHelper.ExecuteReader(query, reader => new CartItem
                {
                    ProdID = reader.GetInt32(0),
                    Quantity = reader.GetInt32(1),
                    ProductName = reader.GetString(2),
                    Price = reader.GetDecimal(3),
                    UserID = UserSession.UserID,
                    AddedDate = DateTime.Now
                }, param);
                foreach (var item in items) _cartItems.Add(item);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}"); }
        }

        public static void SaveCartToDatabase()
        {
            try
            {
                string deleteQuery = "DELETE FROM CartItems WHERE UserID = @UserID";
                SqlParameter[] param = { new SqlParameter("@UserID", UserSession.UserID) };
                DatabaseHelper.ExecuteNonQuery(deleteQuery, param);
                foreach (var item in _cartItems)
                {
                    string insertQuery = @"INSERT INTO CartItems (UserID, ProdID, Quantity, AddedDate) 
                                          VALUES (@UserID, @ProdID, @Quantity, @AddedDate)";
                    SqlParameter[] insertParams =
                    {
                        new SqlParameter("@UserID", UserSession.UserID),
                        new SqlParameter("@ProdID", item.ProdID),
                        new SqlParameter("@Quantity", item.Quantity),
                        new SqlParameter("@AddedDate", DateTime.Now)
                    };
                    DatabaseHelper.ExecuteNonQuery(insertQuery, insertParams);
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка сохранения корзины: {ex.Message}"); }
        }

        public static void AddToCart(int productId, string productName, decimal price)
        {
            var existing = _cartItems.FirstOrDefault(i => i.ProdID == productId);
            if (existing != null) existing.Quantity++;
            else
            {
                _cartItems.Add(new CartItem
                {
                    ProdID = productId,
                    ProductName = productName,
                    Price = price,
                    Quantity = 1,
                    UserID = UserSession.UserID,
                    AddedDate = DateTime.Now
                });
            }
            SaveCartToDatabase();
        }

        public static int GetProductStock(int productId)
        {
            string query = "SELECT StockQuantity FROM Products WHERE ProdID = @ProdID";
            SqlParameter[] param = { new SqlParameter("@ProdID", productId) };
            var result = DatabaseHelper.ExecuteScalar(query, param);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static void IncreaseQuantity(int productId)
        {
            var item = _cartItems.FirstOrDefault(i => i.ProdID == productId);
            if (item == null) return;
            int currentStock = GetProductStock(productId);
            if (item.Quantity + 1 > currentStock)
            {
                MessageBox.Show($"Нельзя увеличить количество. Доступно на складе: {currentStock} шт.",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            item.Quantity++;
            SaveCartToDatabase();
        }

        public static void DecreaseQuantity(int productId)
        {
            var item = _cartItems.FirstOrDefault(i => i.ProdID == productId);
            if (item != null && item.Quantity > 1)
            {
                item.Quantity--;
                SaveCartToDatabase();
            }
        }

        public static void RemoveFromCart(int productId)
        {
            var item = _cartItems.FirstOrDefault(i => i.ProdID == productId);
            if (item != null)
            {
                _cartItems.Remove(item);
                SaveCartToDatabase();
            }
        }

        public static void ClearCart()
        {
            _cartItems.Clear();
            SaveCartToDatabase();
        }

        public CartPage()
        {
            InitializeComponent();
            CartGrid.ItemsSource = _cartItems;
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal totalAmount = _cartItems.Sum(i => i.Total);
            int totalItems = _cartItems.Sum(i => i.Quantity);
            txtTotalAmount.Text = $"Общая сумма: {totalAmount:N2}₽";
            txtTotalItems.Text = $"Товаров: {totalItems}";
        }

        private void RefreshCart()
        {
            CartGrid.Items.Refresh();
            UpdateTotals();
        }

        private void BtnIncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                IncreaseQuantity(productId);
                RefreshCart();
            }
        }

        private void BtnDecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                DecreaseQuantity(productId);
                RefreshCart();
            }
        }

        private void BtnRemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is int productId)) return;
            var item = _cartItems.FirstOrDefault(i => i.ProdID == productId);
            if (item == null) return;

            var result = MessageBox.Show($"Удалить товар '{item.ProductName}' из корзины?",
                                          "Подтверждение удаления",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                RemoveFromCart(productId);
                RefreshCart();
            }
        }

        private void btnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0) return;
            var result = MessageBox.Show($"Очистить всю корзину ({_cartItems.Count} товаров)?",
                                          "Подтверждение",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ClearCart();
                RefreshCart();
            }
        }

        private void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var item in _cartItems)
            {
                int currentStock = GetProductStock(item.ProdID);
                if (item.Quantity > currentStock)
                {
                    MessageBox.Show($"Товар '{item.ProductName}' в количестве {item.Quantity} шт. недоступен.\n" +
                                  $"Доступно на складе: {currentStock} шт.\nПожалуйста, уменьшите количество и повторите попытку.",
                                  "Ошибка оформления", MessageBoxButton.OK, MessageBoxImage.Error);
                    RefreshCart();
                    return;
                }
            }

            try
            {
                decimal totalAmount = _cartItems.Sum(i => i.Total);
                string orderQuery = @"INSERT INTO Orders (UserID, TotalAmount, Status, OrderDate) 
                                     VALUES (@UserID, @TotalAmount, 'Новый', GETDATE()); SELECT SCOPE_IDENTITY();";
                SqlParameter[] orderParams = { new SqlParameter("@UserID", UserSession.UserID), new SqlParameter("@TotalAmount", totalAmount) };
                int orderId = Convert.ToInt32(DatabaseHelper.ExecuteScalar(orderQuery, orderParams));

                foreach (var item in _cartItems)
                {
                    string detailQuery = @"INSERT INTO OrderDetails (OrdID, ProdID, Quantity, UnitPrice) 
                                          VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice)";
                    SqlParameter[] detailParams =
                    {
                        new SqlParameter("@OrderID", orderId),
                        new SqlParameter("@ProductID", item.ProdID),
                        new SqlParameter("@Quantity", item.Quantity),
                        new SqlParameter("@UnitPrice", item.Price)
                    };
                    DatabaseHelper.ExecuteNonQuery(detailQuery, detailParams);

                    string updateStockQuery = "UPDATE Products SET StockQuantity = StockQuantity - @Quantity WHERE ProdID = @ProductID";
                    SqlParameter[] stockParams = { new SqlParameter("@Quantity", item.Quantity), new SqlParameter("@ProductID", item.ProdID) };
                    DatabaseHelper.ExecuteNonQuery(updateStockQuery, stockParams);
                }

                ClearCart();
                RefreshCart();
                MessageBox.Show($"Заказ №{orderId} успешно оформлен!\nСумма: {totalAmount:N2}₽",
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка оформления заказа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}