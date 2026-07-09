using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using AppleStore.Models;
using AppleStore.Helpers;

namespace AppleStore.Pages.AdminPages
{
    public partial class AdminEmployeesPage : Page
    {
        private string currentFilter = "Все";

        public AdminEmployeesPage()
        {
            InitializeComponent();
            LoadEmployees();

            btnRefreshEmployees.Click += (s, e) => LoadEmployees();
            btnAddEmployee.Click += AddEmployee_Click;
            btnEditEmployee.Click += EditEmployee_Click;
            btnDeleteEmployee.Click += DeleteEmployee_Click;
        }

        private void RbFilter_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio != null && radio.IsChecked == true)
            {
                currentFilter = radio.Content.ToString();
                LoadEmployees();
            }
        }

        private void LoadEmployees()
        {
            try
            {
                string query = "";

                if (currentFilter == "Все")
                {
                    query = @"SELECT u.UserID, u.Username, u.FirstName, u.Patronymic, u.LastName, r.RoleName 
                             FROM Users u INNER JOIN Role r ON u.RoleID = r.RoleID ORDER BY r.RoleName";
                }
                else if (currentFilter == "Сотрудники")
                {
                    query = @"SELECT u.UserID, u.Username, u.FirstName, u.Patronymic, u.LastName, r.RoleName 
                             FROM Users u INNER JOIN Role r ON u.RoleID = r.RoleID 
                             WHERE r.RoleName IN ('Администратор', 'Менеджер')
                             ORDER BY r.RoleName";
                }
                else
                {
                    query = @"SELECT u.UserID, u.Username, u.FirstName, u.Patronymic, u.LastName, r.RoleName 
                             FROM Users u INNER JOIN Role r ON u.RoleID = r.RoleID 
                             WHERE r.RoleName = 'Клиент'
                             ORDER BY u.LastName";
                }

                var employees = DatabaseHelper.ExecuteReader(query, reader => new User
                {
                    UserID = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    FirstName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    Patronymic = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    LastName = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    RoleName = reader.GetString(5)
                });

                if (EmployeesGrid != null)
                {
                    if (employees != null && employees.Count > 0)
                    {
                        EmployeesGrid.ItemsSource = employees;
                    }
                    else
                    {
                        EmployeesGrid.ItemsSource = new List<User>();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                if (EmployeesGrid != null)
                {
                    EmployeesGrid.ItemsSource = new List<User>();
                }
            }
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EmployeeEditWindow();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string hashedPassword = PasswordHelper.HashPassword(dialog.Password);
                    string query = @"INSERT INTO Users (Username, Password, Email, RoleID, FirstName, Patronymic, LastName) 
                                   VALUES (@Username, @Password, @Email, 2, @FirstName, @Patronymic, @LastName)";
                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@Username", dialog.Username),
                        new SqlParameter("@Password", hashedPassword),
                        new SqlParameter("@Email", dialog.Username + "@techstore.ru"),
                        new SqlParameter("@FirstName", dialog.FirstName),
                        new SqlParameter("@Patronymic", dialog.Patronymic),
                        new SqlParameter("@LastName", dialog.LastName)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    MessageBox.Show("Сотрудник добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadEmployees();
                }
                catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesGrid != null && EmployeesGrid.SelectedItem is User selected)
            {
                var dialog = new EmployeeEditWindow(selected.FirstName, selected.Patronymic, selected.LastName, true);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                {
                    try
                    {
                        string query = @"UPDATE Users SET FirstName = @FirstName, Patronymic = @Patronymic, LastName = @LastName 
                                       WHERE UserID = @UserID";
                        SqlParameter[] parameters =
                        {
                            new SqlParameter("@FirstName", dialog.FirstName),
                            new SqlParameter("@Patronymic", dialog.Patronymic),
                            new SqlParameter("@LastName", dialog.LastName),
                            new SqlParameter("@UserID", selected.UserID)
                        };
                        DatabaseHelper.ExecuteNonQuery(query, parameters);
                        MessageBox.Show("Данные обновлены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadEmployees();
                    }
                    catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                }
            }
            else MessageBox.Show("Выберите сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesGrid != null && EmployeesGrid.SelectedItem is User selected)
            {
                if (selected.UserID == UserSession.UserID)
                {
                    MessageBox.Show("Нельзя удалить самого себя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Удалить '{selected.FirstName} {selected.LastName}'?\n\n",
                              "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        string checkOrdersQuery = "SELECT COUNT(*) FROM Orders WHERE UserID = @UserID";
                        SqlParameter[] checkParams = { new SqlParameter("@UserID", selected.UserID) };
                        int orderCount = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkOrdersQuery, checkParams));

                        if (orderCount > 0)
                        {
                            string deleteOrderDetailsQuery = "DELETE FROM OrderDetails WHERE OrdID IN (SELECT OrdID FROM Orders WHERE UserID = @UserID)";
                            SqlParameter[] deleteDetailsParams = { new SqlParameter("@UserID", selected.UserID) };
                            DatabaseHelper.ExecuteNonQuery(deleteOrderDetailsQuery, deleteDetailsParams);

                            string deleteOrdersQuery = "DELETE FROM Orders WHERE UserID = @UserID";
                            SqlParameter[] deleteOrdersParams = { new SqlParameter("@UserID", selected.UserID) };
                            DatabaseHelper.ExecuteNonQuery(deleteOrdersQuery, deleteOrdersParams);
                        }

                        string deleteCartQuery = "DELETE FROM CartItems WHERE UserID = @UserID";
                        SqlParameter[] deleteCartParams = { new SqlParameter("@UserID", selected.UserID) };
                        DatabaseHelper.ExecuteNonQuery(deleteCartQuery, deleteCartParams);

                        string deleteUserQuery = "DELETE FROM Users WHERE UserID = @UserID";
                        SqlParameter[] deleteUserParams = { new SqlParameter("@UserID", selected.UserID) };
                        DatabaseHelper.ExecuteNonQuery(deleteUserQuery, deleteUserParams);

                        MessageBox.Show($"Пользователь '{selected.FirstName} {selected.LastName}' удален!",
                                      "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadEmployees();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else MessageBox.Show("Выберите сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}