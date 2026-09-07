using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Api.Contracts;

namespace TicTacToe.Tests;

public sealed class GamesControllerResetTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public GamesControllerResetTests(WebApplicationFactory<Program> factory) => client = factory.CreateClient();

    [Fact]
    public async Task ResetReturnsFreshStateWithSameGameId()
    {
        var created = await CreateGame();
        await client.PostAsJsonAsync($"/api/games/{created.GameId}/moves", new { player = "X", row = 0, column = 0 });

        var response = await client.PostAsJsonAsync($"/api/games/{created.GameId}/reset", new { });
        var reset = await response.Content.ReadFromJsonAsync<GameStateResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(created.GameId, reset!.GameId);
        Assert.Empty(reset.MoveHistory);
        Assert.Equal("X", reset.CurrentPlayer);
        Assert.False(reset.CanUndo);
        Assert.All(reset.Board, row => Assert.All(row, cell => Assert.Null(cell)));
    }

    [Fact]
    public async Task ResetUnknownGameReturnsNotFound()
    {
        var response = await client.PostAsJsonAsync($"/api/games/{Guid.NewGuid()}/reset", new { });
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("GameNotFound", error!.Code);
    }

    private async Task<GameStateResponse> CreateGame()
    {
        var response = await client.PostAsJsonAsync("/api/games", new { mode = "TwoPlayer" });
        return (await response.Content.ReadFromJsonAsync<GameStateResponse>())!;
    }
}
