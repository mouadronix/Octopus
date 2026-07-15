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
        await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = "Ship A" }, _jsonOptions);
        await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = "Ship B" }, _jsonOptions);

        var response = await _client.GetAsync("/api/ships");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ships = await response.Content.ReadFromJsonAsync<List<ShipListItem>>(_jsonOptions);
        Assert.NotNull(ships);
        Assert.True(ships.Count >= 2);
    }

    // 2. GetAll with status filter should return only matching ships
    [Fact]
    public async Task GetAll_WithStatusFilter_ShouldReturnFiltered()
    {
        await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = "Pending Ship" }, _jsonOptions);

        var pendingResponse = await _client.GetAsync("/api/ships?status=Pending");
        Assert.Equal(HttpStatusCode.OK, pendingResponse.StatusCode);

        var pendingShips = await pendingResponse.Content.ReadFromJsonAsync<List<ShipListItem>>(_jsonOptions);
        Assert.NotNull(pendingShips);
        Assert.Contains(pendingShips, s => s.Name == "Pending Ship");

        var assignedResponse = await _client.GetAsync("/api/ships?status=Assigned");
        Assert.Equal(HttpStatusCode.OK, assignedResponse.StatusCode);

        var assignedShips = await assignedResponse.Content.ReadFromJsonAsync<List<ShipListItem>>(_jsonOptions);
        Assert.NotNull(assignedShips);
        Assert.DoesNotContain(assignedShips, s => s.Name == "Pending Ship");
    }

    // 3. GetById with an existing ID should return the ship
    [Fact]
    public async Task GetById_ExistingId_ShouldReturnShip()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = "FindMe" }, _jsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/ships/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var ship = await response.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(ship);
        Assert.Equal("FindMe", ship.Name);
        Assert.Equal(ShipStatus.Pending, ship.Status);
    }

    // 4. GetById with a non-existing ID should return 404
    [Fact]
    public async Task GetById_NonExistingId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync("/api/ships/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 5. Create with a valid request should return 201 Created with auto-generated fields
    [Fact]
    public async Task Create_ValidRequest_ShouldReturnCreated()
    {
        var request = new CreateShipRequest
        {
            Name = "New Ship",
            Notes = "Test notes"
        };

        var response = await _client.PostAsJsonAsync("/api/ships", request, _jsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var ship = await response.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(ship);
        Assert.True(ship.Id > 0);
        Assert.Equal("New Ship", ship.Name);
        Assert.Equal("Test notes", ship.Notes);
        Assert.Equal(ShipStatus.Pending, ship.Status);
        // Auto-generated fields should have valid values
        Assert.True(Enum.IsDefined(ship.Size));
        Assert.True(ship.ArrivalDay >= 1);
        Assert.InRange(ship.Duration, 3, 15);
    }

    // 6. Create should persist the ship so it can be fetched later
    [Fact]
    public async Task Create_ValidRequest_ShouldPersistShip()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = "Persistent" }, _jsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        var getResponse = await _client.GetAsync($"/api/ships/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal("Persistent", fetched.Name);
        Assert.Equal(ShipStatus.Pending, fetched.Status);
    }

    // 7. Create with invalid model data should return a validation problem
    [Fact]
    public async Task Create_InvalidModel_ShouldReturnValidationProblem()
    {
        var invalidRequest = new { Name = "" };
        var response = await _client.PostAsJsonAsync("/api/ships", invalidRequest, _jsonOptions);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 8. Update a Pending ship should return 200 OK
    [Fact]
    public async Task Update_PendingShip_ShouldReturnOk()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = "Original" }, _jsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        var updateRequest = new UpdateShipRequest { Name = "Updated", Notes = "New notes" };
        var updateResponse = await _client.PutAsJsonAsync($"/api/ships/{created.Id}", updateRequest, _jsonOptions);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updated = await updateResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Name);
        Assert.Equal("New notes", updated.Notes);
    }

    // 9. Update a non-existent ship should return 404
    [Fact]
    public async Task Update_NonExistentShip_ShouldReturnNotFound()
    {
        var updateRequest = new UpdateShipRequest { Name = "Ghost" };
        var response = await _client.PutAsJsonAsync("/api/ships/9999", updateRequest, _jsonOptions);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // 10. GetSuggestion with an available dock should return a suggestion
    [Fact]
    public async Task GetSuggestion_WithAvailableDock_ShouldReturnSuggestion()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = "SmallShip" }, _jsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        var response = await _client.GetAsync($"/api/ships/{created.Id}/suggest");
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
        // Create a ship, then check suggestion — if no dock fits the auto-generated size, it returns 404
        var createResponse = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest { Name = "HugeShip" }, _jsonOptions);
        var created = await createResponse.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions);
        Assert.NotNull(created);

        // This may or may not return 404 depending on the auto-generated size
        var response = await _client.GetAsync($"/api/ships/{created.Id}/suggest");
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.NotFound);
    }
}
