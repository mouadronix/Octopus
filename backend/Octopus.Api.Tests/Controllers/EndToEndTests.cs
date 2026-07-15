using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
    // Full lifecycle: Create Ship → Assign to Dock → Advance Day → Departure
    // -----------------------------------------------------------------------

    [Fact]
    public async Task FullLifecycle_ShipCreatedAssignedAdvancedToDeparture()
    {
        // --- Step 0: Read current day from the system ---
        var initStateResp = await _client.GetAsync("/api/system/state");
        Assert.Equal(HttpStatusCode.OK, initStateResp.StatusCode);
        var initState = await initStateResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var currentDay = initState.GetProperty("currentDay").GetInt32();
        var arrivalDay = currentDay; // ship arrives today
        var duration = 2;

        // --- Step 1: Create a ship ---
        var createShipResp = await _client.PostAsJsonAsync("/api/ships", new
        {
            name = "Ever Given",
            notes = "Container ship",
            size = ShipSize.M.ToString(),
            arrivalDay = arrivalDay,
            duration = duration
        });
        Assert.Equal(HttpStatusCode.Created, createShipResp.StatusCode);

        var shipBody = await createShipResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var shipId = shipBody.GetProperty("id").GetInt32();
        Assert.Equal(ShipStatus.Pending.ToString(), shipBody.GetProperty("status").GetString());

        // --- Step 2: Verify ship appears in ship list as Pending ---
        var shipsResp = await _client.GetAsync("/api/ships");
        Assert.Equal(HttpStatusCode.OK, shipsResp.StatusCode);
        var shipsList = await shipsResp.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
        var ourShip = shipsList.First(s => s.GetProperty("id").GetInt32() == shipId);
        Assert.Equal(ShipStatus.Pending.ToString(), ourShip.GetProperty("status").GetString());

        // --- Step 3: Get a dock suggestion for the ship ---
        var suggestionResp = await _client.GetAsync($"/api/ships/{shipId}/suggestion");
        Assert.Equal(HttpStatusCode.OK, suggestionResp.StatusCode);
        var suggestion = await suggestionResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var suggestedDockId = suggestion.GetProperty("dockId").GetInt32();
        Assert.True(suggestedDockId > 0);

        // --- Step 4: Assign the ship to the suggested dock ---
        var assignResp = await _client.PostAsJsonAsync("/api/assignments", new
        {
            shipId = shipId,
            dockId = suggestedDockId
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
        Assert.True(shipAfterAssign.GetProperty("assignmentId").GetInt32() > 0);
        Assert.Equal(suggestion.GetProperty("dockName").GetString(), shipAfterAssign.GetProperty("berthName").GetString());

        // --- Step 5: Verify system state shows the assignment ---
        var stateResp = await _client.GetAsync("/api/system/state");
        Assert.Equal(HttpStatusCode.OK, stateResp.StatusCode);
        var state = await stateResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.Equal(currentDay, state.GetProperty("currentDay").GetInt32());

        // --- Step 6: Advance day by day until the ship departs ---
        // The ship departs when currentDay > endDay
        var daysToAdvance = expectedEndDay - currentDay + 1; // one past endDay
        for (int i = 1; i <= daysToAdvance; i++)
        {
            var advanceResp = await _client.PostAsync("/api/system/advance-day", null);
            Assert.Equal(HttpStatusCode.OK, advanceResp.StatusCode);
            var advancedState = await advanceResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
            Assert.Equal(currentDay + i, advancedState.GetProperty("currentDay").GetInt32());

            var shipCheck = await _client.GetFromJsonAsync<JsonElement>($"/api/ships/{shipId}", _jsonOptions);
            if (i < daysToAdvance)
            {
                // Before departure: still Assigned
                Assert.Equal(ShipStatus.Assigned.ToString(), shipCheck.GetProperty("status").GetString());
            }
        }

        // --- Step 7: Ship should now be Departed ---
        var shipFinal = await _client.GetFromJsonAsync<JsonElement>($"/api/ships/{shipId}", _jsonOptions);
        Assert.Equal(ShipStatus.Departed.ToString(), shipFinal.GetProperty("status").GetString());

        // --- Step 8: Verify the dock is now free — assign a new ship to the same dock ---
        var newShipResp = await _client.PostAsJsonAsync("/api/ships", new
        {
            name = "Maersk Alabama",
            notes = "Another container ship",
            size = ShipSize.M.ToString(),
            arrivalDay = currentDay + daysToAdvance,
            duration = 1
        });
        Assert.Equal(HttpStatusCode.Created, newShipResp.StatusCode);
        var newShipBody = await newShipResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var newShipId = newShipBody.GetProperty("id").GetInt32();

        var reassignResp = await _client.PostAsJsonAsync("/api/assignments", new
        {
            shipId = newShipId,
            dockId = suggestedDockId
        });
        Assert.Equal(HttpStatusCode.Created, reassignResp.StatusCode);
    }

    [Fact]
    public async Task FullLifecycle_ShipTooLargeForAllDocks_ReturnsNotFoundOnSuggestion()
    {
        // Create an XL ship — seeded docks are L, M, S (none fits XL)
        var initStateResp = await _client.GetAsync("/api/system/state");
        var initState = await initStateResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var currentDay = initState.GetProperty("currentDay").GetInt32();

        var createResp = await _client.PostAsJsonAsync("/api/ships", new
        {
            name = "Oasis Class",
            notes = "Mega ship",
            size = ShipSize.XL.ToString(),
            arrivalDay = currentDay,
            duration = 3
        });
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var shipBody = await createResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var shipId = shipBody.GetProperty("id").GetInt32();

        // Suggestion should return 404 — no dock large enough
        var suggestionResp = await _client.GetAsync($"/api/ships/{shipId}/suggestion");
        Assert.Equal(HttpStatusCode.NotFound, suggestionResp.StatusCode);
    }

    [Fact]
    public async Task FullLifecycle_DockConflictPreventsDoubleBooking()
    {
        // Read current day so we can set meaningful arrival days
        var initStateResp = await _client.GetAsync("/api/system/state");
        var initState = await initStateResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var currentDay = initState.GetProperty("currentDay").GetInt32();

        // Create two ships arriving at the same time
        var ship1Resp = await _client.PostAsJsonAsync("/api/ships", new
        {
            name = "Ship A",
            notes = "",
            size = ShipSize.S.ToString(),
            arrivalDay = currentDay,
            duration = 3
        });
        var ship2Resp = await _client.PostAsJsonAsync("/api/ships", new
        {
            name = "Ship B",
            notes = "",
            size = ShipSize.S.ToString(),
            arrivalDay = currentDay,
            duration = 2
        });

        var ship1Body = await ship1Resp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var ship2Body = await ship2Resp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var ship1Id = ship1Body.GetProperty("id").GetInt32();
        var ship2Id = ship2Body.GetProperty("id").GetInt32();

        // Get a suggestion for ship 1 to find a valid S-size dock
        var suggestionResp = await _client.GetAsync($"/api/ships/{ship1Id}/suggestion");
        Assert.Equal(HttpStatusCode.OK, suggestionResp.StatusCode);
        var suggestion = await suggestionResp.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var dockId = suggestion.GetProperty("dockId").GetInt32();

        // Assign ship 1 — should succeed
        var assign1Resp = await _client.PostAsJsonAsync("/api/assignments", new
        {
            shipId = ship1Id,
            dockId = dockId
        });
        Assert.Equal(HttpStatusCode.Created, assign1Resp.StatusCode);

        // Assign ship 2 to same dock with overlapping days — should fail (conflict)
        var assign2Resp = await _client.PostAsJsonAsync("/api/assignments", new
        {
            shipId = ship2Id,
            dockId = dockId
        });
        Assert.Equal(HttpStatusCode.BadRequest, assign2Resp.StatusCode);
    }
}
