using CloudApplication.Data;
using CloudApplication.Models;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Captura a string de conexão priorizando a variável de ambiente, depois a config, e por fim um padrão
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = databaseUrl
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Port=5432;Database=orders;Username=postgres;Password=postgres";

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

// Configuração dinâmica: Suporta PostgreSQL por padrão ou SQLite caso especificado (útil para testes)
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
        connectionString.Contains("Filename=", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Inicialização segura do banco garantindo que as tabelas existam antes de aceitar requisições
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        // Se for SQLite, garante que a conexão in-memory permaneça aberta para preservar as tabelas
        if (db.Database.IsSqlite())
        {
            var connection = db.Database.GetDbConnection();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                connection.Open();
            }
        }
        db.Database.EnsureCreated();
    }
    catch
    {
        // Ignora conflitos de concorrência com múltiplas instâncias
    }
}

app.MapGet("/docs", () => Results.Redirect("/scalar/v1"))
   .ExcludeFromDescription();

app.MapScalarApiReference(options =>
{
    options.Title = "API de Pedidos (.NET)";
});

// Endpoint de health verificando a conectividade com o banco
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
        Id = Guid.NewGuid().ToString(),
        Customer = dto.Customer,
        Status = "Created",
        CreatedAt = DateTime.UtcNow
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
        Id = Guid.NewGuid().ToString(),
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