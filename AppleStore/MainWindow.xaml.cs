using System;
using System.Windows;
using System.Windows.Input;

namespace AppleStore
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => txtUsername.Focus();
            txtPassword.KeyDown += TxtPassword_KeyDown;
            txtPasswordVisible.KeyDown += TxtPasswordVisible_KeyDown;
            btnLogin.Click += BtnLogin_Click;
            chkShowPassword.Checked += ChkShowPassword_Checked;
            chkShowPassword.Unchecked += ChkShowPassword_Unchecked;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e) => Login();
        private void BtnReg_Click(object sender, RoutedEventArgs e)
        {
            Registration registration = new Registration();
            registration.Show();
            Close();
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Login();
        }

        private void TxtPasswordVisible_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Login();
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

        private void Login()
        {
            string username = txtUsername.Text.Trim();
            string password = chkShowPassword.IsChecked == true ? txtPasswordVisible.Text : txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                btnLogin.Content = "ПРОВЕРКА...";
                btnLogin.IsEnabled = false;
                bool isValidUser = DatabaseHelper.ValidateUser(username, password);

                if (isValidUser)
                {

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
                        case "Клиент":
                            Customer customerWindow = new Customer();
                            customerWindow.Show();
                            break;
                        default:
                            MessageBox.Show("Неизвестная роль пользователя", "Ошибка");
                            Show();
                            return;
                    }
                    Close();
                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
                    txtPassword.Password = "";
                    txtPasswordVisible.Text = "";
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                btnLogin.Content = "Войти";
                btnLogin.IsEnabled = true;
            }
        }
    }
}