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

    // ----------------------------------------------------------------
    // Helper: create a ship via the API and return the deserialized response
    // ----------------------------------------------------------------
    private async Task<ShipListItem> CreateShipAsync(
        string name = "TestShip",
        ShipSize size = ShipSize.M,
        int arrivalDay = 1,
        int duration = 5)
    {
        var response = await _client.PostAsJsonAsync("/api/ships", new CreateShipRequest
        {
            Name = name,
            Size = size,
            ArrivalDay = arrivalDay,
            Duration = duration
        }, _jsonOptions);

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ShipListItem>(_jsonOptions))!;
    }

    // ----------------------------------------------------------------
    // Helper: create an assignment via the API
    // ----------------------------------------------------------------
    private async Task<HttpResponseMessage> PostAssignmentAsync(int shipId, int dockId)
    {
        return await _client.PostAsJsonAsync("/api/assignments", new AssignShipRequest
        {
            ShipId = shipId,
            DockId = dockId
        }, _jsonOptions);
    }

    // 1. GET /api/assignments should return 200 with a list of assignments
    [Fact]
    public async Task GetAll_ShouldReturnOkWithAssignments()
    {
        // Create a ship and assign it to Dock A (L)
        var ship = await CreateShipAsync(size: ShipSize.M);
        await PostAssignmentAsync(ship.Id, dockId: 1);

        var response = await _client.GetAsync("/api/assignments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var assignments = await response.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
        Assert.NotNull(assignments);
        Assert.True(assignments.Count >= 1);
    }

    // 2. POST with a valid ship and dock should return 201 Created
    [Fact]
    public async Task Create_ValidAssignment_ShouldReturnCreated()
    {
        var ship = await CreateShipAsync(name: "NewShip", size: ShipSize.S, arrivalDay: 1, duration: 3);

        var response = await PostAssignmentAsync(ship.Id, dockId: 3); // Dock C is size S

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.True(body.GetProperty("shipId").GetInt32() == ship.Id);
        Assert.True(body.GetProperty("dockId").GetInt32() == 3);
        Assert.True(body.GetProperty("id").GetInt32() > 0);
        Assert.True(body.GetProperty("startDay").GetInt32() >= 1);
        Assert.True(body.GetProperty("endDay").GetInt32() >= body.GetProperty("startDay").GetInt32());
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
        var ship = await CreateShipAsync(size: ShipSize.S);

        var response = await PostAssignmentAsync(ship.Id, dockId: 9999);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 5. POST with a ship that is already assigned should return 400 BadRequest
    [Fact]
    public async Task Create_AlreadyAssignedShip_ShouldReturnBadRequest()
    {
        var ship = await CreateShipAsync(name: "OnceOnly", size: ShipSize.S);

        // First assignment succeeds
        var first = await PostAssignmentAsync(ship.Id, dockId: 3);
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        // Second assignment of the same ship should fail
        var second = await PostAssignmentAsync(ship.Id, dockId: 2);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
    }

    // 6. POST with a ship too large for the dock should return 400 BadRequest
    [Fact]
    public async Task Create_ShipTooLarge_ShouldReturnBadRequest()
    {
        // Ship size L, Dock C size S — ship is too large
        var ship = await CreateShipAsync(name: "BigShip", size: ShipSize.L);

        var response = await PostAssignmentAsync(ship.Id, dockId: 3); // Dock C is S

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // 7. POST response should include the Ship navigation object
    [Fact]
    public async Task Create_ShouldIncludeShipInResponse()
    {
        var ship = await CreateShipAsync(name: "IncludedShip", size: ShipSize.M, arrivalDay: 2, duration: 4);

        var response = await PostAssignmentAsync(ship.Id, dockId: 2); // Dock B is M

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);

        Assert.True(body.TryGetProperty("ship", out var shipProp));
        Assert.Equal("IncludedShip", shipProp.GetProperty("name").GetString());
        Assert.Equal(ShipSize.M.ToString(), shipProp.GetProperty("size").GetString());
        Assert.Equal(ShipStatus.Assigned.ToString(), shipProp.GetProperty("status").GetString());
        Assert.Equal(2, shipProp.GetProperty("arrivalDay").GetInt32());
        Assert.Equal(4, shipProp.GetProperty("duration").GetInt32());
    }
}
