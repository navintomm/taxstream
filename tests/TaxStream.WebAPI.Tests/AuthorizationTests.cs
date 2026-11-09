using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace TaxStream.WebAPI.Tests;

public class AuthorizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthorizationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // GET endpoints — no token → 401
    [Theory]
    [InlineData("/api/taxrecords")]
    [InlineData("/api/ai/history")]
    public async Task GetEndpoints_WithoutToken_Return401(string url)
    {
        var response = await _client.GetAsync(url);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // POST endpoints — no token → 401
    // POST /api/ai/chat requires a JSON body, but auth runs before model binding
    [Theory]
    [InlineData("/api/ai/chat")]
    [InlineData("/api/taxrecords")]
    public async Task PostEndpoints_WithoutToken_Return401(string url)
    {
        // Empty JSON body — auth middleware rejects before it even reads the body
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync(url, content);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // PUT endpoint — no token → 401
    [Fact]
    public async Task PutTaxRecord_WithoutToken_Returns401()
    {
        var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _client.PutAsync($"/api/taxrecords/{Guid.NewGuid()}", content);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // DELETE endpoint — no token → 401
    [Fact]
    public async Task DeleteTaxRecord_WithoutToken_Returns401()
    {
        var response = await _client.DeleteAsync($"/api/taxrecords/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
