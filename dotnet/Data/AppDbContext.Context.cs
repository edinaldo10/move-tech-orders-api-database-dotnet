using Microsoft.EntityFrameworkCore;
using CloudApplication.Models;

namespace CloudApplication.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Item> Items => Set<Item>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeia explicitamente ambas as tabelas para garantir que o SQLite as crie corretamente
        modelBuilder.Entity<Order>().ToTable("Orders");
        modelBuilder.Entity<Item>().ToTable("Items");
    }
}