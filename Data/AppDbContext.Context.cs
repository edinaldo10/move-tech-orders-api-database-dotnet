using Microsoft.EntityFrameworkCore;
using CloudApplication.Models;

namespace CloudApplication.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Item> Items => Set<Item>();
}