using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace rms_gui.Services
{
    public static class ApiClient
    {
        private static readonly CookieContainer _cookieContainer = new CookieContainer();
        private static readonly HttpClientHandler _handler;
        public static readonly HttpClient Client;

        private const string BaseUrl = "http://localhost:8000";

        static ApiClient()
        {
            _handler = new HttpClientHandler
            {
                CookieContainer = _cookieContainer,
                UseCookies = true // Automatically handles sessionid and csrftoken
            };

            Client = new HttpClient(_handler)
            {
                BaseAddress = new Uri(BaseUrl)
            };
            // Tell Django we want JSON
            Client.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // Call this after any request to ensure the CSRF header is updated
        public static void UpdateCsrfToken()
        {
            var cookies = _cookieContainer.GetCookies(new Uri(BaseUrl));
            var csrfCookie = cookies["csrftoken"];
            if (csrfCookie != null)
            {
                if (Client.DefaultRequestHeaders.Contains("X-CSRFToken"))
                    Client.DefaultRequestHeaders.Remove("X-CSRFToken");

                Client.DefaultRequestHeaders.Add("X-CSRFToken", csrfCookie.Value);
            }
        }

        // Utility to perform an initial GET to grab the CSRF token before logging in
        public static async Task EnsureCsrfTokenAsync()
        {
            try
            {
                // Try to get CSRF token from a safe endpoint
                // If it fails, we'll just continue without it since the API may handle it differently
                await Client.GetAsync("/api/users/users/");
                UpdateCsrfToken();
            }
            catch
            {
                // CSRF endpoint may not exist, continue anyway
            }
        }
    }
}