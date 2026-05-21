using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace rms_gui
{
    /// <summary>
    /// Interaction logic for ProfileWindow.xaml
    /// </summary>
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
        }
        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            // Show main orders view
            this.NavigationService.Navigate(new MainPage());
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Open table selection window
            this.NavigationService.Navigate(new TableSelectionPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.NavigationService.CanGoBack) this.NavigationService.GoBack();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Open profile window
            UpdateBottomBarButtons(0);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Logout and return to login window
            var result = MessageBox.Show("Tizimdan chiqmoqchimisiz? (Are you sure you want to logout?)", "Logout", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                this.NavigationService.Navigate(new LoginPage());
                while (this.NavigationService.RemoveBackEntry != null) { } // Clear navigation history
            }
        }
        private void UpdateBottomBarButtons(int ActiveIndex)
        { }

    }
}
