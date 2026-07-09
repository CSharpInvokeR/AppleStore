using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AppleStore.Models;
using AppleStore.Pages.ManagerPages;

namespace AppleStore.Pages.AdminPages
{
    public partial class AdminOrdersPage : Page
    {
        private DispatcherTimer timer;

        public AdminOrdersPage()
        {
            InitializeComponent();
            LoadOrders();

            btnRefreshOrders.Click += (s, e) => LoadOrders();
            btnChangeStatus.Click += ChangeOrderStatus_Click;
            btnDeleteOrder.Click += DeleteOrder_Click;
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
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки: {ex.Message}"); }
        }

        private void ChangeOrderStatus_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is Order selected)
            {
                var dialog = new ChangeStatusWindow(selected.OrdID, selected.Status);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    LoadOrders();
                    MessageBox.Show("Статус обновлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else MessageBox.Show("Выберите заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (OrdersGrid.SelectedItem is Order selected)
            {
                var result = MessageBox.Show($"Удалить заказ №{selected.OrdID}?",
                              "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        SqlParameter[] parameters1 = { new SqlParameter("@OrderID", selected.OrdID) };
                        DatabaseHelper.ExecuteNonQuery("DELETE FROM OrderDetails WHERE OrdID = @OrderID", parameters1);

                        SqlParameter[] parameters2 = { new SqlParameter("@OrderID", selected.OrdID) };
                        DatabaseHelper.ExecuteNonQuery("DELETE FROM Orders WHERE OrdID = @OrderID", parameters2);

                        MessageBox.Show("Заказ удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else MessageBox.Show("Выберите заказ", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}