using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace AppleStore.Pages.ManagerPages
{
    public partial class ChangeStatusWindow : Window
    {
        private int _orderId;
        private string _currentStatus;

        public ChangeStatusWindow(int orderId, string currentStatus)
        {
            InitializeComponent();
            _orderId = orderId;
            _currentStatus = currentStatus;
            LoadData();
            btnSave.Click += BtnSave_Click;
            btnCancel.Click += BtnCancel_Click;
        }

        private void LoadData()
        {
            txtOrderId.Text = _orderId.ToString();
            foreach (ComboBoxItem item in cmbStatus.Items)
            {
                if (item.Content.ToString() == _currentStatus)
                {
                    cmbStatus.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStatus.SelectedItem is ComboBoxItem selectedItem)
            {
                string newStatus = selectedItem.Content.ToString();

                try
                {
                    string query = "UPDATE Orders SET Status = @Status WHERE OrdID = @OrderID";
                    SqlParameter[] parameters =
                    {
                        new SqlParameter("@Status", newStatus),
                        new SqlParameter("@OrderID", _orderId)
                    };
                    DatabaseHelper.ExecuteNonQuery(query, parameters);
                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}