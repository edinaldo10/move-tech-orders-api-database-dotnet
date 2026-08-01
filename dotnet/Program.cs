namespace cloud_application;

using cloud_application.Models;
using cloud_application.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o suporte ao OpenAPI nativo do .NET (necessário para o Scalar)
builder.Services.AddOpenApi();

// Captura a string de conexão priorizando a variável de ambiente DATABASE_URL
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = databaseUrl ?? builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db";

// Ajuste para aceitar tanto "postgres://" quanto "postgresql://" de forma robusta
if (!string.IsNullOrEmpty(databaseUrl) && (databaseUrl.StartsWith("postgres://") || databaseUrl.StartsWith("postgresql://")))
{
    try
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Prefer;Trust Server Certificate=true";
    }
    catch
    {
        // Se falhar o parse da URI, tenta usar a string diretamente
        connectionString = databaseUrl;
    }
}

if (connectionString.Contains("Host=") || connectionString.Contains("Server="))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
}

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Bloco protegido para evitar conflitos de concorrência com múltiplas réplicas no Kubernetes
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

// Habilita o endpoint do OpenAPI nativo
app.MapOpenApi();

app.MapGet("/docs", () => Results.Redirect("/scalar/v1"))
   .ExcludeFromDescription();

app.MapScalarApiReference(options =>
{
    options.Title = "API de Pedidos (.NET)";
});

// Endpoint de health retornando 503 caso o banco falhe
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

// NECESSÁRIO PARA O WEBAPPLICATIONFACTORY DOS TESTES FUNCIONAREM:
public partial class Program { }
