using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using rms_gui.Services;
using rms_gui.Models;

namespace rms_gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // When window opens, load all data initially
            try
            {
                await LoadWaitersAsync();
                await LoadLocationsAsync();
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Data loading error: " + ex.Message);
            }
        }

        // --- 1. DATA FETCHING ---

        private async Task LoadWaitersAsync()
        {
            try
            {
                var response = await ApiClient.Client.GetAsync("/api/users/users/");

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Xodimlarni yuklashda xatolik ({response.StatusCode}): {errorContent}");
                    return;
                }

                var waiters = await response.Content.ReadFromJsonAsync<List<User>>();
                if (WaitersListControl != null)
                {
                    WaitersListControl.ItemsSource = waiters;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xodimlarni yuklashda xatolik: " + ex.Message);
            }
        }

        private async Task LoadLocationsAsync()
        {
            try
            {
                var response = await ApiClient.Client.GetAsync("/api/tables/locations/");

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Zonalarni yuklashda xatolik ({response.StatusCode}): {errorContent}");
                    return;
                }

                var locations = await response.Content.ReadFromJsonAsync<List<Location>>();
                if (LocationsListControl != null)
                {
                    LocationsListControl.ItemsSource = locations;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Zonalarni yuklashda xatolik: " + ex.Message);
            }
        }

        // Central method to load orders. Applies filters if IDs are provided.
        private async Task LoadOrdersAsync(int? waiterId = null, int? locationId = null)
        {
            try
            {
                // Build the URL with query parameters. By default, only get 'open' status orders.
                string url = "/api/orders/orders/?status=open";

                if (waiterId.HasValue)
                    url += $"&waiter={waiterId.Value}";

                if (locationId.HasValue)
                    url += $"&location={locationId.Value}";

                var response = await ApiClient.Client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Buyurtmalarni yuklashda xatolik ({response.StatusCode}): {errorContent}");
                    return;
                }

                // Fetch from Django
                var orders = await response.Content.ReadFromJsonAsync<List<Order>>();

                // Bind to the UI
                if (OrdersListControl != null)
                {
                    OrdersListControl.ItemsSource = orders;
                }

                if (TotalOrdersText != null)
                {
                    TotalOrdersText.Text = $"Jami: {orders?.Count ?? 0}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Buyurtmalarni yuklashda xatolik: " + ex.Message);
            }
        }

        // --- 2. FILTER CLICKS ---

        private async void WaiterFilter_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is int waiterId)
            {
                await LoadOrdersAsync(waiterId: waiterId);
            }
        }

        private async void LocationFilter_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is int locationId)
            {
                await LoadOrdersAsync(locationId: locationId);
            }
        }

        private async void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync(); // Loads all active orders without filters
        }

        // --- 3. UI TAB SWITCHING (Waiters vs Locations) ---

        private void WaitersTab_Click(object sender, RoutedEventArgs e)
        {
            if (WaitersListControl != null)
                WaitersListControl.Visibility = Visibility.Visible;
            if (LocationsListControl != null)
                LocationsListControl.Visibility = Visibility.Collapsed;

            if (WaitersTabBtn != null)
            {
                WaitersTabBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                WaitersTabBtn.Foreground = Brushes.White;
            }

            if (LocationsTabBtn != null)
            {
                LocationsTabBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A30"));
                LocationsTabBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
            }
        }

        private void LocationsTab_Click(object sender, RoutedEventArgs e)
        {
            if (WaitersListControl != null)
                WaitersListControl.Visibility = Visibility.Collapsed;
            if (LocationsListControl != null)
                LocationsListControl.Visibility = Visibility.Visible;

            if (LocationsTabBtn != null)
            {
                LocationsTabBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                LocationsTabBtn.Foreground = Brushes.White;
            }

            if (WaitersTabBtn != null)
            {
                WaitersTabBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A30"));
                WaitersTabBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF"));
            }
        }

        // --- 4. BOTTOM BAR NAVIGATION ---

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            // Show main orders view
            UpdateBottomBarButtons(0);
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Open table selection window
            TableSelectionWindow tableSelectionWindow = new TableSelectionWindow()
            {
                Owner = this
            };

            if (tableSelectionWindow.ShowDialog() == true)
            {
                var selectedTable = tableSelectionWindow.SelectedTable;
                if (selectedTable != null)
                {
                    MessageBox.Show($"Order created for Table {selectedTable.id}", "Order Created");
                    // TODO: Navigate to order detail window
                }
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Show main content
            UpdateBottomBarButtons(0);
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Open profile window
            ProfileWindow profileWindow = new ProfileWindow()
            {
                Owner = this
            };
            profileWindow.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Logout and return to login window
            var result = MessageBox.Show("Tizimdan chiqmoqchimisiz? (Are you sure you want to logout?)", "Logout", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle tab selection if needed
        }

        private void UpdateBottomBarButtons(int activeIndex)
        {
            // This would update the styling of bottom bar buttons to show which is active
        }
    }
}
