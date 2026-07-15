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

public class ShipsControllerTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public ShipsControllerTests()
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

    // 1. GetAll should return OK with a list of ships
    [Fact]
    public async Task GetAll_ShouldReturnOkWithShips()
    {
        // Seed two ships via the API
        await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = "Ship A",
            Size = ShipSize.M,
            ArrivalDay = 1,
            Duration = 5
        }, _jsonOptions);

        await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = "Ship B",
            Size = ShipSize.S,
            ArrivalDay = 3,
            Duration = 2
        }, _jsonOptions);

        var response = await _client.GetAsync("/api/ships");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ships = await response.Content.ReadFromJsonAsync<List<ShipListItem>>(_jsonOptions);
        Assert.NotNull(ships);
        Assert.Equal(2, ships.Count);
    }

    // 2. GetAll with status filter should return only matching ships
    [Fact]
    public async Task GetAll_WithStatusFilter_ShouldReturnFiltered()
    {
        // All created ships default to Pending status
        await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = "Pending Ship",
            Size = ShipSize.M,
            ArrivalDay = 1,
            Duration = 5
        }, _jsonOptions);

        // Filter by Pending - should return the ship
        var pendingResponse = await _client.GetAsync("/api/ships?status=Pending");
        Assert.Equal(HttpStatusCode.OK, pendingResponse.StatusCode);

        var pendingShips = await pendingResponse.Content.ReadFromJsonAsync<List<ShipListItem>>(_jsonOptions);
        Assert.NotNull(pendingShips);
        Assert.Single(pendingShips);
        Assert.Equal("Pending Ship", pendingShips[0].Name);

        // Filter by Assigned - should return empty list
        var assignedResponse = await _client.GetAsync("/api/ships?status=Assigned");
        Assert.Equal(HttpStatusCode.OK, assignedResponse.StatusCode);

        var assignedShips = await assignedResponse.Content.ReadFromJsonAsync<List<ShipListItem>>(_jsonOptions);
        Assert.NotNull(assignedShips);
        Assert.Empty(assignedShips);
    }

    // 3. GetById with an existing ID should return the ship
    [Fact]
    public async Task GetById_ExistingId_ShouldReturnShip()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = "FindMe",
            Size = ShipSize.L,
            ArrivalDay = 2,
            Duration = 3
        }, _jsonOptions);

        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/ships/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ship = await response.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(ship);
        Assert.Equal("FindMe", ship.Name);
        Assert.Equal(ShipSize.L, ship.Size);
        Assert.Equal(ShipStatus.Pending, ship.Status);
        Assert.Equal(2, ship.ArrivalDay);
        Assert.Equal(3, ship.Duration);
    }

    // 4. GetById with a non-existing ID should return 404
    [Fact]
    public async Task GetById_NonExistingId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/ships/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 5. Create with a valid request should return 201 Created with the ship
    [Fact]
    public async Task Create_ValidRequest_ShouldReturnCreated()
    {
        var request = new CreateShipRequest
        {
            Name = "New Ship",
            Size = ShipSize.M,
            ArrivalDay = 5,
            Duration = 10,
            Notes = "Test notes"
        };

        var response = await _client.PostAsJsonAsync("/api/ships", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var ship = await response.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(ship);
        Assert.True(ship.Id > 0);
        Assert.Equal("New Ship", ship.Name);
        Assert.Equal(ShipSize.M, ship.Size);
        Assert.Equal(5, ship.ArrivalDay);
        Assert.Equal(10, ship.Duration);
        Assert.Equal("Test notes", ship.Notes);
        Assert.Equal(ShipStatus.Pending, ship.Status);
    }

    // 6. Create should persist the ship so it can be fetched later
    [Fact]
    public async Task Create_ValidRequest_ShouldPersistShip()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = "Persistent",
            Size = ShipSize.S,
            ArrivalDay = 1,
            Duration = 3
        }, _jsonOptions);

        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        // Fetch the ship back to verify it was actually persisted
        var getResponse = await _client.GetAsync($"/api/ships/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal("Persistent", fetched.Name);
        Assert.Equal(ShipSize.S, fetched.Size);
        Assert.Equal(ShipStatus.Pending, fetched.Status);
    }

    // 7. Create with invalid model data should return a validation problem
    [Fact]
    public async Task Create_InvalidModel_ShouldReturnValidationProblem()
    {
        // Send request with empty Name which violates [Required]
        var invalidRequest = new
        {
            Name = "",
            Size = "M",
            ArrivalDay = 1,
            Duration = 5
        };

        var response = await _client.PostAsJsonAsync("/api/ships", invalidRequest, _jsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 8. Delete an existing ship should return 204 No Content
    [Fact]
    public async Task Delete_ExistingShip_ShouldReturnNoContent()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = "ToDelete",
            Size = ShipSize.M,
            ArrivalDay = 1,
            Duration = 2
        }, _jsonOptions);

        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        var deleteResponse = await _client.DeleteAsync($"/api/ships/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify the ship is gone
        var getResponse = await _client.GetAsync($"/api/ships/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    // 9. Delete a non-existing ship should return 404
    [Fact]
    public async Task Delete_NonExistingShip_ShouldReturnNotFound()
    {
        var response = await _client.DeleteAsync("/api/ships/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 10. GetSuggestion with an available dock should return a suggestion
    [Fact]
    public async Task GetSuggestion_WithAvailableDock_ShouldReturnSuggestion()
    {
        // Create a small ship that fits Dock C (size S)
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = "SmallShip",
            Size = ShipSize.S,
            ArrivalDay = 1,
            Duration = 3
        }, _jsonOptions);

        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/ships/{created.Id}/suggestion");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var suggestion = await response.Content.ReadFromJsonAsync<SuggestionResponse>(_jsonOptions);
        Assert.NotNull(suggestion);
        Assert.True(suggestion.DockId > 0);
        Assert.False(string.IsNullOrEmpty(suggestion.DockName));
        Assert.True(suggestion.StartDay >= 1);
        Assert.False(string.IsNullOrEmpty(suggestion.Message));
    }

    // 11. GetSuggestion with no fitting dock should return 404
    [Fact]
    public async Task GetSuggestion_NoDock_ShouldReturnNotFound()
    {
        // Create an XL ship - no dock is large enough (largest dock is L)
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = "HugeShip",
            Size = ShipSize.XL,
            ArrivalDay = 1,
            Duration = 3
        }, _jsonOptions);

        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/ships/{created.Id}/suggestion");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
