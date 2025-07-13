using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderTrackWebAPI.Data;
using OrderTrackWebAPI.DTOs;
using OrderTrackWebAPI.Models;

namespace OrderTrackWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Creates a new order
    /// </summary>
    /// <param name="model">Order details</param>
    /// <returns>Created order</returns>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var restaurant = await _context.Restaurants.FindAsync(model.RestaurantId);
        if (restaurant == null)
            return BadRequest("Restaurant not found");
            
        // Verify all products belong to the restaurant and are available
        var productIds = model.Items.Select(i => i.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id) && p.RestaurantId == model.RestaurantId && p.IsAvailable)
            .ToListAsync();
            
        if (products.Count != productIds.Count)
            return BadRequest("One or more products are not available or don't belong to the restaurant");
            
        // Calculate total amount
        decimal totalAmount = 0;
        var orderItems = new List<OrderItem>();
        
        foreach (var item in model.Items)
        {
            var product = products.First(p => p.Id == item.ProductId);
            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };
            
            orderItems.Add(orderItem);
            totalAmount += product.Price * item.Quantity;
        }
        
        var order = new Order
        {
            CustomerId = userId,
            RestaurantId = model.RestaurantId,
            Note = model.Note,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            Items = orderItems
        };
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        var orderItemDtos = order.Items.Select(oi => new OrderItemDto
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            ProductName = products.First(p => p.Id == oi.ProductId).Name,
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,
            TotalPrice = oi.UnitPrice * oi.Quantity
        }).ToList();
        
        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
            RestaurantId = order.RestaurantId,
            RestaurantName = restaurant.Name,
            Status = order.Status.ToString(),
            Note = order.Note,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            Items = orderItemDtos
        };
        
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, orderDto);
    }
    
    /// <summary>
    /// Gets an order by id
    /// </summary>
    /// <param name="id">Order id</param>
    /// <returns>Order details</returns>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Courier)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Rating)
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (order == null)
            return NotFound();
            
        // Check permissions
        if (userRole == "Customer" && order.CustomerId != userId)
            return Forbid();
            
        if (userRole == "Restaurant")
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.UserId == userId);
            if (restaurant == null || order.RestaurantId != restaurant.Id)
                return Forbid();
        }
        
        if (userRole == "Courier")
        {
            var courier = await _context.Couriers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (courier == null || order.CourierId != courier.Id)
                return Forbid();
        }
        
        var orderItemDtos = order.Items.Select(oi => new OrderItemDto
        {
            Id = oi.Id,
            ProductId = oi.ProductId,
            ProductName = oi.Product.Name,
            Quantity = oi.Quantity,
            UnitPrice = oi.UnitPrice,
            TotalPrice = oi.UnitPrice * oi.Quantity
        }).ToList();
        
        OrderRatingDto? ratingDto = null;
        if (order.Rating != null)
        {
            ratingDto = new OrderRatingDto
            {
                Id = order.Rating.Id,
                OrderId = order.Rating.OrderId,
                Rating = order.Rating.Rating,
                Comment = order.Rating.Comment
            };
        }
        
        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.Username,
            RestaurantId = order.RestaurantId,
            RestaurantName = order.Restaurant.Name,
            CourierId = order.CourierId,
            CourierName = order.Courier?.Name,
            Status = order.Status.ToString(),
            Note = order.Note,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            DeliveredAt = order.DeliveredAt,
            Items = orderItemDtos,
            Rating = ratingDto
        };
        
        return Ok(orderDto);
    }
    
    /// <summary>
    /// Gets all orders for the current customer
    /// </summary>
    /// <returns>List of orders</returns>
    [HttpGet]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetCustomerOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var orders = await _context.Orders
            .Include(o => o.Restaurant)
            .Include(o => o.Courier)
            .Where(o => o.CustomerId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
            
        var orderDtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            CustomerName = User.FindFirst(ClaimTypes.Name)?.Value ?? "",
            RestaurantId = o.RestaurantId,
            RestaurantName = o.Restaurant.Name,
            CourierId = o.CourierId,
            CourierName = o.Courier?.Name,
            Status = o.Status.ToString(),
            Note = o.Note,
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt,
            DeliveredAt = o.DeliveredAt
        });
        
        return Ok(orderDtos);
    }
    
    /// <summary>
    /// Gets all orders for the current restaurant
    /// </summary>
    /// <returns>List of orders</returns>
    [HttpGet("restaurant")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetRestaurantOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.UserId == userId);
            
        if (restaurant == null)
            return BadRequest("Restaurant not found for this user");
            
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Courier)
            .Where(o => o.RestaurantId == restaurant.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
            
        var orderDtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer.Username,
            RestaurantId = o.RestaurantId,
            RestaurantName = restaurant.Name,
            CourierId = o.CourierId,
            CourierName = o.Courier?.Name,
            Status = o.Status.ToString(),
            Note = o.Note,
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt,
            DeliveredAt = o.DeliveredAt
        });
        
        return Ok(orderDtos);
    }
    
    /// <summary>
    /// Gets all orders for the current courier
    /// </summary>
    /// <returns>List of orders</returns>
    [HttpGet("courier")]
    [Authorize(Roles = "Courier")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetCourierOrders()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.UserId == userId);
            
        if (courier == null)
            return BadRequest("Courier not found for this user");
            
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Where(o => o.CourierId == courier.Id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
            
        var orderDtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer.Username,
            RestaurantId = o.RestaurantId,
            RestaurantName = o.Restaurant.Name,
            CourierId = o.CourierId,
            CourierName = courier.Name,
            Status = o.Status.ToString(),
            Note = o.Note,
            TotalAmount = o.TotalAmount,
            CreatedAt = o.CreatedAt,
            DeliveredAt = o.DeliveredAt
        });
        
        return Ok(orderDtos);
    }
    
    /// <summary>
    /// Updates an order's status
    /// </summary>
    /// <param name="id">Order id</param>
    /// <param name="model">Updated status</param>
    /// <returns>Updated order</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Restaurant,Courier")]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, UpdateOrderStatusDto model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .Include(o => o.Courier)
            .FirstOrDefaultAsync(o => o.Id == id);
            
        if (order == null)
            return NotFound();
            
        // Check permissions
        if (userRole == "Restaurant")
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.UserId == userId);
            if (restaurant == null || order.RestaurantId != restaurant.Id)
                return Forbid();
                
            // Restaurants can only change status to Confirmed, Preparing, ReadyForPickup, or Cancelled
            if (!Enum.TryParse<OrderStatus>(model.Status, out var newStatus) ||
                (newStatus != OrderStatus.Confirmed && 
                 newStatus != OrderStatus.Preparing && 
                 newStatus != OrderStatus.ReadyForPickup &&
                 newStatus != OrderStatus.Cancelled))
            {
                return BadRequest("Invalid status or not allowed to change to this status");
            }
        }
        else if (userRole == "Courier")
        {
            var courier = await _context.Couriers.FirstOrDefaultAsync(c => c.UserId == userId);
            if (courier == null || order.CourierId != courier.Id)
                return Forbid();
                
            // Couriers can only change status to InTransit or Delivered
            if (!Enum.TryParse<OrderStatus>(model.Status, out var newStatus) ||
                (newStatus != OrderStatus.InTransit && 
                 newStatus != OrderStatus.Delivered))
            {
                return BadRequest("Invalid status or not allowed to change to this status");
            }
            
            // If status is changed to Delivered, set DeliveredAt
            if (newStatus == OrderStatus.Delivered)
            {
                order.DeliveredAt = DateTime.UtcNow;
            }
        }
        
        if (!Enum.TryParse<OrderStatus>(model.Status, out var status))
            return BadRequest("Invalid order status");
            
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.Username,
            RestaurantId = order.RestaurantId,
            RestaurantName = order.Restaurant.Name,
            CourierId = order.CourierId,
            CourierName = order.Courier?.Name,
            Status = order.Status.ToString(),
            Note = order.Note,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            DeliveredAt = order.DeliveredAt
        };
        
        return Ok(orderDto);
    }
    
    /// <summary>
    /// Assigns a courier to an order
    /// </summary>
    /// <param name="id">Order id</param>
    /// <param name="model">Courier assignment details</param>
    /// <returns>Updated order</returns>
    [HttpPatch("{id}/assign-courier")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<OrderDto>> AssignCourier(int id, AssignCourierDto model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.UserId == userId);
        if (restaurant == null)
            return BadRequest("Restaurant not found for this user");
            
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.Restaurant)
            .FirstOrDefaultAsync(o => o.Id == id && o.RestaurantId == restaurant.Id);
            
        if (order == null)
            return NotFound("Order not found or you don't have permission to update it");
            
        if (order.Status != OrderStatus.ReadyForPickup)
            return BadRequest("Order must be in ReadyForPickup status to assign a courier");
            
        var courier = await _context.Couriers.FindAsync(model.CourierId);
        if (courier == null)
            return BadRequest("Courier not found");
            
        order.CourierId = model.CourierId;
        order.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        var orderDto = new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer.Username,
            RestaurantId = order.RestaurantId,
            RestaurantName = order.Restaurant.Name,
            CourierId = order.CourierId,
            CourierName = courier.Name,
            Status = order.Status.ToString(),
            Note = order.Note,
            TotalAmount = order.TotalAmount,
            CreatedAt = order.CreatedAt,
            DeliveredAt = order.DeliveredAt
        };
        
        return Ok(orderDto);
    }
    
    /// <summary>
    /// Rates an order
    /// </summary>
    /// <param name="id">Order id</param>
    /// <param name="model">Rating details</param>
    /// <returns>Created rating</returns>
    [HttpPost("{id}/rate")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<OrderRatingDto>> RateOrder(int id, CreateOrderRatingDto model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var order = await _context.Orders
            .Include(o => o.Rating)
            .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == userId);
            
        if (order == null)
            return NotFound("Order not found or you don't have permission to rate it");
            
        if (order.Status != OrderStatus.Delivered)
            return BadRequest("Only delivered orders can be rated");
            
        if (order.Rating != null)
            return BadRequest("Order has already been rated");
            
        var rating = new OrderRating
        {
            OrderId = id,
            Rating = model.Rating,
            Comment = model.Comment
        };
        
        _context.OrderRatings.Add(rating);
        await _context.SaveChangesAsync();
        
        var ratingDto = new OrderRatingDto
        {
            Id = rating.Id,
            OrderId = rating.OrderId,
            Rating = rating.Rating,
            Comment = rating.Comment
        };
        
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, ratingDto);
    }
}
