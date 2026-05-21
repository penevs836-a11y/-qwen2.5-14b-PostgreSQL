using Microsoft.EntityFrameworkCore;
using AiShoppingAssistant.Models;

namespace AiShoppingAssistant;

public class AppDbContext : DbContext
{
    public DbSet<Store> Stores { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<SearchHistory> History { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=YOUR_PASSWORD");
    }
}

