using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AppleStore.Models;

namespace AppleStore.Pages.CustomerPages
{
    public partial class OrdersPage : Page
    {
        public OrdersPage()
        {
            InitializeComponent();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"SELECT OrdID, TotalAmount, Status, OrderDate 
                               FROM Orders WHERE UserID = @UserID ORDER BY OrderDate DESC";
                SqlParameter[] param = { new SqlParameter("@UserID", UserSession.UserID) };
                var orders = DatabaseHelper.ExecuteReader(query, reader => new Order
                {
                    OrdID = reader.GetInt32(0),
                    TotalAmount = reader.GetDecimal(1),
                    Status = reader.GetString(2),
                    OrderDate = reader.GetDateTime(3)
                }, param);
                OrdersGrid.ItemsSource = orders;
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}"); }
        }

        private void btnRefreshOrders_Click(object sender, RoutedEventArgs e) => LoadOrders();

        private void BtnViewOrderDetails_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button) || !(button.Tag is int orderId)) return;
            try
            {
                string details = GetOrderDetails(orderId);
                var scrollViewer = new ScrollViewer { MaxHeight = 450, Width = 500, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                var textBlock = new TextBlock { Text = details, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(15), FontFamily = new System.Windows.Media.FontFamily("Consolas"), FontSize = 13 };
                scrollViewer.Content = textBlock;
                var window = new Window
                {
                    Title = $"Детали заказа №{orderId}",
                    Content = scrollViewer,
                    Width = 550,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this),
                    ResizeMode = ResizeMode.CanResize,
                    ShowInTaskbar = false
                };
                window.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки деталей: {ex.Message}"); }
        }

        private string GetOrderDetails(int orderId)
        {
            var details = new StringBuilder();
            details.AppendLine($"Детали заказа №{orderId}");
            details.AppendLine(new string('=', 40));
            details.AppendLine();

            string orderQuery = @"SELECT TotalAmount, Status, OrderDate FROM Orders 
                         WHERE OrdID = @OrderID AND UserID = @UserID";
            SqlParameter[] orderParams =
            {
        new SqlParameter("@OrderID", orderId),
        new SqlParameter("@UserID", UserSession.UserID)
    };
            var orderInfo = DatabaseHelper.ExecuteReader(orderQuery, reader => new
            {
                TotalAmount = reader.GetDecimal(0),
                Status = reader.GetString(1),
                OrderDate = reader.GetDateTime(2)
            }, orderParams);

            if (orderInfo.Count > 0)
            {
                details.AppendLine($"Сумма: {orderInfo[0].TotalAmount:N2} ₽");
                details.AppendLine($"Статус: {orderInfo[0].Status}");
                details.AppendLine($"Дата: {orderInfo[0].OrderDate:dd.MM.yyyy HH:mm}");
                details.AppendLine();
            }

            details.AppendLine("Состав заказа:");
            details.AppendLine(new string('-', 30));

            string itemsQuery = @"SELECT p.ProductName, od.Quantity, od.UnitPrice 
                         FROM OrderDetails od INNER JOIN Products p ON od.ProdID = p.ProdID 
                         WHERE od.OrdID = @OrderID";
            SqlParameter[] itemsParams = { new SqlParameter("@OrderID", orderId) };
            var items = DatabaseHelper.ExecuteReader(itemsQuery, reader => new
            {
                ProductName = reader.GetString(0),
                Quantity = reader.GetInt32(1),
                UnitPrice = reader.GetDecimal(2)
            }, itemsParams);

            foreach (var item in items)
            {
                details.AppendLine($"{item.ProductName}: {item.Quantity}шт. × {item.UnitPrice:N2} ₽ = {(item.Quantity * item.UnitPrice):N2} ₽");
            }

            return details.ToString();
        }
    }
}