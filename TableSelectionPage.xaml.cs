using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using rms_gui.Services;
using rms_gui.Models;
using System.Text.Json;
using System.Net;
using System.Text.Json.Serialization;

namespace rms_gui
{
    public partial class TableSelectionPage : Page
    {
        public Table SelectedTable { get; set; }

        public TableSelectionPage()
        {
            InitializeComponent();
            Loaded += TableSelectionWindow_Loaded;
        }

        private async void TableSelectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLocationsAsync();
            await LoadTablesAsync();
        }

        private async Task LoadLocationsAsync()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                };
                var response = await ApiClient.Client.GetAsync("/api/tables/locations/");
                response.EnsureSuccessStatusCode();
                var locations = await response.Content.ReadFromJsonAsync<List<Location>>(options);
                LocationsListControl.ItemsSource = locations;

                LocationsListControl.ItemsSource = locations;
            }
            catch (Exception ex) { MessageBox.Show("Error loading locations: " + ex.Message); }
        }

        private async Task LoadTablesAsync(string locationId = null)
        {
            try
            {
                string url = "/api/tables/tables/";
                if (!string.IsNullOrEmpty(locationId)) url += $"?location={locationId}";

                var response = await ApiClient.Client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var tables = await response.Content.ReadFromJsonAsync<List<Table>>(new JsonSerializerOptions
                {
                    NumberHandling = JsonNumberHandling.AllowReadingFromString
                });
                TablesListControl.ItemsSource = tables;
            }
            catch (Exception ex) { MessageBox.Show("Error loading tables: " + ex.Message); }
        }

        private async void LocationFilter_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string locId = btn.Tag as string;
            await LoadTablesAsync(locId);
        }

        private async void ClearLocationFilter_Click(object sender, RoutedEventArgs e)
        {
            await LoadTablesAsync();
        }
        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            // Show main orders view
            this.NavigationService.Navigate(new MainPage());
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            // Open table selection window
            UpdateBottomBarButtons(0);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.NavigationService.CanGoBack) this.NavigationService.GoBack();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            // Open profile window
            this.NavigationService.Navigate(new ProfilePage());
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

        private void Table_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var selectedTable = btn.Tag as Table;

            if (selectedTable.availability == false)
            {
                MessageBox.Show("Bu stol band! (This table is occupied)", "Warning");
                return;
            }

            this.NavigationService.Navigate(new OrderDetailPage(selectedTable));
        }
        private void UpdateBottomBarButtons(int activeIndex)
        {
            // This would update the styling of bottom bar buttons to show which is active
        }
    }
}