using System.Net;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CloudApplication.Data;
using CloudApplication.Models;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove qualquer configuração anterior do DbContext
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                     d.ServiceType == typeof(DbContextOptions)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Abre uma conexão SQLite compartilhada em memória que não fecha ao trocar de thread
            _connection = new SqliteConnection("DataSource=Filename=:memory:;Cache=Shared");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Cria o banco e as tabelas com o nome correto esperado pelo EF Core ("Orders")
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close();
        _connection?.Dispose();
    }
}

public class ApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Test_Health()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(json);
        Assert.True(json.ContainsKey("status"));
        Assert.True(json.ContainsKey("database"));
    }

    [Fact]
    public async Task Test_Create_Order()
    {
        var response = await _client.PostAsJsonAsync("/orders", new { customer = "Maria" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<Order>();
        Assert.NotNull(order);
        Assert.Equal("Maria", order.Customer);
        Assert.Equal("Created", order.Status);
        Assert.False(string.IsNullOrEmpty(order.Id));
    }
}