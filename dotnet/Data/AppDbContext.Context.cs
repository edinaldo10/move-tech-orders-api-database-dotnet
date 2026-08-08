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

        // Mapeia explicitamente as tabelas para o PostgreSQL (usando minúsculas se preferir padrão SQL, ou mantendo Orders/Items)
        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<Item>().ToTable("items");
    }
}