using System.ComponentModel.DataAnnotations;
using OrderTrackWebAPI.Models;

namespace OrderTrackWebAPI.DTOs;

// Auth DTOs
public class RegisterDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;
    
    [Required]
    public string Role { get; set; } = string.Empty; // Customer, Restaurant, Courier
}

public class RestaurantRegisterDto : RegisterDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
}

public class CourierRegisterDto : RegisterDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    public string VehicleType { get; set; } = string.Empty;
    
    [Required]
    public string VehiclePlate { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public RestaurantDto? Restaurant { get; set; }
    public CourierDto? Courier { get; set; }
}

// Restaurant DTOs
public class RestaurantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateRestaurantDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
}

// Product DTOs
public class ProductDto
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 10000)]
    public decimal Price { get; set; }
    
    public bool IsAvailable { get; set; } = true;
}

public class UpdateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, 10000)]
    public decimal Price { get; set; }
    
    public bool IsAvailable { get; set; } = true;
}

// Courier DTOs
public class CourierDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
}

public class UpdateCourierDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [Phone]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [Required]
    public string VehicleType { get; set; } = string.Empty;
    
    [Required]
    public string VehiclePlate { get; set; } = string.Empty;
}

// Order DTOs
public class OrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int RestaurantId { get; set; }
    public string RestaurantName { get; set; } = string.Empty;
    public int? CourierId { get; set; }
    public string? CourierName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public OrderRatingDto? Rating { get; set; }
}

public class CreateOrderDto
{
    [Required]
    public int RestaurantId { get; set; }
    
    [Required]
    public List<OrderItemCreateDto> Items { get; set; } = new();
    
    public string Note { get; set; } = string.Empty;
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class OrderItemCreateDto
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, 100)]
    public int Quantity { get; set; }
}

public class UpdateOrderStatusDto
{
    [Required]
    public string Status { get; set; } = string.Empty;
}

public class AssignCourierDto
{
    [Required]
    public int CourierId { get; set; }
}

public class OrderRatingDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class CreateOrderRatingDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
    
    public string Comment { get; set; } = string.Empty;
}
