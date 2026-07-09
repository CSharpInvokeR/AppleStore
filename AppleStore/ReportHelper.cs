using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace AppleStore
{
    public static class ReportHelper
    {
        private static string GetDesktopPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        public static string ExportSalesReportWord(string reportName, DateTime startDate, DateTime endDate, dynamic data)
        {
            return CreateWordDocument(reportName, (doc, selection) =>
            {
                selection.TypeText($"=== {reportName} ===");
                selection.TypeParagraph();
                selection.TypeParagraph();
                selection.TypeText($"Период: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}");
                selection.TypeParagraph();
                selection.TypeText($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}");
                selection.TypeParagraph();
                selection.TypeText($"Создано: {UserSession.FirstName} {UserSession.LastName}");
                selection.TypeParagraph();
                selection.TypeText(new string('=', 70));
                selection.TypeParagraph();
                selection.TypeParagraph();
                selection.TypeText($"{"№",-5} {"Дата",-12} {"Клиент",-25} {"Сумма",-15} {"Статус",-15}");
                selection.TypeParagraph();
                selection.TypeText(new string('-', 72));
                selection.TypeParagraph();

                decimal totalSum = 0;
                int count = 0;
                foreach (var item in data)
                {
                    count++;
                    totalSum += item.TotalAmount;
                    string customer = item.CustomerName ?? "Неизвестный клиент";
                    if (customer.Length > 23) customer = customer.Substring(0, 20) + "...";
                    string dateStr = item.OrderDate.ToString("dd.MM.yyyy");
                    selection.TypeText($"{count,-5} {dateStr,-12} {customer,-25} {item.TotalAmount:N2} ₽, {" " + item.Status,-15}");
                    selection.TypeParagraph();
                }

                selection.TypeText(new string('-', 72));
                selection.TypeParagraph();
                selection.TypeText($"{"ИТОГО заказов:",-42} {count,-15}");
                selection.TypeParagraph();
                selection.TypeText($"{"ИТОГО сумма:",-42} {totalSum:N2} ₽");
                selection.TypeParagraph();
                selection.TypeText(new string('=', 72));
                selection.TypeParagraph();
            });
        }

        public static string ExportProductsReportWord(string reportName, dynamic data)
        {
            return CreateWordDocument(reportName, (doc, selection) =>
            {
                selection.TypeText($"=== {reportName} ===");
                selection.TypeParagraph();
                selection.TypeParagraph();
                selection.TypeText($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}");
                selection.TypeParagraph();
                selection.TypeText($"Создано: {UserSession.FirstName} {UserSession.LastName}");
                selection.TypeParagraph();
                selection.TypeText(new string('=', 70));
                selection.TypeParagraph();
                selection.TypeParagraph();
                selection.TypeText($"{"№",-5} {"Товар",-40} {"Остаток",-10} {"Цена",-15}");
                selection.TypeParagraph();
                selection.TypeText(new string('-', 70));
                selection.TypeParagraph();

                decimal totalPrice = 0;
                int totalStock = 0;
                int count = 0;
                foreach (var item in data)
                {
                    count++;
                    decimal total = item.Price * item.StockQuantity;
                    totalPrice += total;
                    totalStock += item.StockQuantity;
                    string name = item.ProductName?.Length > 38 ? item.ProductName.Substring(0, 35) + "..." : item.ProductName;
                    selection.TypeText($"{count,-5} {name,-40} {item.StockQuantity,-10} {item.Price:N2} ₽");
                    selection.TypeParagraph();
                }

                selection.TypeText(new string('-', 70));
                selection.TypeParagraph();
                selection.TypeText($"{"ИТОГО:",-45} {totalStock,-10} {totalPrice:N2} ₽");
                selection.TypeParagraph();
                selection.TypeText(new string('=', 70));
                selection.TypeParagraph();
            });
        }

        public static string ExportCustomersReportWord(string reportName, dynamic data)
        {
            return CreateWordDocument(reportName, (doc, selection) =>
            {
                selection.TypeText($"=== {reportName} ===");
                selection.TypeParagraph();
                selection.TypeParagraph();
                selection.TypeText($"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}");
                selection.TypeParagraph();
                selection.TypeText($"Создано: {UserSession.FirstName} {UserSession.LastName}");
                selection.TypeParagraph();
                selection.TypeText(new string('=', 70));
                selection.TypeParagraph();
                selection.TypeParagraph();
                selection.TypeText($"{"№",-5} {"Клиент",-35} {"Заказов",-10} {"Сумма",-15}");
                selection.TypeParagraph();
                selection.TypeText(new string('-', 65));
                selection.TypeParagraph();

                int totalOrders = 0;
                decimal totalSum = 0;
                int count = 0;
                foreach (var item in data)
                {
                    count++;
                    totalOrders += item.OrderCount;
                    totalSum += item.TotalSpent;
                    string name = item.CustomerName?.Length > 33 ? item.CustomerName.Substring(0, 30) + "..." : item.CustomerName;
                    selection.TypeText($"{count,-5} {name,-35} {item.OrderCount,-10} {item.TotalSpent:N2} ₽");
                    selection.TypeParagraph();
                }

                selection.TypeText(new string('-', 65));
                selection.TypeParagraph();
                selection.TypeText($"{"ИТОГО:",-40} {totalOrders,-10} {totalSum:N2} ₽");
                selection.TypeParagraph();
                selection.TypeText(new string('=', 65));
                selection.TypeParagraph();
            });
        }

        private static string CreateWordDocument(string reportName, Action<Microsoft.Office.Interop.Word.Document, Microsoft.Office.Interop.Word.Selection> buildContent)
        {
            Microsoft.Office.Interop.Word.Application wordApp = null;
            Microsoft.Office.Interop.Word.Document doc = null;

            try
            {
                wordApp = new Microsoft.Office.Interop.Word.Application();
                wordApp.Visible = false;
                doc = wordApp.Documents.Add();

                var selection = wordApp.Selection;
                selection.Font.Name = "Courier New";
                selection.Font.Size = 10;

                buildContent(doc, selection);

                string desktopPath = GetDesktopPath();
                string safeName = string.Join("_", reportName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                string filePath = Path.Combine(desktopPath, fileName);

                doc.SaveAs2(filePath);
                doc.Close();
                wordApp.Quit();

                return filePath;
            }
            catch (Exception ex)
            {
                if (doc != null) doc.Close(false);
                if (wordApp != null) wordApp.Quit();
                throw new Exception($"Ошибка создания Word документа: {ex.Message}");
            }
        }

        public static string ExportSalesReportExcel(string reportName, DateTime startDate, DateTime endDate, dynamic data)
        {
            try
            {
                var excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = false;
                var workbook = excelApp.Workbooks.Add();
                var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Отчет по продажам";

                worksheet.Cells[1, 1] = reportName;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1]).Font.Bold = true;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1]).Font.Size = 16;
                worksheet.Cells[2, 1] = $"Период: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
                worksheet.Cells[3, 1] = $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}";
                worksheet.Cells[4, 1] = $"Создано: {UserSession.FirstName} {UserSession.LastName}";

                string[] headers = { "№", "Дата", "Клиент", "Сумма", "Статус" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[6, i + 1] = headers[i];
                    ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[6, i + 1]).Font.Bold = true;
                    ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[6, i + 1]).Interior.Color = System.Drawing.Color.LightGray;
                }

                int row = 7;
                decimal totalSum = 0;
                int count = 0;
                foreach (var item in data)
                {
                    count++;
                    totalSum += item.TotalAmount;
                    worksheet.Cells[row, 1] = count;
                    worksheet.Cells[row, 2] = item.OrderDate.ToString("dd.MM.yyyy");
                    worksheet.Cells[row, 3] = item.CustomerName ?? "Неизвестный клиент";
                    worksheet.Cells[row, 4] = item.TotalAmount;
                    worksheet.Cells[row, 5] = item.Status;
                    row++;
                }

                worksheet.Cells[row, 3] = "ИТОГО заказов:";
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 3]).Font.Bold = true;
                worksheet.Cells[row, 4] = count;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 4]).Font.Bold = true;
                row++;

                worksheet.Cells[row, 3] = "ИТОГО сумма:";
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 3]).Font.Bold = true;
                worksheet.Cells[row, 4] = totalSum;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 4]).Font.Bold = true;

                worksheet.Columns.AutoFit();

                string desktopPath = GetDesktopPath();
                string safeName = string.Join("_", reportName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(desktopPath, fileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания Excel: {ex.Message}");
            }
        }

        public static string ExportProductsReportExcel(string reportName, dynamic data)
        {
            try
            {
                var excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = false;
                var workbook = excelApp.Workbooks.Add();
                var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Отчет по товарам";

                worksheet.Cells[1, 1] = reportName;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1]).Font.Bold = true;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1]).Font.Size = 16;
                worksheet.Cells[2, 1] = $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}";
                worksheet.Cells[3, 1] = $"Создано: {UserSession.FirstName} {UserSession.LastName}";

                string[] headers = { "№", "Товар", "Остаток", "Цена", "Общая стоимость" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[5, i + 1] = headers[i];
                    ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[5, i + 1]).Font.Bold = true;
                    ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[5, i + 1]).Interior.Color = System.Drawing.Color.LightGray;
                }

                int row = 6;
                decimal totalPrice = 0;
                int totalStock = 0;
                int count = 0;
                foreach (var item in data)
                {
                    count++;
                    decimal total = item.Price * item.StockQuantity;
                    totalPrice += total;
                    totalStock += item.StockQuantity;
                    worksheet.Cells[row, 1] = count;
                    worksheet.Cells[row, 2] = item.ProductName;
                    worksheet.Cells[row, 3] = item.StockQuantity;
                    worksheet.Cells[row, 4] = item.Price;
                    worksheet.Cells[row, 5] = total;
                    row++;
                }

                worksheet.Cells[row, 2] = "ИТОГО:";
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 2]).Font.Bold = true;
                worksheet.Cells[row, 3] = totalStock;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 3]).Font.Bold = true;
                worksheet.Cells[row, 5] = totalPrice;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 5]).Font.Bold = true;

                worksheet.Columns.AutoFit();

                string desktopPath = GetDesktopPath();
                string safeName = string.Join("_", reportName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(desktopPath, fileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания Excel: {ex.Message}");
            }
        }

        public static string ExportCustomersReportExcel(string reportName, dynamic data)
        {
            try
            {
                var excelApp = new Microsoft.Office.Interop.Excel.Application();
                excelApp.Visible = false;
                var workbook = excelApp.Workbooks.Add();
                var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];
                worksheet.Name = "Отчет по клиентам";

                worksheet.Cells[1, 1] = reportName;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1]).Font.Bold = true;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[1, 1]).Font.Size = 16;
                worksheet.Cells[2, 1] = $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}";
                worksheet.Cells[3, 1] = $"Создано: {UserSession.FirstName} {UserSession.LastName}";

                string[] headers = { "№", "Клиент", "Заказов", "Сумма" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[5, i + 1] = headers[i];
                    ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[5, i + 1]).Font.Bold = true;
                    ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[5, i + 1]).Interior.Color = System.Drawing.Color.LightGray;
                }

                int row = 6;
                int totalOrders = 0;
                decimal totalSum = 0;
                int count = 0;
                foreach (var item in data)
                {
                    count++;
                    totalOrders += item.OrderCount;
                    totalSum += item.TotalSpent;
                    worksheet.Cells[row, 1] = count;
                    worksheet.Cells[row, 2] = item.CustomerName ?? "Неизвестный клиент";
                    worksheet.Cells[row, 3] = item.OrderCount;
                    worksheet.Cells[row, 4] = item.TotalSpent;
                    row++;
                }

                worksheet.Cells[row, 2] = "ИТОГО:";
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 2]).Font.Bold = true;
                worksheet.Cells[row, 3] = totalOrders;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 3]).Font.Bold = true;
                worksheet.Cells[row, 4] = totalSum;
                ((Microsoft.Office.Interop.Excel.Range)worksheet.Cells[row, 4]).Font.Bold = true;

                worksheet.Columns.AutoFit();

                string desktopPath = GetDesktopPath();
                string safeName = string.Join("_", reportName.Split(Path.GetInvalidFileNameChars()));
                string fileName = $"{safeName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                string filePath = Path.Combine(desktopPath, fileName);
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();

                return filePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания Excel: {ex.Message}");
            }
        }
    }
}