using Microsoft.EntityFrameworkCore;
using CloudApplication.Models;

namespace CloudApplication.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Mudado de Orders para orders
    public DbSet<Order> orders => Set<Order>();
    public DbSet<Item> items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<Item>().ToTable("items");
    }
}