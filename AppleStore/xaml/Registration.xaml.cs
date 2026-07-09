using System;
using System.Windows;
using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace AppleStore
{
    public partial class Registration : Window
    {
        public Registration()
        {
            InitializeComponent();
            txtUsername.Focus();
            chkShowPassword.Checked += ChkShowPassword_Checked;
            chkShowPassword.Unchecked += ChkShowPassword_Unchecked;
            btnBack.Click += BtnBack_Click;
            btnRegister.Click += BtnRegister_Click;
        }

        private void ChkShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtPasswordVisible.Text = txtPassword.Password;
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;
            chkShowPassword.Content = "🔒";
        }

        private void ChkShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            txtPassword.Password = txtPasswordVisible.Text;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            txtPassword.Visibility = Visibility.Visible;
            chkShowPassword.Content = "👁";
            txtPassword.Focus();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string phoneNumber = txtPhoneNumber.Text.Trim();
            string email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Введите логин\n\nПример: Cat456", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsername.Focus();
                return;
            }

            if (username.Length < 3)
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа\n\nПример: Cat456", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль\n\nПример: myCatName321", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (chkShowPassword.IsChecked == true) txtPasswordVisible.Focus();
                else txtPassword.Focus();
                return;
            }

            if (password.Length < 4)
            {
                MessageBox.Show("Пароль должен содержать минимум 4 символа\n\nПример: myCatName321", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (chkShowPassword.IsChecked == true) txtPasswordVisible.Focus();
                else txtPassword.Focus();
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtConfirmPassword.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Введите email\n\nПример: cat@mail.ru", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            if (!IsValidEmail(email))
            {
                MessageBox.Show("Введите корректный email адрес\n\nПример: cat@mail.ru", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                if (!IsValidPhoneNumber(phoneNumber))
                {
                    MessageBox.Show("Введите корректный номер телефона\n\nПримеры:\n+7 123 456-78-90\n8 123 456-78-90\n91234567890\n+79123456789",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPhoneNumber.Focus();
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                if (!IsValidName(firstName))
                {
                    MessageBox.Show("Имя должно содержать только буквы\n\nПример: Иван", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtFirstName.Focus();
                    return;
                }
            }

            if (!string.IsNullOrWhiteSpace(lastName))
            {
                if (!IsValidName(lastName))
                {
                    MessageBox.Show("Фамилия должна содержать только буквы\n\nПример: Иванов", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLastName.Focus();
                    return;
                }
            }

            try
            {
                btnRegister.IsEnabled = false;
                btnRegister.Content = "РЕГИСТРАЦИЯ...";

                if (IsUsernameTaken(username))
                {
                    MessageBox.Show("Этот логин уже занят. Выберите другой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    btnRegister.IsEnabled = true;
                    btnRegister.Content = "Зарегистрироваться";
                    return;
                }

                if (IsEmailTaken(email))
                {
                    MessageBox.Show("Этот email уже используется. Выберите другой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    btnRegister.IsEnabled = true;
                    btnRegister.Content = "Зарегистрироваться";
                    return;
                }

                if (CreateUser(username, password, firstName, lastName, phoneNumber, email))
                {
                    MessageBoxResult result = MessageBox.Show("Регистрация прошла успешно!\n\nВойти в систему сейчас?",
                                                              "Регистрация завершена",
                                                              MessageBoxButton.YesNo,
                                                              MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool isValidUser = DatabaseHelper.ValidateUser(username, password);

                        if (isValidUser)
                        {
                            this.Hide();

                            switch (UserSession.Role)
                            {
                                case "Администратор":
                                    Admin adminWindow = new Admin();
                                    adminWindow.Show();
                                    break;
                                case "Менеджер":
                                    Manager managerWindow = new Manager();
                                    managerWindow.Show();
                                    break;
                                default:
                                    Customer customerWindow = new Customer();
                                    customerWindow.Show();
                                    break;
                            }

                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка автоматического входа. Пожалуйста, войдите вручную.",
                                          "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            MainWindow loginWindow = new MainWindow();
                            loginWindow.Show();
                            Close();
                        }
                    }
                    else
                    {
                        MainWindow loginWindow = new MainWindow();
                        loginWindow.Show();
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show("Ошибка при создании пользователя. Попробуйте еще раз.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnRegister.IsEnabled = true;
                    btnRegister.Content = "Зарегистрироваться";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка регистрации: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                btnRegister.IsEnabled = true;
                btnRegister.Content = "Зарегистрироваться";
            }
        }

        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return true;

            string pattern = @"^[a-zA-Zа-яА-ЯёЁ]+$";
            return Regex.IsMatch(name, pattern);
        }

        private bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return true;

            string cleaned = "";
            foreach (char c in phone)
            {
                if (char.IsDigit(c))
                    cleaned += c;
            }

            if (cleaned.Length == 11)
            {
                if (cleaned[0] == '7' || cleaned[0] == '8')
                {
                    return true;
                }
                return false;
            }
            else if (cleaned.Length == 10)
            {
                return true;
            }

            return false;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, pattern);
            }
            catch
            {
                return false;
            }
        }

        private bool IsUsernameTaken(string username)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            SqlParameter[] parameters = { new SqlParameter("@Username", username) };
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }

        private bool IsEmailTaken(string email)
        {
            string query = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            SqlParameter[] parameters = { new SqlParameter("@Email", email) };
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            return count > 0;
        }

        private bool CreateUser(string username, string password, string firstName, string lastName, string phoneNumber, string email)
        {
            string query = @"INSERT INTO Users (Username, Password, Email, RoleID, FirstName, LastName, PhoneNumber, CreatedDate) 
                            VALUES (@Username, @Password, @Email, 1, @FirstName, @LastName, @PhoneNumber, GETDATE())";

            SqlParameter[] parameters =
            {
                new SqlParameter("@Username", username),
                new SqlParameter("@Password", password),
                new SqlParameter("@Email", email),
                new SqlParameter("@FirstName", string.IsNullOrWhiteSpace(firstName) ? (object)DBNull.Value : firstName),
                new SqlParameter("@LastName", string.IsNullOrWhiteSpace(lastName) ? (object)DBNull.Value : lastName),
                new SqlParameter("@PhoneNumber", string.IsNullOrWhiteSpace(phoneNumber) ? (object)DBNull.Value : phoneNumber)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(query, parameters);
            return rowsAffected > 0;
        }
    }
}