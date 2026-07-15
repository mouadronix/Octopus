using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Octopus.Api.DTOs;
using Octopus.Api.Models;
using Octopus.Api.Tests.Helpers;
using Xunit;

namespace Octopus.Api.Tests.Controllers;

public class AssignmentsControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public AssignmentsControllerTests()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    private async Task<ShipListItem> CreateShipAsync(string name = "TestShip")
    {
        var response = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = name }, _jsonOptions);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions))!;
    }

    private async Task<HttpResponseMessage> PostAssignmentAsync(int shipId, int dockId)
    {
        return await _client.PostAsJsonAsync($"/api/docks/{dockId}/assign", new AssignShipRequest { ShipId = shipId }, _jsonOptions);
    }

    // 1. GET /api/docks/assignments should return 200 with a list of assignments
    [Fact]
    public async Task GetAll_ShouldReturnOkWithAssignments()
    {
        var ship = await CreateShipAsync();
        // Get a suggestion to find a valid dock
        var suggestResp = await _client.GetAsync($"/api/ships/{ship.Id}/suggest");
        if (suggestResp.StatusCode == HttpStatusCode.OK)
        {
            var suggestion = await suggestResp.Content.ReadFromJsonAsync<SuggestionResponse>(_jsonOptions);
            Assert.NotNull(suggestion);
            await PostAssignmentAsync(ship.Id, suggestion.DockId);
        }

        var response = await _client.GetAsync("/api/docks/assignments");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // 2. POST with a valid ship and dock should return 201 Created
    [Fact]
    public async Task Create_ValidAssignment_ShouldReturnCreated()
    {
        var ship = await CreateShipAsync(name: "NewShip");

        // Get suggestion to find a valid dock
        var suggestResp = await _client.GetAsync($"/api/ships/{ship.Id}/suggest");
        Assert.Equal(HttpStatusCode.OK, suggestResp.StatusCode);
        var suggestion = await suggestResp.Content.ReadFromJsonAsync<SuggestionResponse>(_jsonOptions);
        Assert.NotNull(suggestion);

        var response = await PostAssignmentAsync(ship.Id, suggestion.DockId);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.True(body.GetProperty("shipId").GetInt32() == ship.Id);
        Assert.True(body.GetProperty("dockId").GetInt32() == suggestion.DockId);
        Assert.True(body.GetProperty("id").GetInt32() > 0);
    }

    // 3. POST with a non-existent ship should return 400 BadRequest
    [Fact]
    public async Task Create_NonExistentShip_ShouldReturnBadRequest()
    {
        var response = await PostAssignmentAsync(shipId: 9999, dockId: 1);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 4. POST with a non-existent dock should return 400 BadRequest
    [Fact]
    public async Task Create_NonExistentDock_ShouldReturnBadRequest()
    {
        var ship = await CreateShipAsync();
        var response = await PostAssignmentAsync(ship.Id, dockId: 9999);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 5. POST with a ship that is already assigned should return 400 BadRequest
    [Fact]
    public async Task Create_AlreadyAssignedShip_ShouldReturnBadRequest()
    {
        var ship = await CreateShipAsync(name: "OnceOnly");

        var suggestResp = await _client.GetAsync($"/api/ships/{ship.Id}/suggest");
        Assert.Equal(HttpStatusCode.OK, suggestResp.StatusCode);
        var suggestion = await suggestResp.Content.ReadFromJsonAsync<SuggestionResponse>(_jsonOptions);
        Assert.NotNull(suggestion);

        // First assignment succeeds
        var first = await PostAssignmentAsync(ship.Id, suggestion.DockId);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // Second assignment of the same ship should fail
        var second = await PostAssignmentAsync(ship.Id, suggestion.DockId);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }
}
