using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using rms_gui.Services;
using rms_gui.Models;

namespace rms_gui
{
    public partial class OrderDetailPage : Page
    {
        private Order _currentOrder; // Endi Table emas, Order saqlaymiz
        private List<CartItem> _cart = new List<CartItem>();
        private List<rms_gui.Models.MenuItem> _allMenuItems = new List<rms_gui.Models.MenuItem>();
        private string _selectedPaymentMethod = "cash"; // Default payment method

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        // KONSTRUKTOR ENDI ORDER QABUL QILADI
        public OrderDetailPage(Order order)
        {
            InitializeComponent();
            _currentOrder = order;

            // Headerga qisqartirilgan Order ID sini va stolni yozamiz
            string shortId = order.id.Length >= 6 ? order.id.Substring(0, 6) : order.id;
            TableHeaderText.Text = $"ID: {shortId.ToUpper()}";

            Loaded += OrderDetailWindow_Loaded;
        }

        private async void OrderDetailWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
            await LoadMenuItemsAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var response = await ApiClient.Client.GetAsync("/api/inventory/menu-categories/");
                if (response.IsSuccessStatusCode)
                {
                    string rawJson = await response.Content.ReadAsStringAsync();
                    var categories = JsonSerializer.Deserialize<List<MenuCategory>>(rawJson, JsonOptions);
                    CategoriesListControl.ItemsSource = categories;
                }
            }
            catch (Exception ex) { MessageBox.Show("Kategoriyalarni yuklashda xatolik: " + ex.Message); }
        }

        private async Task LoadMenuItemsAsync(string categoryId = null)
        {
            try
            {
                string url = "/api/inventory/menu-items/";
                if (!string.IsNullOrEmpty(categoryId)) url += $"?menu_category={categoryId}";

                var response = await ApiClient.Client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string rawJson = await response.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<rms_gui.Models.MenuItem>>(rawJson, JsonOptions);
                    MenuItemsListControl.ItemsSource = items;

                    // Dastlabki yuklanishda hamma taomlarni xotiraga olamiz (ismlarini topish uchun)
                    if (string.IsNullOrEmpty(categoryId) && _allMenuItems.Count == 0 && items != null)
                    {
                        _allMenuItems = items;
                        MapSavedItemsToCart(); // Taomlar kelgach, avval saqlanganlarni savatchaga chizamiz
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Taomlarni yuklashda xatolik: " + ex.Message); }
        }

        private void MapSavedItemsToCart()
        {
            if (_allMenuItems.Count == 0 || _currentOrder.items == null) return;

            _cart.Clear();
            foreach (var oItem in _currentOrder.items)
            {
                if (oItem.is_deleted) continue; // O'chirilganlarni ko'rsatmaymiz

                var product = _allMenuItems.FirstOrDefault(m => m.id == oItem.menu_item_id);
                if (product != null)
                {
                    _cart.Add(new CartItem
                    {
                        Product = product,
                        Quantity = oItem.quantity,
                        IsSaved = true,
                        CreatedAt = oItem.created_at
                    });
                }
            }
            RefreshCartUI();
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

            // Faqat YANGLI qo'shilgan (unsaved) qatorga qo'shish (Eskilarga tegmaymiz)
            var existingCartItem = _cart.FirstOrDefault(c => c.Product.id == item.id && !c.IsSaved);

            if (existingCartItem != null) existingCartItem.Quantity++;
            else _cart.Add(new CartItem { Product = item, Quantity = 1, IsSaved = false });

            RefreshCartUI();
        }

        private void IncreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).Tag as CartItem;
            if (cartItem.IsSaved) { MessageBox.Show("Saqlangan taomni o'zgartirib bo'lmaydi. Yangi qo'shing."); return; }

            cartItem.Quantity++;
            RefreshCartUI();
        }

        private void DecreaseQty_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).Tag as CartItem;
            if (cartItem.IsSaved) { MessageBox.Show("Saqlangan taomni o'zgartirib bo'lmaydi."); return; }

            if (cartItem.Quantity > 1) cartItem.Quantity--;
            else _cart.Remove(cartItem);
            RefreshCartUI();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            var cartItem = (sender as Button).Tag as CartItem;
            if (cartItem.IsSaved) { MessageBox.Show("Saqlangan taomni o'chirish uchun administrator huquqi kerak."); return; }

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

        private async void SaveOrder_Click(object sender, RoutedEventArgs e)
        {
            // FAQATGINA hali saqlanmagan taomlarni ajratib olamiz
            var unsavedItems = _cart.Where(c => !c.IsSaved).ToList();

            if (!unsavedItems.Any())
            {
                MessageBox.Show("Saqlash uchun yangi taom yo'q! (No new items to save)");
                this.NavigationService.Navigate(new MainPage());
                return;
            }

            try
            {
                foreach (var cartItem in unsavedItems)
                {
                    var itemPayload = new
                    {
                        menu_item = cartItem.Product.id,
                        quantity = cartItem.Quantity,
                        price = cartItem.Product.price
                    };

                    // Buyurtma (Order) allaqachon mavjud, faqat yangi taomlarni POST qilamiz
                    await ApiClient.Client.PostAsJsonAsync($"/api/orders/orders/{_currentOrder.id}/add_item/", itemPayload);
                }

                MessageBox.Show("Yangi taomlar saqlandi! (Items saved!)");
                _cart.Clear();
                this.NavigationService.Navigate(new MainPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xatolik: " + ex.Message);
            }
        }

        // --- NEW ORDER ACTIONS ---

        /// <summary>
        /// Clear unsaved items from the cart
        /// </summary>
        private void ClearUnsavedItems_Click(object sender, RoutedEventArgs e)
        {
            var unsavedItems = _cart.Where(c => !c.IsSaved).ToList();

            if (!unsavedItems.Any())
            {
                MessageBox.Show("O'chirilishga tayyor yangi taom yo'q!");
                return;
            }

            var result = MessageBox.Show($"{unsavedItems.Count} ta yangi taomni o'chirmoqchimisiz?", 
                "Tasdiqlash", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var item in unsavedItems)
                {
                    _cart.Remove(item);
                }
                RefreshCartUI();
                MessageBox.Show("Yangi taomlar o'chirildi!");
            }
        }

        /// <summary>
        /// Preview the order before saving
        /// </summary>
        private void PreviewCheck_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var unsavedItems = _cart.Where(c => !c.IsSaved).ToList();
                decimal totalPrice = unsavedItems.Sum(c => c.TotalPrice);
                decimal tax = totalPrice * 0.10m; // 10% tax
                decimal finalTotal = totalPrice + tax;

                string preview = "=== DASTLABKI CHEK ===\n\n";
                preview += $"Buyurtma ID: {_currentOrder.id.Substring(0, 6).ToUpper()}\n";
                preview += $"Vaqt: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                preview += "------------------------\n\n";

                foreach (var item in unsavedItems)
                {
                    preview += $"{item.Product.name}\n";
                    preview += $"  Miqdori: {item.Quantity} x {item.Product.price:N0} = {item.TotalPrice:N0} so'm\n";
                }

                preview += "\n------------------------\n";
                preview += $"Jami: {totalPrice:N0} so'm\n";
                preview += $"QQS (10%): {tax:N0} so'm\n";
                preview += $"Umumiy: {finalTotal:N0} so'm\n";
                preview += "========================\n";

                MessageBox.Show(preview, "Dastlabki Chek", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xatolik: " + ex.Message);
            }
        }

        /// <summary>
        /// Close order item and change its status to pre-close
        /// </summary>
        private async void CloseOrderItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Buyurtmani yopmoqchimisiz? Status 'Dastlabki yopilgan' holatiga o'zgaradi.", 
                    "Tasdiqlash", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.No) return;

                var response = await ApiClient.Client.PostAsync(
                    $"/api/orders/order/{_currentOrder.id}/preclose", 
                    new StringContent("")
                );

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Buyurtma dastlabki yopilgan holatiga o'tkazildi!");
                    // Refresh order data
                    await LoadOrderDetails();
                }
                else
                {
                    MessageBox.Show("Xatolik: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xatolik: " + ex.Message);
            }
        }

        /// <summary>
        /// Open payment popup
        /// </summary>
        private void AddPayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentAmountDisplay.Text = "0";
            _selectedPaymentMethod = "cash";
            CashPaymentMethod.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129)); // Green
            CardPaymentMethod.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)); // Gray

            decimal orderTotal = _cart.Sum(c => c.TotalPrice);
            PaymentOrderTotal.Text = $"{orderTotal:N0} so'm";

            PaymentOverlay.Visibility = Visibility.Visible;
            PaymentPopupBorder.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Close payment popup
        /// </summary>
        private void ClosePayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentOverlay.Visibility = Visibility.Collapsed;
            PaymentPopupBorder.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Handle numpad button clicks
        /// </summary>
        private void NumpadButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string content = button.Content.ToString();

            if (PaymentAmountDisplay.Text == "0" && content != ".")
            {
                PaymentAmountDisplay.Text = content;
            }
            else if (content == "." && !PaymentAmountDisplay.Text.Contains("."))
            {
                PaymentAmountDisplay.Text += content;
            }
            else if (content != ".")
            {
                PaymentAmountDisplay.Text += content;
            }
        }

        /// <summary>
        /// Handle backspace on numpad
        /// </summary>
        private void NumpadBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (PaymentAmountDisplay.Text.Length > 1)
            {
                PaymentAmountDisplay.Text = PaymentAmountDisplay.Text.Substring(0, PaymentAmountDisplay.Text.Length - 1);
            }
            else
            {
                PaymentAmountDisplay.Text = "0";
            }
        }

        /// <summary>
        /// Select payment method (card or cash)
        /// </summary>
        private void PaymentMethod_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _selectedPaymentMethod = button.Tag.ToString();

            // Reset colors
            CashPaymentMethod.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)); // Gray
            CardPaymentMethod.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(107, 114, 128)); // Gray

            // Highlight selected
            if (_selectedPaymentMethod == "cash")
            {
                CashPaymentMethod.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(16, 185, 129)); // Green
            }
            else
            {
                CardPaymentMethod.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37, 99, 235)); // Blue
            }
        }

        /// <summary>
        /// Submit payment to API
        /// </summary>
        private async void ConfirmPayment_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(PaymentAmountDisplay.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Iltimos, to'g'ri to'lov summasini kiriting!");
                    return;
                }

                var paymentPayload = new
                {
                    amount = amount,
                    method = _selectedPaymentMethod
                };

                var response = await ApiClient.Client.PostAsJsonAsync(
                    $"/api/orders/orders/{_currentOrder.id}/add_payment/",
                    paymentPayload
                );

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"{amount:N0} so'm to'lov qabul qilindi!");
                    ClosePayment_Click(null, null);
                    // Refresh order data
                    await LoadOrderDetails();
                }
                else
                {
                    MessageBox.Show("To'lovni qo'shishda xatolik: " + response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Xatolik: " + ex.Message);
            }
        }

        /// <summary>
        /// Load/refresh order details from API
        /// </summary>
        private async Task LoadOrderDetails()
        {
            try
            {
                var response = await ApiClient.Client.GetAsync($"/api/orders/orders/{_currentOrder.id}/");
                if (response.IsSuccessStatusCode)
                {
                    string rawJson = await response.Content.ReadAsStringAsync();
                    _currentOrder = JsonSerializer.Deserialize<Order>(rawJson, JsonOptions);
                    MapSavedItemsToCart();
                    RefreshCartUI();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Buyurtma ma'lumotlarini yangilashshda xatolik: " + ex.Message);
            }
        }

        // --- BOTTOM BAR ---
        private void OrdersButton_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new MainPage());
        private void CreateOrderButton_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new TableSelectionPage());
        private void BackButton_Click(object sender, RoutedEventArgs e) { if (this.NavigationService.CanGoBack) this.NavigationService.GoBack(); }
        private void ProfileButton_Click(object sender, RoutedEventArgs e) => this.NavigationService.Navigate(new ProfilePage());
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Tizimdan chiqmoqchimisiz?", "Logout", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                this.NavigationService.Navigate(new LoginPage());
                Dispatcher.BeginInvoke(new Action(() => {
                    while (this.NavigationService.CanGoBack) this.NavigationService.RemoveBackEntry();
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }
    }
}