using CloudApplication.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net;
using System.Net.Http.Json;
using Xunit;

public class ApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiTests(WebApplicationFactory<Program> factory)
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
        Assert.Equal("open", order.Status);
        Assert.False(string.IsNullOrEmpty(order.Id));
    }
}