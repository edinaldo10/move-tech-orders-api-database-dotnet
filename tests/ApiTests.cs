using System.Net;
using System.Net.Http.Json;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using CloudApplication.Data;
using CloudApplication.Models;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                     d.ServiceType == typeof(DbContextOptions)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // Força o uso do PostgreSQL também nos testes (pegando da variável de ambiente ou string padrão do workflow)
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
                                   ?? "Host=localhost;Port=5432;Database=orders;Username=postgres;Password=postgres";

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Garante que o banco e as tabelas sejam criados/migrados antes dos testes rodarem no Postgres
            db.Database.EnsureCreated();
        });
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