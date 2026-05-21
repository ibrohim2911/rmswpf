using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.Json;
using System.Text.Json.Serialization;
using rms_gui.Services;
using rms_gui.Models;

namespace rms_gui
{
    public partial class MainPage : Page
    {
        // 1. JSON xatoliklarini oldini olish uchun C# ga "bo'shroq" ishlashni buyuramiz
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString // "1" matnini int ga aylantiradi
        };

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Ensure CSRF token is set before making requests
                await ApiClient.EnsureCsrfTokenAsync();

                // Load all initial data
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
                    System.Diagnostics.Debug.WriteLine($"Waiters API error: {response.StatusCode}");
                    return;
                }

                string rawJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Waiters JSON: {rawJson}");

                var waiters = JsonSerializer.Deserialize<List<User>>(rawJson, JsonOptions);

                if (WaitersListControl != null) 
                {
                    WaitersListControl.ItemsSource = waiters;
                    System.Diagnostics.Debug.WriteLine($"Loaded {waiters?.Count ?? 0} waiters");
                }
            }
            catch (JsonException ex)
            {
                try
                {
                    string rawJson = await ApiClient.Client.GetStringAsync("/api/users/users/");
                    MessageBox.Show($"Xodimlar JSON xatosi!\n{ex.Message}\n\nDjango yuborgan data:\n{rawJson}");
                }
                catch (Exception debugEx)
                {
                    MessageBox.Show($"Xodimlar yuklanib bolmadi!\n{debugEx.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Waiters error: {ex.Message}");
            }
        }

        private async Task LoadLocationsAsync()
        {
            try
            {
                var response = await ApiClient.Client.GetAsync("/api/tables/locations/");
                if (!response.IsSuccessStatusCode) 
                {
                    System.Diagnostics.Debug.WriteLine($"Locations API error: {response.StatusCode}");
                    return;
                }

                string rawJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Locations JSON: {rawJson}");

                var locations = JsonSerializer.Deserialize<List<Location>>(rawJson, JsonOptions);

                if (LocationsListControl != null) 
                {
                    LocationsListControl.ItemsSource = locations;
                    System.Diagnostics.Debug.WriteLine($"Loaded {locations?.Count ?? 0} locations");
                }
            }
            catch (JsonException ex)
            {
                try
                {
                    string rawJson = await ApiClient.Client.GetStringAsync("/api/tables/locations/");
                    MessageBox.Show($"Zonalar JSON xatosi!\n{ex.Message}\n\nDjango yuborgan data:\n{rawJson}");
                }
                catch (Exception debugEx)
                {
                    MessageBox.Show($"Zonalar yuklanib bolmadi!\n{debugEx.Message}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Locations error: {ex.Message}");
            }
        }

        private async Task LoadOrdersAsync(string waiterId = null, string locationId = null)
        {
            try
            {
                string url = "/api/orders/orders/?status=open";
                if (!string.IsNullOrEmpty(waiterId)) url += $"&waiter={waiterId}";
                if (!string.IsNullOrEmpty(locationId)) url += $"&location={locationId}";

                var response = await ApiClient.Client.GetAsync(url);
                if (!response.IsSuccessStatusCode) 
                {
                    MessageBox.Show($"Server error: {response.StatusCode}");
                    return;
                }

                string rawJson = await response.Content.ReadAsStringAsync();

                // Log raw JSON for debugging
                System.Diagnostics.Debug.WriteLine($"Raw JSON from backend: {rawJson}");

                var orders = JsonSerializer.Deserialize<List<Order>>(rawJson, JsonOptions);

                if (OrdersListControl != null) 
                {
                    OrdersListControl.ItemsSource = orders;
                    System.Diagnostics.Debug.WriteLine($"Loaded {orders?.Count ?? 0} orders successfully");
                }

                if (TotalOrdersText != null) 
                {
                    TotalOrdersText.Text = $"Jami: {orders?.Count ?? 0}";
                }
            }
            catch (JsonException ex)
            {
                string url = "/api/orders/orders/?status=open";
                try
                {
                    string rawJson = await ApiClient.Client.GetStringAsync(url);
                    MessageBox.Show($"Buyurtmalar JSON xatosi!\n{ex.Message}\n\nDjango yuborgan data:\n{rawJson}");
                }
                catch (Exception debugEx)
                {
                    MessageBox.Show($"Buyurtmalar yuklanib bolmadi!\n{debugEx.Message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Xato: {ex.Message}");
            }
        }

        // --- 2. FILTER CLICKS ---

        private async void WaiterFilter_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is string waiterId) await LoadOrdersAsync(waiterId: waiterId);
        }

        private async void LocationFilter_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn?.Tag is string locationId) await LoadOrdersAsync(locationId: locationId);
        }

        private async void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrdersAsync();
        }

        // --- 3. UI TAB SWITCHING (Waiters vs Locations) ---

        private void WaitersTab_Click(object sender, RoutedEventArgs e)
        {
            if (WaitersListControl != null) WaitersListControl.Visibility = Visibility.Visible;
            if (LocationsListControl != null) LocationsListControl.Visibility = Visibility.Collapsed;

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
            if (WaitersListControl != null) WaitersListControl.Visibility = Visibility.Collapsed;
            if (LocationsListControl != null) LocationsListControl.Visibility = Visibility.Visible;

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

        private async void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Refresh all data when Orders button is clicked
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error refreshing orders: " + ex.Message);
            }
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new TableSelectionPage());
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
                this.NavigationService.GoBack();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new ProfilePage());
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tizimdan chiqmoqchimisiz? (Are you sure you want to logout?)", "Logout", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // Qotib qolmasligi uchun oldin Navigate qilamiz
                this.NavigationService.Navigate(new LoginPage());

                // Orqa tarixni tozalashni sahifa yuklanib bo'lgandan KEYIN orqa fonda bajaramiz! (FREEZE ga yechim)
                Dispatcher.BeginInvoke(new Action(() => {
                    while (this.NavigationService.CanGoBack)
                    {
                        this.NavigationService.RemoveBackEntry();
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}