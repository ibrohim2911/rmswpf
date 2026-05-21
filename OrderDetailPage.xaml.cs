using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using rms_gui.Services;
using rms_gui.Models;
// Removed 'using System.Windows.Controls.MenuItem' conflicts

namespace rms_gui
{
    public partial class OrderDetailPage : Page
    {
        private Table _currentTable;
        private List<CartItem> _cart = new List<CartItem>();

        public OrderDetailPage(Table table)
        {
            InitializeComponent();
            _currentTable = table;

            // Link to the XAML element
            TableHeaderText.Text = $"Stol: {table.name}";

            Loaded += OrderDetailWindow_Loaded;
        }

        private async void OrderDetailWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
            await LoadMenuItemsAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await ApiClient.Client.GetFromJsonAsync<List<MenuCategory>>("/api/inventory/menu-categories/");
            CategoriesListControl.ItemsSource = categories;
        }

        private async Task LoadMenuItemsAsync(string categoryId = null)
        {
            string url = "/api/inventory/menu-items/";
            if (!string.IsNullOrEmpty(categoryId)) url += $"?menu_category={categoryId}";

            // Explicitly use your model here
            var items = await ApiClient.Client.GetFromJsonAsync<List<rms_gui.Models.MenuItem>>(url);
            MenuItemsListControl.ItemsSource = items;
        }

        private async void Category_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string catId = btn.Tag as string;
            await LoadMenuItemsAsync(catId);
        }

        // --- CART LOGIC ---
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).Tag as rms_gui.Models.MenuItem;
            var existingCartItem = _cart.FirstOrDefault(c => c.Product.id == item.id);

            if (existingCartItem != null)
            {
                existingCartItem.Quantity++;
            }
            else
            {
                _cart.Add(new CartItem { Product = item, Quantity = 1 });
            }
            RefreshCartUI();
        }

        private void IncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).Tag as CartItem;
            cartItem.Quantity++;
            RefreshCartUI();
        }

        private void DecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).Tag as CartItem;
            if (cartItem.Quantity > 1) cartItem.Quantity--;
            else _cart.Remove(cartItem);
            RefreshCartUI();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).Tag as CartItem;
            _cart.Remove(cartItem);
            RefreshCartUI();
        }

        private void RefreshCartUI()
        {
            CartListControl.ItemsSource = null;
            CartListControl.ItemsSource = _cart;

            decimal total = _cart.Sum(c => c.TotalPrice);
            TotalPriceText.Text = $"{total:N0} so'm";
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

        private async void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!_cart.Any())
            {
                MessageBox.Show("Savatcha bo'sh! (Cart is empty)");
                return;
            }

            try
            {
                var orderPayload = new
                {
                    table = _currentTable.id,
                    customer_quantity = 1
                };

                var orderResponse = await ApiClient.Client.PostAsJsonAsync("/api/orders/orders/", orderPayload);

                if (!orderResponse.IsSuccessStatusCode)
                {
                    MessageBox.Show("Buyurtma yaratishda xatolik! (Error creating order)");
                    return;
                }

                var createdOrder = await orderResponse.Content.ReadFromJsonAsync<Order>();

                foreach (var cartItem in _cart)
                {
                    var itemPayload = new
                    {
                        menu_item = cartItem.Product.id,
                        quantity = cartItem.Quantity,
                        price = cartItem.Product.price
                    };

                    await ApiClient.Client.PostAsJsonAsync($"/api/orders/orders/{createdOrder.id}/add_item/", itemPayload);
                }

                MessageBox.Show("Buyurtma muvaffaqiyatli saqlandi! (Order saved!)");

                _cart.Clear();
                this.NavigationService.Navigate(new MainPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xatolik: " + ex.Message);
            }
        }
    }
}