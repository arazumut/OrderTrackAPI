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
public class RestaurantsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public RestaurantsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets all restaurants
    /// </summary>
    /// <returns>List of restaurants</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RestaurantDto>>> GetRestaurants()
    {
        var restaurants = await _context.Restaurants.ToListAsync();
        
        var restaurantDtos = restaurants.Select(r => new RestaurantDto
        {
            Id = r.Id,
            Name = r.Name,
            Address = r.Address,
            PhoneNumber = r.PhoneNumber,
            Description = r.Description
        });
        
        return Ok(restaurantDtos);
    }
    
    /// <summary>
    /// Gets a restaurant by id
    /// </summary>
    /// <param name="id">Restaurant id</param>
    /// <returns>Restaurant details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<RestaurantDto>> GetRestaurant(int id)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        
        if (restaurant == null)
            return NotFound();
            
        var restaurantDto = new RestaurantDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            Address = restaurant.Address,
            PhoneNumber = restaurant.PhoneNumber,
            Description = restaurant.Description
        };
        
        return Ok(restaurantDto);
    }
    
    /// <summary>
    /// Updates a restaurant's details
    /// </summary>
    /// <param name="id">Restaurant id</param>
    /// <param name="model">Updated restaurant details</param>
    /// <returns>Updated restaurant</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<RestaurantDto>> UpdateRestaurant(int id, UpdateRestaurantDto model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
            
        if (restaurant == null)
            return NotFound("Restaurant not found or you don't have permission to update it");
            
        restaurant.Name = model.Name;
        restaurant.Address = model.Address;
        restaurant.PhoneNumber = model.PhoneNumber;
        restaurant.Description = model.Description;
        restaurant.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        var restaurantDto = new RestaurantDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            Address = restaurant.Address,
            PhoneNumber = restaurant.PhoneNumber,
            Description = restaurant.Description
        };
        
        return Ok(restaurantDto);
    }
    
    /// <summary>
    /// Gets all products for a specific restaurant
    /// </summary>
    /// <param name="id">Restaurant id</param>
    /// <returns>List of products</returns>
    [HttpGet("{id}/products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetRestaurantProducts(int id)
    {
        var restaurant = await _context.Restaurants.FindAsync(id);
        
        if (restaurant == null)
            return NotFound("Restaurant not found");
            
        var products = await _context.Products
            .Where(p => p.RestaurantId == id)
            .ToListAsync();
            
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            RestaurantId = p.RestaurantId,
            RestaurantName = restaurant.Name,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            IsAvailable = p.IsAvailable
        });
        
        return Ok(productDtos);
    }
}
