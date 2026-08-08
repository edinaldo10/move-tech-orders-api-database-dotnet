using Microsoft.EntityFrameworkCore;
using CloudApplication.Data;
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

        // Força os nomes exatos das tabelas em minúsculas no banco de dados
        modelBuilder.Entity<Order>().ToTable("orders");
        modelBuilder.Entity<Item>().ToTable("items");
    }
}