using System;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace AppleStore
{
    public static class EmailHelper
    {
        public static bool SendOrderReceipt(string email, int orderId, decimal totalAmount, string orderDate, string address, dynamic items)
        {
            try
            {
                Console.WriteLine($"=== ОТПРАВКА ПИСЬМА ===");
                Console.WriteLine($"Кому: {email}");
                Console.WriteLine($"Заказ №: {orderId}");
                Console.WriteLine($"Сумма: {totalAmount}");

                string fromEmail = ConfigHelper.GetEmailSetting("Email");
                string fromPassword = ConfigHelper.GetEmailSetting("Password");
                string fromName = ConfigHelper.GetEmailSetting("FromName");

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromEmail, fromName);
                    mail.To.Add(email);
                    mail.Subject = $"Чек по заказу №{orderId}";
                    mail.Body = BuildReceiptBody(orderId, totalAmount, orderDate, address, items);
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient())
                    {
                        string domain = fromEmail.Split('@')[1].ToLower();
                        smtp.Host = ConfigHelper.GetEmailSetting("SmtpServer");
                        smtp.Port = int.Parse(ConfigHelper.GetEmailSetting("Port"));
                        smtp.EnableSsl = bool.Parse(ConfigHelper.GetEmailSetting("EnableSsl"));
                        smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                        smtp.Timeout = 10000;

                        Console.WriteLine($"SMTP сервер: {smtp.Host}");
                        smtp.Send(mail);
                        Console.WriteLine("Письмо отправлено успешно!");
                    }
                }

                return true;
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Ошибка: {smtpEx.StatusCode} - {smtpEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки email: {ex.Message}");
                Console.WriteLine($"Стек: {ex.StackTrace}");
                return false;
            }
        }

        private static string BuildReceiptBody(int orderId, decimal totalAmount, string orderDate, string address, dynamic items)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><meta charset='UTF-8'><style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; }");
            sb.AppendLine(".header { text-align: center; border-bottom: 2px solid #000; padding-bottom: 15px; }");
            sb.AppendLine(".header h1 { font-size: 24px; margin: 0; }");
            sb.AppendLine(".info { background: #f5f5f5; padding: 15px; border-radius: 8px; margin: 15px 0; }");
            sb.AppendLine(".info td { padding: 5px 10px; }");
            sb.AppendLine(".items table { width: 100%; border-collapse: collapse; }");
            sb.AppendLine(".items th { background: #000; color: #fff; padding: 10px; text-align: left; }");
            sb.AppendLine(".items td { padding: 8px 10px; border-bottom: 1px solid #ddd; }");
            sb.AppendLine(".total { text-align: right; font-size: 18px; font-weight: bold; margin-top: 15px; }");
            sb.AppendLine(".footer { text-align: center; border-top: 1px solid #ddd; padding-top: 15px; margin-top: 20px; color: #666; font-size: 12px; }");
            sb.AppendLine(".warranty { color: #FF6B00; font-weight: bold; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='header'><h1>TechStore</h1><p>Интернет-магазин техники Apple</p></div>");

            sb.AppendLine("<div class='info'><table>");
            sb.AppendLine($"<tr><td><strong>Заказ №</strong></td><td>{orderId}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Дата</strong></td><td>{orderDate}</td></tr>");
            sb.AppendLine($"<tr><td><strong>Адрес доставки</strong></td><td>{address}</td></tr>");
            sb.AppendLine("</table></div>");

            sb.AppendLine("<div class='items'><h3>Состав заказа</h3><table>");
            sb.AppendLine("<tr><th>Товар</th><th style='text-align:center'>Кол-во</th><th style='text-align:right'>Цена</th><th style='text-align:right'>Сумма</th><th style='text-align:center'>Гарантия</th></tr>");

            foreach (var item in items)
            {
                decimal itemTotal = item.Quantity * item.Price;
                string warrantyText = "-";
                try
                {
                    int warranty = Convert.ToInt32(item.Warranty);
                    if (warranty > 0)
                        warrantyText = warranty + " мес.";
                }
                catch
                {

                }

                sb.AppendLine($"<tr><td>{item.ProductName}</td><td style='text-align:center'>{item.Quantity}</td><td style='text-align:right'>{item.Price:N2} ₽</td><td style='text-align:right'>{itemTotal:N2} ₽</td><td style='text-align:center' class='warranty'>{warrantyText}</td></tr>");
            }

            sb.AppendLine("</table></div>");
            sb.AppendLine($"<div class='total'>Итого: {totalAmount:N2} ₽</div>");
            sb.AppendLine("<div class='footer'>Спасибо за покупку!</div>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
    }
}