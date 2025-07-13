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
public class CouriersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public CouriersController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets all couriers
    /// </summary>
    /// <returns>List of couriers</returns>
    [HttpGet]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<IEnumerable<CourierDto>>> GetCouriers()
    {
        var couriers = await _context.Couriers.ToListAsync();
        
        var courierDtos = couriers.Select(c => new CourierDto
        {
            Id = c.Id,
            Name = c.Name,
            PhoneNumber = c.PhoneNumber,
            VehicleType = c.VehicleType,
            VehiclePlate = c.VehiclePlate
        });
        
        return Ok(courierDtos);
    }
    
    /// <summary>
    /// Gets a courier by id
    /// </summary>
    /// <param name="id">Courier id</param>
    /// <returns>Courier details</returns>
    [HttpGet("{id}")]
    [Authorize(Roles = "Restaurant")]
    public async Task<ActionResult<CourierDto>> GetCourier(int id)
    {
        var courier = await _context.Couriers.FindAsync(id);
        
        if (courier == null)
            return NotFound();
            
        var courierDto = new CourierDto
        {
            Id = courier.Id,
            Name = courier.Name,
            PhoneNumber = courier.PhoneNumber,
            VehicleType = courier.VehicleType,
            VehiclePlate = courier.VehiclePlate
        };
        
        return Ok(courierDto);
    }
    
    /// <summary>
    /// Updates a courier's details
    /// </summary>
    /// <param name="id">Courier id</param>
    /// <param name="model">Updated courier details</param>
    /// <returns>Updated courier</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Courier")]
    public async Task<ActionResult<CourierDto>> UpdateCourier(int id, UpdateCourierDto model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (courier == null)
            return NotFound("Courier not found or you don't have permission to update it");
            
        courier.Name = model.Name;
        courier.PhoneNumber = model.PhoneNumber;
        courier.VehicleType = model.VehicleType;
        courier.VehiclePlate = model.VehiclePlate;
        courier.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        
        var courierDto = new CourierDto
        {
            Id = courier.Id,
            Name = courier.Name,
            PhoneNumber = courier.PhoneNumber,
            VehicleType = courier.VehicleType,
            VehiclePlate = courier.VehiclePlate
        };
        
        return Ok(courierDto);
    }
    
    /// <summary>
    /// Deletes a courier
    /// </summary>
    /// <param name="id">Courier id</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Courier")]
    public async Task<ActionResult> DeleteCourier(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var courier = await _context.Couriers
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
            
        if (courier == null)
            return NotFound("Courier not found or you don't have permission to delete it");
            
        // Check if courier has active orders
        var hasActiveOrders = await _context.Orders
            .AnyAsync(o => o.CourierId == id && 
                     (o.Status == OrderStatus.ReadyForPickup || 
                      o.Status == OrderStatus.InTransit));
                      
        if (hasActiveOrders)
            return BadRequest("Cannot delete courier with active orders");
            
        var user = await _context.Users.FindAsync(courier.UserId);
        if (user != null)
        {
            _context.Users.Remove(user); // Also remove the associated user
        }
        
        _context.Couriers.Remove(courier);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
}
