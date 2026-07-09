using System.Text.RegularExpressions;
using System.Windows;

namespace AppleStore.Pages.AdminPages
{
    public partial class EmployeeEditWindow : Window
    {
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string FirstName { get; private set; }
        public string Patronymic { get; private set; }
        public string LastName { get; private set; }
        public bool IsEditMode { get; private set; }

        public EmployeeEditWindow(string firstName = "", string patronymic = "", string lastName = "", bool isEdit = false)
        {
            InitializeComponent();
            IsEditMode = isEdit;

            txtFirstName.Text = firstName;
            txtPatronymic.Text = patronymic;
            txtLastName.Text = lastName;

            if (isEdit)
            {
                Title = "Редактирование";
                txtUsername.IsEnabled = false;
                txtPassword.IsEnabled = false;
                txtUsername.Text = "логин нельзя изменить";
                txtPassword.Password = "********";
            }

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => DialogResult = false;
        }

        private bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return true;
            return Regex.IsMatch(name, @"^[a-zA-Zа-яА-ЯёЁ]+$");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditMode)
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (txtUsername.Text.Length < 3)
                {
                    MessageBox.Show("Логин должен содержать минимум 3 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Password.Length < 4)
                {
                    MessageBox.Show("Пароль должен содержать минимум 4 символа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return;
                }

                Username = txtUsername.Text.Trim();
                Password = txtPassword.Password;
            }

            if (!IsValidName(txtFirstName.Text))
            {
                MessageBox.Show("Имя должно содержать только буквы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtFirstName.Focus();
                return;
            }

            if (!IsValidName(txtPatronymic.Text))
            {
                MessageBox.Show("Отчество должно содержать только буквы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPatronymic.Focus();
                return;
            }

            if (!IsValidName(txtLastName.Text))
            {
                MessageBox.Show("Фамилия должна содержать только буквы", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtLastName.Focus();
                return;
            }

            FirstName = txtFirstName.Text.Trim();
            Patronymic = txtPatronymic.Text.Trim();
            LastName = txtLastName.Text.Trim();
            DialogResult = true;
            Close();
        }
    }
}