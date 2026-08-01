using Microsoft.EntityFrameworkCore;
using cloud_application.Data; // <- Add semicolon here

namespace cloud_application.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Item> Items => Set<Item>();
}
