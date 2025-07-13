using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderTrackWebAPI.Data;
using OrderTrackWebAPI.DTOs;
using OrderTrackWebAPI.Models;
using OrderTrackWebAPI.Services;

namespace OrderTrackWebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;
    
    public AuthController(ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _authService = authService;
    }
    
    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="model">Registration details</param>
    /// <returns>User info with token</returns>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto model)
    {
        if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            return BadRequest("Username is already taken");
            
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            return BadRequest("Email is already taken");
            
        if (model.Role != "Customer" && model.Role != "Restaurant" && model.Role != "Courier")
            return BadRequest("Invalid role");
            
        _authService.CreatePasswordHash(model.Password, out string passwordHash, out string passwordSalt);
        
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = model.Role
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = _authService.GenerateJwtToken(user)
        });
    }
    
    /// <summary>
    /// Registers a new restaurant user with restaurant details
    /// </summary>
    /// <param name="model">Restaurant registration details</param>
    /// <returns>User info with token</returns>
    [HttpPost("register/restaurant")]
    public async Task<ActionResult<UserDto>> RegisterRestaurant(RestaurantRegisterDto model)
    {
        if (model.Role != "Restaurant")
            return BadRequest("Role must be Restaurant");
        
        if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            return BadRequest("Username is already taken");
            
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            return BadRequest("Email is already taken");
            
        _authService.CreatePasswordHash(model.Password, out string passwordHash, out string passwordSalt);
        
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = "Restaurant"
        };
        
        var restaurant = new Restaurant
        {
            User = user,
            Name = model.Name,
            Address = model.Address,
            PhoneNumber = model.PhoneNumber,
            Description = model.Description
        };
        
        _context.Users.Add(user);
        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();
        
        var restaurantDto = new RestaurantDto
        {
            Id = restaurant.Id,
            Name = restaurant.Name,
            Address = restaurant.Address,
            PhoneNumber = restaurant.PhoneNumber,
            Description = restaurant.Description
        };
        
        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = _authService.GenerateJwtToken(user),
            Restaurant = restaurantDto
        });
    }
    
    /// <summary>
    /// Registers a new courier user with courier details
    /// </summary>
    /// <param name="model">Courier registration details</param>
    /// <returns>User info with token</returns>
    [HttpPost("register/courier")]
    public async Task<ActionResult<UserDto>> RegisterCourier(CourierRegisterDto model)
    {
        if (model.Role != "Courier")
            return BadRequest("Role must be Courier");
        
        if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            return BadRequest("Username is already taken");
            
        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            return BadRequest("Email is already taken");
            
        _authService.CreatePasswordHash(model.Password, out string passwordHash, out string passwordSalt);
        
        var user = new User
        {
            Username = model.Username,
            Email = model.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Role = "Courier"
        };
        
        var courier = new Courier
        {
            User = user,
            Name = model.Name,
            PhoneNumber = model.PhoneNumber,
            VehicleType = model.VehicleType,
            VehiclePlate = model.VehiclePlate
        };
        
        _context.Users.Add(user);
        _context.Couriers.Add(courier);
        await _context.SaveChangesAsync();
        
        var courierDto = new CourierDto
        {
            Id = courier.Id,
            Name = courier.Name,
            PhoneNumber = courier.PhoneNumber,
            VehicleType = courier.VehicleType,
            VehiclePlate = courier.VehiclePlate
        };
        
        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = _authService.GenerateJwtToken(user),
            Courier = courierDto
        });
    }
    
    /// <summary>
    /// Authenticates a user
    /// </summary>
    /// <param name="model">Login credentials</param>
    /// <returns>User info with token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto model)
    {
        var user = await _context.Users
            .Include(u => u.Restaurant)
            .Include(u => u.Courier)
            .FirstOrDefaultAsync(u => u.Username == model.Username);
            
        if (user == null)
            return Unauthorized("Invalid username");
            
        if (!_authService.VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
            return Unauthorized("Invalid password");
            
        RestaurantDto? restaurantDto = null;
        if (user.Restaurant != null)
        {
            restaurantDto = new RestaurantDto
            {
                Id = user.Restaurant.Id,
                Name = user.Restaurant.Name,
                Address = user.Restaurant.Address,
                PhoneNumber = user.Restaurant.PhoneNumber,
                Description = user.Restaurant.Description
            };
        }
        
        CourierDto? courierDto = null;
        if (user.Courier != null)
        {
            courierDto = new CourierDto
            {
                Id = user.Courier.Id,
                Name = user.Courier.Name,
                PhoneNumber = user.Courier.PhoneNumber,
                VehicleType = user.Courier.VehicleType,
                VehiclePlate = user.Courier.VehiclePlate
            };
        }
        
        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = _authService.GenerateJwtToken(user),
            Restaurant = restaurantDto,
            Courier = courierDto
        });
    }
    
    /// <summary>
    /// Gets the current user's information
    /// </summary>
    /// <returns>User info</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var user = await _context.Users
            .Include(u => u.Restaurant)
            .Include(u => u.Courier)
            .FirstOrDefaultAsync(u => u.Id == userId);
            
        if (user == null)
            return NotFound();
            
        RestaurantDto? restaurantDto = null;
        if (user.Restaurant != null)
        {
            restaurantDto = new RestaurantDto
            {
                Id = user.Restaurant.Id,
                Name = user.Restaurant.Name,
                Address = user.Restaurant.Address,
                PhoneNumber = user.Restaurant.PhoneNumber,
                Description = user.Restaurant.Description
            };
        }
        
        CourierDto? courierDto = null;
        if (user.Courier != null)
        {
            courierDto = new CourierDto
            {
                Id = user.Courier.Id,
                Name = user.Courier.Name,
                PhoneNumber = user.Courier.PhoneNumber,
                VehicleType = user.Courier.VehicleType,
                VehiclePlate = user.Courier.VehiclePlate
            };
        }
        
        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = _authService.GenerateJwtToken(user),
            Restaurant = restaurantDto,
            Courier = courierDto
        });
    }
}
