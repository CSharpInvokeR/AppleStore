using System.Windows;
using AppleStore.Pages;
using AppleStore.Pages.CustomerPages;

namespace AppleStore
{
    public partial class Customer : Window
    {
        public static event System.Action<string> UserInfoUpdated;

        public Customer()
        {
            InitializeComponent();
            txtUserInfo.Text = $"{UserSession.FirstName} {UserSession.LastName}";
            CartPage.LoadCartFromDatabase();
            MainFrame.Navigate(new CatalogPage());

            UserInfoUpdated += OnUserInfoUpdated;
        }

        private void OnUserInfoUpdated(string fullName)
        {
            txtUserInfo.Text = fullName;
        }

        public static void UpdateUserInfoStatic(string fullName)
        {
            UserInfoUpdated?.Invoke(fullName);
        }

        private void btnCatalog_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CatalogPage());
        private void btnCart_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new CartPage());
        private void btnOrders_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new OrdersPage());
        private void btnProfile_Click(object sender, RoutedEventArgs e) => MainFrame.Navigate(new ProfilePage());

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}