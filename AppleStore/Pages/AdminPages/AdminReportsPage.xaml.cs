using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using AppleStore.Models;

namespace AppleStore.Pages.AdminPages
{
    public partial class AdminReportsPage : Page
    {
        public AdminReportsPage()
        {
            InitializeComponent();
            btnSalesReport.Click += SalesReport_Click;
            btnProductsReport.Click += ProductsReport_Click;
            btnCustomersReport.Click += CustomersReport_Click;
        }

        private void SalesReport_Click(object sender, RoutedEventArgs e)
        {
            string defaultName = "Отчет по продажам";
            var dialog = new FormatSelectionWindow(defaultName, true);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() != true) return;

            string format = dialog.SelectedFormat;
            string reportName = dialog.ReportName;
            DateTime startDate = dialog.StartDate;
            DateTime endDate = dialog.EndDate;

            try
            {
                string query = @"SELECT o.OrdID, o.OrderDate, o.TotalAmount, o.Status,
                                ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') as CustomerName
                               FROM Orders o
                               LEFT JOIN Users u ON o.UserID = u.UserID
                               WHERE o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate
                               ORDER BY o.OrderDate DESC";

                SqlParameter[] parameters = {
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var result = DatabaseHelper.ExecuteReader(query, reader => new
                {
                    OrderID = reader.GetInt32(0),
                    OrderDate = reader.GetDateTime(1),
                    TotalAmount = reader.GetDecimal(2),
                    Status = reader.GetString(3),
                    CustomerName = string.IsNullOrWhiteSpace(reader.GetString(4)) ? "Неизвестный клиент" : reader.GetString(4).Trim()
                }, parameters);

                string finalReportName = $"{reportName} с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";

                string filePath = "";
                if (format == "Word")
                {
                    filePath = ReportHelper.ExportSalesReportWord(finalReportName, startDate, endDate, result);
                }
                else
                {
                    filePath = ReportHelper.ExportSalesReportExcel(finalReportName, startDate, endDate, result);
                }

                ShowOpenDialog(filePath);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ProductsReport_Click(object sender, RoutedEventArgs e)
        {
            string defaultName = "Отчет по товарам";
            var dialog = new FormatSelectionWindow(defaultName, false);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() != true) return;

            string format = dialog.SelectedFormat;
            string reportName = dialog.ReportName;

            try
            {
                string query = @"SELECT ProductName, StockQuantity, Price 
                               FROM Products ORDER BY ProductName";

                var result = DatabaseHelper.ExecuteReader(query, reader => new
                {
                    ProductName = reader.GetString(0),
                    StockQuantity = reader.GetInt32(1),
                    Price = reader.GetDecimal(2)
                });

                string finalReportName = $"{reportName} на {DateTime.Now:dd.MM.yyyy}";

                string filePath = "";
                if (format == "Word")
                {
                    filePath = ReportHelper.ExportProductsReportWord(finalReportName, result);
                }
                else
                {
                    filePath = ReportHelper.ExportProductsReportExcel(finalReportName, result);
                }

                ShowOpenDialog(filePath);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CustomersReport_Click(object sender, RoutedEventArgs e)
        {
            string defaultName = "Отчет по клиентам";
            var dialog = new FormatSelectionWindow(defaultName, true);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() != true) return;

            string format = dialog.SelectedFormat;
            string reportName = dialog.ReportName;
            DateTime startDate = dialog.StartDate;
            DateTime endDate = dialog.EndDate;

            try
            {
                string query = @"SELECT ISNULL(u.FirstName, '') + ' ' + ISNULL(u.LastName, '') as CustomerName,
                                COUNT(o.OrdID) as OrderCount,
                                ISNULL(SUM(o.TotalAmount), 0) as TotalSpent
                               FROM Users u
                               LEFT JOIN Orders o ON u.UserID = o.UserID
                               WHERE u.RoleID = 1 AND o.OrderDate >= @StartDate AND o.OrderDate <= @EndDate
                               GROUP BY u.FirstName, u.LastName
                               ORDER BY TotalSpent DESC";

                SqlParameter[] parameters = {
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                };

                var result = DatabaseHelper.ExecuteReader(query, reader => new
                {
                    CustomerName = string.IsNullOrWhiteSpace(reader.GetString(0)) ? "Неизвестный клиент" : reader.GetString(0).Trim(),
                    OrderCount = reader.GetInt32(1),
                    TotalSpent = reader.GetDecimal(2)
                }, parameters);

                string finalReportName = $"{reportName} с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";

                string filePath = "";
                if (format == "Word")
                {
                    filePath = ReportHelper.ExportCustomersReportWord(finalReportName, result);
                }
                else
                {
                    filePath = ReportHelper.ExportCustomersReportExcel(finalReportName, result);
                }

                ShowOpenDialog(filePath);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void ShowOpenDialog(string filePath)
        {
            var result = MessageBox.Show($"Отчет создан!\n\nОткрыть его?",
                                          "Успех",
                                          MessageBoxButton.YesNo,
                                          MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }
    }
}