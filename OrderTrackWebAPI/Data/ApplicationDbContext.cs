using Microsoft.EntityFrameworkCore;
using OrderTrackWebAPI.Models;

namespace OrderTrackWebAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Courier> Couriers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<OrderRating> OrderRatings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
            
        // Restaurant - User (1:1)
        modelBuilder.Entity<Restaurant>()
            .HasOne(r => r.User)
            .WithOne(u => u.Restaurant)
            .HasForeignKey<Restaurant>(r => r.UserId);
            
        // Courier - User (1:1)
        modelBuilder.Entity<Courier>()
            .HasOne(c => c.User)
            .WithOne(u => u.Courier)
            .HasForeignKey<Courier>(c => c.UserId);
            
        // Product - Restaurant
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Restaurant)
            .WithMany(r => r.Products)
            .HasForeignKey(p => p.RestaurantId);
            
        // Order - Customer (User)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(u => u.CustomerOrders)
            .HasForeignKey(o => o.CustomerId);
            
        // Order - Restaurant
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Restaurant)
            .WithMany(r => r.Orders)
            .HasForeignKey(o => o.RestaurantId);
            
        // Order - Courier (Optional)
        modelBuilder.Entity<Order>()
            .HasOne(o => o.Courier)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CourierId)
            .IsRequired(false);
            
        // OrderItem - Order
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId);
            
        // OrderItem - Product
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);
            
        // OrderRating - Order (1:1)
        modelBuilder.Entity<OrderRating>()
            .HasOne(or => or.Order)
            .WithOne(o => o.Rating)
            .HasForeignKey<OrderRating>(or => or.OrderId);
    }
}
