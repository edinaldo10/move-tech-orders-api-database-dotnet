using CloudApplication.Data;
using CloudApplication.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Captura a string de conexão priorizando a variável de ambiente DATABASE_URL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = databaseUrl ?? builder.Configuration.GetConnectionString("DefaultConnection");

// Ajuste robusto para aceitar URLs do PostgreSQL (ex: postgres://user:pass@host:port/db)
if (!string.IsNullOrEmpty(databaseUrl) && (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://")))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        var user = userInfo.Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : string.Empty;
        var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var port = uri.Port > 0 ? uri.Port : 5432;

        connectionString = $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')};Username={user};Password={pass};SSL Mode=Prefer;Trust Server Certificate=true";
    }
    catch
    {
        connectionString = databaseUrl;
    }
}

// Configuração exclusiva para PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Bloco protegido para criar o banco/tabelas caso ainda não existam
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.EnsureCreated();
    }
    catch
    {
        // Ignora caso outra réplica já esteja criando as tabelas simultaneamente
    }
}

app.MapGet("/docs", () => Results.Redirect("/scalar/v1"))
   .ExcludeFromDescription();

app.MapScalarApiReference(options =>
{
    options.Title = "API de Pedidos (.NET)";
});

// Endpoint de health verificando a conectividade com o PostgreSQL
app.MapGet("/health", async (AppDbContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        if (canConnect)
        {
            return Results.Ok(new { status = "ok", database = "ok" });
        }
        return Results.StatusCode(503);
    }
    catch
    {
        return Results.StatusCode(503);
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

public partial class Program { }