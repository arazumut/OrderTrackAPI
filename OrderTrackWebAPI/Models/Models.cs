namespace OrderTrackWebAPI.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Customer, Restaurant, Courier
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Restaurant? Restaurant { get; set; }
    public Courier? Courier { get; set; }
    public List<Order> CustomerOrders { get; set; } = new();
}

public class Restaurant
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public List<Product> Products { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}

public class Courier
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public List<Order> Orders { get; set; } = new();
}

public class Product
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public List<OrderItem> OrderItems { get; set; } = new();
}

public enum OrderStatus
{
    Pending, // Sipariş alındı, hazırlanmayı bekliyor
    Confirmed, // Restoran siparişi onayladı
    Preparing, // Hazırlanıyor
    ReadyForPickup, // Kurye tarafından alınmaya hazır
    InTransit, // Kuryede, taşınıyor
    Delivered, // Teslim edildi
    Cancelled // İptal edildi
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; } // User Id (Role = Customer)
    public User Customer { get; set; } = null!;
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public int? CourierId { get; set; }
    public Courier? Courier { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string Note { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    
    // Navigation properties
    public List<OrderItem> Items { get; set; } = new();
    public OrderRating? Rating { get; set; }
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; } // Ürünün sipariş anındaki fiyatı
}

public class OrderRating
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public int Rating { get; set; } // 1-5 arası
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
