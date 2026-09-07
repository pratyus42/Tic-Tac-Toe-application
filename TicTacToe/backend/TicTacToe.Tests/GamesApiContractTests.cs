using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Api.Contracts;

namespace TicTacToe.Tests;

public sealed class GamesApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public GamesApiContractTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateAndGetReturnCompleteCamelCaseState()
    {
        var create = await client.PostAsJsonAsync("/api/games", new { mode = "TwoPlayer" });
        var created = await create.Content.ReadFromJsonAsync<GameStateResponse>();

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("X", created.CurrentPlayer);
        Assert.Equal("InProgress", created.Status);
        Assert.Equal(3, created.Board.Length);
        Assert.Equal(3, created.Board[0].Length);
        Assert.Contains("gameId", await create.Content.ReadAsStringAsync());

        var get = await client.GetFromJsonAsync<GameStateResponse>($"/api/games/{created.GameId}");
        Assert.Equal(created.GameId, get!.GameId);
        Assert.False(get.CanUndo);
    }

    [Fact]
    public async Task InvalidMoveReturnsErrorAndLeavesStateUnchanged()
    {
        var created = await CreateGame();
        var valid = await client.PostAsJsonAsync($"/api/games/{created.GameId}/moves", new { player = "X", row = 0, column = 0 });
        Assert.Equal(HttpStatusCode.OK, valid.StatusCode);
        var before = await client.GetFromJsonAsync<GameStateResponse>($"/api/games/{created.GameId}");

        var invalid = await client.PostAsJsonAsync($"/api/games/{created.GameId}/moves", new { player = "O", row = 0, column = 0 });
        var error = await invalid.Content.ReadFromJsonAsync<ApiErrorResponse>();
        var after = await client.GetFromJsonAsync<GameStateResponse>($"/api/games/{created.GameId}");

        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("InvalidMove", error!.Code);
        Assert.Equal(JsonSerializer.Serialize(before), JsonSerializer.Serialize(after));
    }

    [Fact]
    public async Task UnknownGameReturnsNotFoundContractError()
    {
        var response = await client.GetAsync($"/api/games/{Guid.NewGuid()}");
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("GameNotFound", error!.Code);
    }

    [Fact]
    public async Task ComputerMoveReturnsBothAuthoritativeMoves()
    {
        var created = await CreateGame("Computer");
        var response = await client.PostAsJsonAsync($"/api/games/{created.GameId}/moves", new { player = "X", row = 0, column = 0 });
        var state = await response.Content.ReadFromJsonAsync<GameStateResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(2, state!.MoveHistory.Length);
        Assert.Equal("X", state.MoveHistory[0].Player);
        Assert.Equal("O", state.MoveHistory[1].Player);
        Assert.Equal("X", state.CurrentPlayer);
    }

    private async Task<GameStateResponse> CreateGame(string mode = "TwoPlayer")
    {
        var response = await client.PostAsJsonAsync("/api/games", new { mode });
        return (await response.Content.ReadFromJsonAsync<GameStateResponse>())!;
    }
}
