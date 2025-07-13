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
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets all products
    /// </summary>
    /// <returns>List of products</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.Restaurant)
            .ToListAsync();
            
        var productDtos = products.Select(p => new ProductDto
        {
            Id = p.Id,
            RestaurantId = p.RestaurantId,
            RestaurantName = p.Restaurant.Name,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            IsAvailable = p.IsAvailable
        });
        
        return Ok(productDtos);
    }
    
    /// <summary>
    /// Gets a product by id
    /// </summary>
    /// <param name="id">Product id</param>
    /// <returns>Product details</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Restaurant)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (product == null)
            return NotFound();
            
        var productDto = new ProductDto
        {
            Id = product.Id,
            RestaurantId = product.RestaurantId,
            RestaurantName = product.Restaurant.Name,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsAvailable = product.IsAvailable
        };
        
        return Ok(productDto);
    }
    
    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="model">Product details</param>
    /// <returns>Created product</returns>
    [HttpPost]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<ProductDto>> CreateProduct(CreateProductDto model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.UserId == userId);
            
        if (restaurant == null)
            return BadRequest("Restaurant not found for this user");
            
        var product = new Product
        {
            RestaurantId = restaurant.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            IsAvailable = model.IsAvailable
        };
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        var productDto = new ProductDto
        {
            Id = product.Id,
            RestaurantId = product.RestaurantId,
            RestaurantName = restaurant.Name,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsAvailable = product.IsAvailable
        };
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, productDto);
    }
    
    /// <summary>
    /// Updates a product
    /// </summary>
    /// <param name="id">Product id</param>
    /// <param name="model">Updated product details</param>
    /// <returns>Updated product</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, UpdateProductDto model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.UserId == userId);
            
        if (restaurant == null)
            return BadRequest("Restaurant not found for this user");
            
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.RestaurantId == restaurant.Id);
            
        if (product == null)
            return NotFound("Product not found or you don't have permission to update it");
            
        product.Name = model.Name;
        product.Description = model.Description;
        product.Price = model.Price;
        product.IsAvailable = model.IsAvailable;
        product.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        var productDto = new ProductDto
        {
            Id = product.Id,
            RestaurantId = product.RestaurantId,
            RestaurantName = restaurant.Name,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            IsAvailable = product.IsAvailable
        };
        
        return Ok(productDto);
    }
    
    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product id</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var restaurant = await _context.Restaurants
            .FirstOrDefaultAsync(r => r.UserId == userId);
            
        if (restaurant == null)
            return BadRequest("Restaurant not found for this user");
            
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.RestaurantId == restaurant.Id);
            
        if (product == null)
            return NotFound("Product not found or you don't have permission to delete it");
            
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}
