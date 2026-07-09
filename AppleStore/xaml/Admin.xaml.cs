using System.Windows;
using System.Windows.Controls;
using AppleStore.Pages.AdminPages;

namespace AppleStore
{
    public partial class Admin : Window
    {
        private Button _activeButton;

        public Admin()
        {
            InitializeComponent();
            txtUserInfo.Text = $"{UserSession.FirstName} {UserSession.LastName}";

            SetActiveButton(btnProducts);
            MainFrame.Navigate(new AdminProductsPage());
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
            MainFrame.Navigate(new AdminProductsPage());
        }

        private void btnEmployees_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnEmployees);
            MainFrame.Navigate(new AdminEmployeesPage());
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnOrders);
            MainFrame.Navigate(new AdminOrdersPage());
        }

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnReports);
            MainFrame.Navigate(new AdminReportsPage());
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow loginWindow = new MainWindow();
            loginWindow.Show();
            Close();
        }
    }
}