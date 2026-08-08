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

        // Mapeia explicitamente a entidade Order para a tabela "orders" em minúsculas
        modelBuilder.Entity<Order>().ToTable("orders");
        
        // Caso a tabela de itens também precise seguir o mesmo padrão minúsculo:
        modelBuilder.Entity<Item>().ToTable("items");
    }
}