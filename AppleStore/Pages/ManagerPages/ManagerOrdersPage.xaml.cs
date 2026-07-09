using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AppleStore.Models;

namespace AppleStore.Pages.ManagerPages
{
    public partial class ManagerOrdersPage : Page
    {
        private DispatcherTimer timer;

        public ManagerOrdersPage()
        {
            InitializeComponent();
            LoadOrders();

            btnRefreshOrders.Click += (s, e) => LoadOrders();
            btnChangeStatus.Click += ChangeOrderStatus_Click;
            btnViewDetails.Click += BtnViewDetails_Click;
        }

        private void LoadOrders()
        {
            try
            {
                string query = @"SELECT o.OrdID, ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') as CustomerName, 
                                       o.TotalAmount, o.Status, o.OrderDate 
                               FROM Orders o LEFT JOIN Users u ON o.UserID = u.UserID ORDER BY o.OrderDate DESC";
                var orders = DatabaseHelper.ExecuteReader(query, reader => new Order
                {
                    OrdID = reader.GetInt32(0),
                    CustomerName = string.IsNullOrWhiteSpace(reader.GetString(1)) ? "Неизвестный клиент" : reader.GetString(1).Trim(),
                    TotalAmount = reader.GetDecimal(2),
                    Status = reader.GetString(3),
                    OrderDate = reader.GetDateTime(4)
                });
                OrdersGrid.ItemsSource = orders;
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}"); }
        }

        private void ChangeOrderStatus_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is Order selectedOrder)
            {
                var statusWindow = new ChangeStatusWindow(selectedOrder.OrdID, selectedOrder.Status);
                statusWindow.Owner = Window.GetWindow(this);
                if (statusWindow.ShowDialog() == true)
                {
                    LoadOrders();
                    MessageBox.Show("Статус заказа обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для изменения статуса", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is Order selectedOrder)
            {
                try
                {
                    string details = GetOrderDetails(selectedOrder.OrdID);
                    MessageBox.Show(details, $"Детали заказа", MessageBoxButton.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки деталей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для просмотра деталей", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GetOrderDetails(int orderId)
        {
            var details = new System.Text.StringBuilder();
            details.AppendLine($"Заказ №{orderId}");
            details.AppendLine(new string('=', 40));
            details.AppendLine();

            string orderQuery = @"SELECT o.TotalAmount, o.Status, o.OrderDate, u.FirstName + ' ' + u.LastName as Customer
                         FROM Orders o LEFT JOIN Users u ON o.UserID = u.UserID WHERE o.OrdID = @OrderID";
            SqlParameter[] orderParams = { new SqlParameter("@OrderID", orderId) };
            var orderInfo = DatabaseHelper.ExecuteReader(orderQuery, reader => new
            {
                TotalAmount = reader.GetDecimal(0),
                Status = reader.GetString(1),
                OrderDate = reader.GetDateTime(2),
                Customer = reader.IsDBNull(3) ? "Неизвестный клиент" : reader.GetString(3)
            }, orderParams);

            if (orderInfo.Count > 0)
            {
                details.AppendLine($"Клиент: {orderInfo[0].Customer}");
                details.AppendLine($"Сумма: {orderInfo[0].TotalAmount:N2}₽");
                details.AppendLine($"Статус: {orderInfo[0].Status}");
                details.AppendLine($"Дата: {orderInfo[0].OrderDate:dd.MM.yyyy HH:mm}");
                details.AppendLine();
            }

            details.AppendLine("Состав заказа:");
            details.AppendLine(new string('-', 30));

            string itemsQuery = @"SELECT p.ProductName, od.Quantity, od.UnitPrice
                         FROM OrderDetails od INNER JOIN Products p ON od.ProdID = p.ProdID WHERE od.OrdID = @OrderID";
            SqlParameter[] itemsParams = { new SqlParameter("@OrderID", orderId) };
            var items = DatabaseHelper.ExecuteReader(itemsQuery, reader => new
            {
                ProductName = reader.GetString(0),
                Quantity = reader.GetInt32(1),
                UnitPrice = reader.GetDecimal(2)
            }, itemsParams);

            foreach (var item in items)
            {
                details.AppendLine($"{item.ProductName}: {item.Quantity}шт. × {item.UnitPrice:N2}₽ = {(item.Quantity * item.UnitPrice):N2}₽");
            }

            return details.ToString();
        }
    }
}