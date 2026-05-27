using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace rms_gui.Models
{
    // --- AUTHENTICATION MODELS ---
    public class PinLoginRequest
    {
        [JsonPropertyName("pin")]
        public int pin { get; set; }
    }
    public class LoginRequest
    {
        [JsonPropertyName ("username")]
        public string username { get; set; }
        [JsonPropertyName ("password")]
        public string password { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")]
        public string id { get; set; } // Updated to string for UUID
        [JsonPropertyName("username")]
        public string username { get; set; }
        [JsonPropertyName("first_name")]
        public string first_name { get; set; }
        [JsonPropertyName("role")]
        public string role { get; set; }
        [JsonPropertyName("name")]
        public string name { get; set; }
    }

    // --- TABLE & LOCATION MODELS ---
    public class Location
    {
        [JsonPropertyName("id")]
        public string id { get; set; } // Updated to string for UUID
        [JsonPropertyName("name")]
        public string name { get; set; }
        [JsonPropertyName("default_tax")]
        public decimal default_tax { get; set; }

        [JsonPropertyName("tables")]
        public List<Table> tables { get; set; }
    }

    public class Table
    {
        [JsonPropertyName("id")]
        public string id { get; set; } // Updated to string for UUID
        [JsonPropertyName("name")]
        public string name { get; set; }
        [JsonPropertyName("capacity")]
        public int capacity { get; set; }
        [JsonPropertyName("availability")]
        public bool availability { get; set; }
        [JsonPropertyName("tax")]
        public decimal? tax { get; set; }

        [JsonPropertyName("location.id")] // Fixed mapping
        public string location_id { get; set; } // Updated to string for UUID
    }

    // --- MENU MODELS ---
    public class MenuCategory
    {
        [JsonPropertyName("id")]
        public string id { get; set; } // Updated to string for UUID
        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("items")]
        public List<MenuItem> items { get; set; }
    }

    public class MenuItem
    {
        [JsonPropertyName("id")]
        public string id { get; set; } // Updated to string for UUID
        [JsonPropertyName("name")]
        public string name { get; set; }
        [JsonPropertyName("description")]
        public string description { get; set; }
        [JsonPropertyName("price")]
        public decimal price { get; set; }
    }

    // --- ORDER MODELS ---
    public class Order
    {
        [JsonPropertyName("id")]
        public string id { get; set; } // Updated to string for UUID

        [JsonPropertyName("table")] // Fixed mapping
        public string table_id { get; set; } // Updated to string for UUID

        [JsonPropertyName("waiter")] // Fixed mapping
        public string waiter_id { get; set; } // Updated to string for UUID

        [JsonPropertyName("customer_quantity")]
        public int? customer_quantity { get; set; }
        [JsonPropertyName("status")]
        public string status { get; set; }
        [JsonPropertyName("raw_price")]
        public decimal raw_price { get; set; }
        [JsonPropertyName("price")]
        public decimal price { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItem> items { get; set; }
        [JsonPropertyName("payments")]
        public List<OrderPayment> payments { get; set; }
        [JsonPropertyName("waiter_name")]
        public string waiter_name { get; set; }
        [JsonPropertyName("table_name")]
        public string table_name { get; set; }
        [JsonPropertyName("location_name")]
        public string location_name { get; set; }
        [JsonPropertyName("tax")]
        public decimal? tax { get; set; }
    }

    public class OrderItem
    {
        [JsonPropertyName("id")]
        public string id { get; set; } // Updated to string for UUID

        [JsonPropertyName("order")] // Fixed mapping
        public string order_id { get; set; } // Updated to string for UUID

        [JsonPropertyName("menu_item")] // Fixed mapping
        public string menu_item_id { get; set; } // Updated to string for UUID

        // REMOVED the duplicate MenuItem object property that caused the collision!

        [JsonPropertyName("quantity")]
        public int quantity { get; set; }
        [JsonPropertyName("price")]
        public decimal price { get; set; }
        [JsonPropertyName("is_deleted")]
        public bool is_deleted { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime? created_at { get; set; }
    }

    public class OrderPayment
    {
        [JsonPropertyName("id")]
        public string id { get; set; } // Updated to string for UUID

        [JsonPropertyName("order")] // Fixed mapping
        public string order_id { get; set; } // Updated to string for UUID

        [JsonPropertyName("amount")]
        public decimal amount { get; set; }
        [JsonPropertyName("method")]
        public string method { get; set; }
    }

    public class CartItem
    {
        public MenuItem Product { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Product.price * Quantity;
        public bool IsSaved { get; set; }
        public DateTime? CreatedAt { get; set; }

        public string TimesinceCreated
        {
            get
            {
                if(!IsSaved) return "Yangi";
                if (CreatedAt.HasValue) return "Saqlangan";

                var span = DateTime.UtcNow - CreatedAt.Value.ToUniversalTime();
                if (span.TotalSeconds < 60) return $"{(int)span.TotalSeconds} bir necha soniya oldin";
                return $"{(int)span.TotalMinutes} daqiqa oldin"; 
            }
        }
    }
}