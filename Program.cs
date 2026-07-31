using Microsoft.EntityFrameworkCore;
using CloudApplication.Data;
using CloudApplication.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";

if (connectionString.Contains("Host="))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/docs", () => Results.Redirect("/scalar/v1"))
   .ExcludeFromDescription();

app.MapScalarApiReference(options =>
{
    options.Title = "API de Pedidos (.NET)";
});

app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        await db.Database.CanConnectAsync();
        return Results.Ok(new { status = "ok", database = "ok" });
    }
    catch
    {
        return Results.Ok(new { status = "degraded", database = "unavailable" });
    }
}).WithTags("health");

app.MapPost("/orders", async (OrderCreateDto dto, AppDbContext db) =>
{
    var order = new Order
    {
        Customer = dto.Customer
    };
    db.Orders.Add(order);
    await db.SaveChangesAsync();
    return Results.Created($"/orders/{order.Id}", order);
}).WithTags("orders");

app.MapGet("/orders", async (AppDbContext db) =>
    await db.Orders.Include(o => o.Items).ToListAsync()
).WithTags("orders");

app.MapGet("/orders/{id}", async (string id, AppDbContext db) =>
{
    var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
    return order is not null ? Results.Ok(order) : Results.NotFound(new { detail = "Pedido não encontrado" });
}).WithTags("orders");

app.MapPost("/orders/{id}/items", async (string id, ItemCreateDto dto, AppDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound(new { detail = "Pedido não encontrado" });

    var item = new Item
    {
        OrderId = id,
        Sku = dto.Sku,
        Description = dto.Description,
        Quantity = dto.Quantity
    };
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/orders/{id}/items/{item.Id}", item);
}).WithTags("items");

app.MapGet("/orders/{id}/items", async (string id, AppDbContext db) =>
{
    var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
    if (order is null) return Results.NotFound(new { detail = "Pedido não encontrado" });
    return Results.Ok(order.Items);
}).WithTags("items");

app.MapDelete("/orders/{id}", async (string id, AppDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null) return Results.NotFound(new { detail = "Pedido não encontrado" });
    order.Status = "cancelled";
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithTags("orders");

app.Run();

record OrderCreateDto(string Customer);
record ItemCreateDto(string Sku, string Description, int Quantity);