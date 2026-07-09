using System.Windows;
using System.Windows.Controls;
using AppleStore.Pages.ManagerPages;

namespace AppleStore
{
    public partial class Manager : Window
    {
        private Button _activeButton;

        public Manager()
        {
            InitializeComponent();
            txtUserInfo.Text = $"{UserSession.FirstName} {UserSession.LastName}";

            SetActiveButton(btnProducts);
            MainFrame.Navigate(new ManagerProductsPage());
        }

        private void SetActiveButton(Button button)
        {
            if (_activeButton != null)
            {
                _activeButton.BorderBrush = System.Windows.Media.Brushes.Transparent;
                _activeButton.BorderThickness = new Thickness(0);
            }

            _activeButton = button;
            _activeButton.BorderBrush = System.Windows.Media.Brushes.White;
            _activeButton.BorderThickness = new Thickness(2);
        }

        private void btnProducts_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnProducts);
            MainFrame.Navigate(new ManagerProductsPage());
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnOrders);
            MainFrame.Navigate(new ManagerOrdersPage());
        }

        private void btnCustomers_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnCustomers);
            MainFrame.Navigate(new ManagerCustomersPage());
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnReports);
            MainFrame.Navigate(new ManagerReportsPage());
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            Close();
        }
    }
}