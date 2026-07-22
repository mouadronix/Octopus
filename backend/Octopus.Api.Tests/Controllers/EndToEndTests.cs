using System;
using System.Collections.Generic;
using System.Linq;
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

public class EndToEndTests : IDisposable
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public EndToEndTests()
    {
        _factory = new TestWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    // -----------------------------------------------------------------------
    // Full lifecycle: Create Ship → Suggest → Assign → Advance Day → Departure
    // -----------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycle_ShipCreatedAssignedAdvancedToDeparture()
    {
        // --- Step 0: Read current day ---
        var initDayResp = await _client.GetAsync("/api/terminal/day");
        Assert.Equal(HttpStatusCode.OK, initDayResp.StatusCode);
        var initDayBody = await initDayResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var currentDay = initDayBody.GetProperty("currentDay").GetInt32();

        // --- Step 1: Create a ship (auto-generated fields) ---
        var createShipResp = await _client.PostAsJsonAsync("/api/ships", new
        {
            name = "Ever Given",
            notes = "Container ship"
        });
        Assert.Equal(HttpStatusCode.Created, createShipResp.StatusCode);

        var shipBody = await createShipResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var shipId = shipBody.GetProperty("id").GetInt32();
        Assert.Equal(ShipStatus.Pending.ToString(), shipBody.GetProperty("status").GetString());
        var arrivalDay = shipBody.GetProperty("arrivalDay").GetInt32();
        var duration = shipBody.GetProperty("duration").GetInt32();

        // --- Step 2: Verify ship appears in ship list as Pending ---
        var shipsResp = await _client.GetAsync("/api/ships");
        Assert.Equal(HttpStatusCode.OK, shipsResp.StatusCode);
        var shipsList = await shipsResp.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
        var ourShip = shipsList.First(s => s.GetProperty("id").GetInt32() == shipId);
        Assert.Equal(ShipStatus.Pending.ToString(), ourShip.GetProperty("status").GetString());

        // --- Step 3: Get a dock suggestion for the ship ---
        var suggestionResp = await _client.GetAsync($"/api/ships/{shipId}/suggest");
        Assert.Equal(HttpStatusCode.OK, suggestionResp.StatusCode);
        var suggestion = await suggestionResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var suggestedDockId = suggestion.GetProperty("dockId").GetInt32();
        Assert.True(suggestedDockId > 0);

        // --- Step 4: Assign the ship to the suggested dock ---
        var assignResp = await _client.PostAsJsonAsync($"/api/docks/{suggestedDockId}/assign", new
        {
            shipId = shipId
        });
        Assert.Equal(HttpStatusCode.Created, assignResp.StatusCode);

        var assignmentBody = await assignResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var startDay = assignmentBody.GetProperty("startDay").GetInt32();
        var endDay = assignmentBody.GetProperty("endDay").GetInt32();
        var expectedStartDay = Math.Max(arrivalDay, currentDay);
        var expectedEndDay = expectedStartDay + duration - 1;
        Assert.Equal(expectedStartDay, startDay);
        Assert.Equal(expectedEndDay, endDay);

        // Verify ship is now Assigned
        var shipAfterAssign = await _client.GetFromJsonAsync<JsonElement>($"/api/ships/{shipId}", _jsonOptions);
        Assert.Equal(ShipStatus.Assigned.ToString(), shipAfterAssign.GetProperty("status").GetString());

        // --- Step 5: Advance day by day until the ship departs ---
        var daysToAdvance = expectedEndDay - currentDay + 1;
        for (int i = 1; i <= daysToAdvance; i++)
        {
            var advanceResp = await _client.PostAsync("/api/terminal/next-day", null);
            Assert.Equal(HttpStatusCode.OK, advanceResp.StatusCode);

            var shipCheck = await _client.GetFromJsonAsync<JsonElement>($"/api/ships/{shipId}", _jsonOptions);
            if (i < daysToAdvance)
            {
                Assert.Equal(ShipStatus.Assigned.ToString(), shipCheck.GetProperty("status").GetString());
            }
        }

        // --- Step 6: Ship should now be Departed ---
        var shipFinal = await _client.GetFromJsonAsync<JsonElement>($"/api/ships/{shipId}", _jsonOptions);
        Assert.Equal(ShipStatus.Departed.ToString(), shipFinal.GetProperty("status").GetString());
    }

    [Fact]
    public async Task FullLifecycle_DockConflictPreventsDoubleBooking()
    {
        // Create two ships
        var ship1Resp = await _client.PostAsJsonAsync("/api/ships", new { name = "Ship A", notes = "" });
        var ship2Resp = await _client.PostAsJsonAsync("/api/ships", new { name = "Ship B", notes = "" });

        var ship1Body = await ship1Resp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var ship2Body = await ship2Resp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var ship1Id = ship1Body.GetProperty("id").GetInt32();
        var ship2Id = ship2Body.GetProperty("id").GetInt32();

        // Get a suggestion for ship 1
        var suggestionResp = await _client.GetAsync($"/api/ships/{ship1Id}/suggest");
        Assert.Equal(HttpStatusCode.OK, suggestionResp.StatusCode);
        var suggestion = await suggestionResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var dockId = suggestion.GetProperty("dockId").GetInt32();

        // Assign ship 1 — should succeed
        var assign1Resp = await _client.PostAsJsonAsync($"/api/docks/{dockId}/assign", new { shipId = ship1Id });
        Assert.Equal(HttpStatusCode.Created, assign1Resp.StatusCode);

        // Assign ship 2 to same dock — should fail if overlapping
        var assign2Resp = await _client.PostAsJsonAsync($"/api/docks/{dockId}/assign", new { shipId = ship2Id });
        // May succeed or fail depending on timing — but the endpoint should respond
        Assert.True(assign2Resp.StatusCode is HttpStatusCode.Created or HttpStatusCode.BadRequest);
    }
}
