using System;
using System.Collections.Generic;
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

public class RemainingControllersTests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public RemainingControllersTests()
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

    // ================================================================
    // SystemController tests
    // ================================================================

    [Fact]
    public async Task System_GetState_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/system/state");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task System_GetState_ShouldContainCorrectCounts()
    {
        var response = await _client.GetAsync("/api/system/state");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var state = await response.Content.ReadFromJsonAsync<SystemState>(_jsonOptions);
        Assert.NotNull(state);
        Assert.True(state.CurrentDay >= 1);
        Assert.True(state.BerthCount >= 3); // SeedHelper seeds 3 docks
        Assert.True(state.ShipCount >= 0);
        Assert.True(state.ActiveAssignmentCount >= 0);
        Assert.False(string.IsNullOrEmpty(state.Environment));
        Assert.True(state.ServerTimeUtc != default);
    }

    [Fact]
    public async Task System_AdvanceDay_ShouldReturnOk()
    {
        var response = await _client.PostAsync("/api/system/advance-day", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task System_AdvanceDay_ShouldIncrementDay()
    {
        // Get the current day first
        var beforeResponse = await _client.GetAsync("/api/system/state");
        var before = await beforeResponse.Content.ReadFromJsonAsync<SystemState>(_jsonOptions);
        Assert.NotNull(before);

        // Advance the day
        var advanceResponse = await _client.PostAsync("/api/system/advance-day", null);
        Assert.Equal(HttpStatusCode.OK, advanceResponse.StatusCode);

        var after = await advanceResponse.Content.ReadFromJsonAsync<SystemState>(_jsonOptions);
        Assert.NotNull(after);

        Assert.Equal(before.CurrentDay + 1, after.CurrentDay);
    }

    // ================================================================
    // TerminalController tests
    // ================================================================

    [Fact]
    public async Task Terminal_GetCurrentDay_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/terminal/day");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.True(body.TryGetProperty("currentDay", out var dayProp));
        Assert.True(dayProp.GetInt32() >= 1);
    }

    [Fact]
    public async Task Terminal_NextDay_ShouldReturnOk()
    {
        var response = await _client.PostAsync("/api/terminal/next-day", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        Assert.True(body.TryGetProperty("currentDay", out var dayProp));
        Assert.True(dayProp.GetInt32() >= 1);
    }

    [Fact]
    public async Task Terminal_NextDay_ShouldIncrementDay()
    {
        // Get current day
        var beforeResponse = await _client.GetAsync("/api/terminal/day");
        var beforeBody = await beforeResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var beforeDay = beforeBody.GetProperty("currentDay").GetInt32();

        // Advance via next-day
        var nextDayResponse = await _client.PostAsync("/api/terminal/next-day", null);
        Assert.Equal(HttpStatusCode.OK, nextDayResponse.StatusCode);

        var afterBody = await nextDayResponse.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
        var afterDay = afterBody.GetProperty("currentDay").GetInt32();

        Assert.Equal(beforeDay + 1, afterDay);
    }

    // ================================================================
    // BerthsController (api/docks) tests
    // ================================================================

    [Fact]
    public async Task Docks_GetAll_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/docks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Docks_GetAll_ShouldIncludeAssignments()
    {
        var response = await _client.GetAsync("/api/docks");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var docks = await response.Content.ReadFromJsonAsync<List<JsonElement>>(_jsonOptions);
        Assert.NotNull(docks);
        Assert.True(docks.Count >= 3); // SeedHelper seeds 3 docks

        // Each dock should have the expected properties
        foreach (var dock in docks)
        {
            Assert.True(dock.TryGetProperty("id", out _));
            Assert.True(dock.TryGetProperty("name", out _));
            Assert.True(dock.TryGetProperty("size", out _));
            Assert.True(dock.TryGetProperty("assignments", out var assignments));
            Assert.Equal(JsonValueKind.Array, assignments.ValueKind);
        }
    }
}
