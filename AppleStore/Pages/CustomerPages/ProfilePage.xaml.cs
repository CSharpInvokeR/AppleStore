using System;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using AppleStore.Models;

namespace AppleStore.Pages.CustomerPages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadUserProfile();
        }

        private void LoadUserProfile()
        {
            try
            {
                string query = "SELECT FirstName, LastName, PhoneNumber, Email FROM Users WHERE UserID = @UserID";
                SqlParameter[] param = { new SqlParameter("@UserID", UserSession.UserID) };
                var user = DatabaseHelper.ExecuteReader(query, reader => new User
                {
                    FirstName = reader.IsDBNull(0) ? "" : reader.GetString(0),
                    LastName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    PhoneNumber = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Email = reader.IsDBNull(3) ? "" : reader.GetString(3)
                }, param);

                if (user.Count > 0)
                {
                    txtFirstName.Text = user[0].FirstName;
                    txtLastName.Text = user[0].LastName;
                    txtPhone.Text = user[0].PhoneNumber;
                    txtEmail.Text = user[0].Email;
                }
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}"); }
        }

        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            return Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯёЁ]+$");
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true;
            string cleaned = "";
            foreach (char c in phone) if (char.IsDigit(c)) cleaned += c;
            if (cleaned.Length == 11 && (cleaned[0] == '7' || cleaned[0] == '8')) return true;
            if (cleaned.Length == 10) return true;
            return false;
        }

        private bool IsValidEmail(string email)
        {
            try { return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"); }
            catch { return false; }
        }

        private void btnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (!string.IsNullOrWhiteSpace(firstName) && !IsValidName(firstName))
            {
                MessageBox.Show("Имя должно содержать только буквы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFirstName.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(lastName) && !IsValidName(lastName))
            {
                MessageBox.Show("Фамилия должна содержать только буквы", "Ошибka", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLastName.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(phone) && !IsValidPhoneNumber(phone))
            {
                MessageBox.Show("Введите корректный номер телефона", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPhone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Email не может быть пустым", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный email адрес", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            try
            {
                string query = @"UPDATE Users SET FirstName = @FirstName, LastName = @LastName, 
                               PhoneNumber = @PhoneNumber, Email = @Email WHERE UserID = @UserID";
                SqlParameter[] param =
                {
                    new SqlParameter("@FirstName", string.IsNullOrEmpty(firstName) ? DBNull.Value : (object)firstName),
                    new SqlParameter("@LastName", string.IsNullOrEmpty(lastName) ? DBNull.Value : (object)lastName),
                    new SqlParameter("@PhoneNumber", string.IsNullOrEmpty(phone) ? DBNull.Value : (object)phone),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@UserID", UserSession.UserID)
                };
                DatabaseHelper.ExecuteNonQuery(query, param);

                UserSession.FirstName = firstName;
                UserSession.LastName = lastName;

                Customer.UpdateUserInfoStatic($"{firstName} {lastName}");

                MessageBox.Show("Данные профиля успешно сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
    }
}