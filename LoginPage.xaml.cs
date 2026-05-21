using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using rms_gui.Services;
using rms_gui.Models;

namespace rms_gui
{
    public partial class LoginPage : Page
    {
        private string _pinInput = string.Empty;
        private PasswordBox _pinBox;
        private TextBox _usernameBox;
        private PasswordBox _passwordBox;

        public LoginPage()
        {
            InitializeComponent();
            Loaded += LoginPage_Loaded;
        }

        private void InitializeControls()
        {
            _pinBox = (PasswordBox)FindName("PinBox");
            _usernameBox = (TextBox)FindName("UsernameBox");
            _passwordBox = (PasswordBox)FindName("PasswordBox");
        }

        // --- PIN LOGIN METHODS ---

        private async void NumpadButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            string content = btn.Content.ToString();

            if (content == "C")
            {
                _pinInput = string.Empty;
            }
            else if (content == "<")
            {
                if (_pinInput.Length > 0)
                    _pinInput = _pinInput.Substring(0, _pinInput.Length - 1);
            }
            else if (int.TryParse(content, out _))
            {
                if (_pinInput.Length < 4)
                    _pinInput += content;
            }

            if (_pinBox != null)
                _pinBox.Password = _pinInput;

            // Auto-submit when PIN reaches 4 digits
            if (_pinInput.Length == 4)
            {
                await LoginWithPinAsync(_pinInput);
            }
        }

        private async Task LoginWithPinAsync(string pin)
        {
            try
            {
                await ApiClient.EnsureCsrfTokenAsync();
                var loginRequest = new LoginRequest { pin = pin };

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await ApiClient.Client.PostAsync("/api/users/auth/login/", content);
                ApiClient.UpdateCsrfToken();
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<User>();
                    var TableWindow = new TableSelectionWindow();
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"PIN noto'g'ri! ({response.StatusCode})\n{errorContent}", "Login Failed");
                    _pinInput = string.Empty;
                    if (_pinBox != null) _pinBox.Password = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login xatolik: " + ex.Message);
                _pinInput = string.Empty;
                if (_pinBox != null) _pinBox.Password = "";
            }
        }

        // --- USERNAME/PASSWORD LOGIN ---

        private void LoginTabButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn.Content.ToString() == "PIN LOGIN")
            {
                PinLoginSection.Visibility = Visibility.Visible;
                UsernameLoginSection.Visibility = Visibility.Collapsed;
            }
            else
            {
                PinLoginSection.Visibility = Visibility.Collapsed;
                UsernameLoginSection.Visibility = Visibility.Visible;
            }
        }

        private async void UsernameLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (_usernameBox == null || _passwordBox == null)
            {
                MessageBox.Show("Login form elements not found");
                return;
            }

            string username = _usernameBox.Text;
            string password = _passwordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Foydalanuvchi nomi va parol kiritish shart! (Username and password required)");
                return;
            }

            await LoginWithUsernameAsync(username, password);
        }

        private async Task LoginWithUsernameAsync(string username, string password)
        {
            try
            {
                await ApiClient.EnsureCsrfTokenAsync();
                var loginRequest = new LoginRequest { username = username, password = password };

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var response = await ApiClient.Client.PostAsync("/api/users/auth/login/", content);
                ApiClient.UpdateCsrfToken();
                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<User>();
                    var TableWindow = new TableSelectionWindow();
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Foydalanuvchi nomi yoki parol noto'g'ri! ({response.StatusCode})\n{errorContent}", "Login Failed");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Login xatolik: " + ex.Message);
            }
        }
    }
}