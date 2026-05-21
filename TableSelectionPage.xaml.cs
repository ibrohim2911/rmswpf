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

        private async Task LoadTablesAsync(int? locationId = null)
        {
            try
            {
                string url = "/api/tables/tables/";
                if (locationId.HasValue) url += $"?location={locationId.Value}";

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
            int locId = (int)btn.Tag;
            await LoadTablesAsync(locId);
        }

        private async void ClearLocationFilter_Click(object sender, RoutedEventArgs e)
        {
            await LoadTablesAsync();
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

            SelectedTable = selectedTable;
            this.DialogResult = true;
            this.Close();
        }
    }
}