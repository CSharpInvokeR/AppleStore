using System;
using System.Data.SqlClient;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AppleStore.Models;

namespace AppleStore.Pages.ManagerPages
{
    public partial class ManagerCustomersPage : Page
    {
        public ManagerCustomersPage()
        {
            InitializeComponent();
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            try
            {
                string query = @"SELECT u.UserID, u.FirstName, u.Patronymic, u.LastName, u.PhoneNumber, u.Email,
                       (SELECT COUNT(*) FROM Orders o WHERE o.UserID = u.UserID) as OrderCount
                       FROM Users u WHERE u.RoleID = 1 ORDER BY OrderCount DESC";
                var customers = DatabaseHelper.ExecuteReader(query, reader => new User
                {
                    UserID = reader.GetInt32(0),
                    FirstName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    Patronymic = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    LastName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    PhoneNumber = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    Email = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    OrderCount = reader.GetInt32(6)
                });
                CustomersGrid.ItemsSource = customers;
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки клиентов: {ex.Message}"); }
        }

        private void btnRefreshCustomers_Click(object sender, RoutedEventArgs e) => LoadCustomers();

        private void btnViewHistory_Click(object sender, RoutedEventArgs e)
        {
            if (CustomersGrid.SelectedItem is User selectedCustomer)
            {
                try
                {
                    string history = GetCustomerHistory(selectedCustomer);
                    MessageBox.Show(history, $"История заказов клиента", MessageBoxButton.OK);
                }
                catch (Exception ex) { MessageBox.Show($"Ошибка загрузки истории: {ex.Message}"); }
            }
            else MessageBox.Show("Выберите клиента для просмотра истории", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private string GetCustomerHistory(User customer)
        {
            var history = new StringBuilder();
            string fullName = $"{customer.FirstName} {customer.LastName} {customer.Patronymic} ".Trim();
            history.AppendLine($"{fullName}");
            history.AppendLine(new string('=', 46));
            history.AppendLine();

            string query = @"SELECT o.OrdID, o.TotalAmount, o.Status, o.OrderDate
                    FROM Orders o WHERE o.UserID = @UserID ORDER BY o.OrderDate DESC";
            SqlParameter[] parameters = { new SqlParameter("@UserID", customer.UserID) };
            var orders = DatabaseHelper.ExecuteReader(query, reader => new
            {
                OrdID = reader.GetInt32(0),
                TotalAmount = reader.GetDecimal(1),
                Status = reader.GetString(2),
                OrderDate = reader.GetDateTime(3)
            }, parameters);

            if (orders.Count == 0)
            {
                history.AppendLine("У клиента нет заказов");
            }
            else
            {
                decimal totalSum = 0;
                foreach (var order in orders)
                {
                    totalSum += order.TotalAmount;
                    history.AppendLine($"Заказ №{order.OrdID}");
                    history.AppendLine($"  Сумма: {order.TotalAmount:N2} ₽");
                    history.AppendLine($"  Статус: {order.Status}");
                    history.AppendLine($"  Дата: {order.OrderDate:dd.MM.yyyy HH:mm}");
                    history.AppendLine();
                }
                history.AppendLine(new string('-', 50));
                history.AppendLine($"Всего заказов: {orders.Count}");
                history.AppendLine($"Общая сумма: {totalSum:N2} ₽");
            }

            return history.ToString();
        }
    }
}