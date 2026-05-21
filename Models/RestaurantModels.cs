using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace rms_gui.Models
{
    // --- AUTHENTICATION MODELS ---
    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string username { get; set; }
        [JsonPropertyName("password")]
        public string password { get; set; }
        [JsonPropertyName("pin")]
        public string pin { get; set; }
    }

    public class User
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        [JsonPropertyName("username")]
        public string username { get; set; }
        [JsonPropertyName("first_name")]
        public string first_name { get; set; }
        [JsonPropertyName("role")]
        public string role { get; set; }
    }

    // --- TABLE & LOCATION MODELS ---
    public class Location
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
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
        public int id { get; set; }
        [JsonPropertyName("name")]
        public string name { get; set; }
        [JsonPropertyName("capacity")]
        public int capacity { get; set; }
        [JsonPropertyName("availability")]
        public bool availability { get; set; }
        [JsonPropertyName("tax")]
        public decimal? tax { get; set; }
        [JsonPropertyName("location")]
        public int location_id { get; set; }
    }

    // --- MENU MODELS ---
    public class MenuCategory
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        [JsonPropertyName("name")]
        public string name { get; set; }

        [JsonPropertyName("items")]
        public List<MenuItem> items { get; set; }
    }

    public class MenuItem
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
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
        public int id { get; set; }
        [JsonPropertyName("table")]
        public int table_id { get; set; }
        [JsonPropertyName("waiter")]
        public int? waiter_id { get; set; }
        [JsonPropertyName("customer_quantity")]
        public int customer_quantity { get; set; }
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
    }

    public class OrderItem
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        [JsonPropertyName("order_id")]
        public int order_id { get; set; }
        [JsonPropertyName("menu_item")]
        public int menu_item_id { get; set; }

        [JsonPropertyName("menu_item")]
        public MenuItem menu_item { get; set; }

        [JsonPropertyName("quantity")]
        public int quantity { get; set; }
        [JsonPropertyName("price")]
        public decimal price { get; set; }
        [JsonPropertyName("is_deleted")]
        public bool is_deleted { get; set; }
    }

    public class OrderPayment
    {
        [JsonPropertyName("id")]
        public int id { get; set; }
        [JsonPropertyName("order_id")]
        public int order_id { get; set; }
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
    }
}