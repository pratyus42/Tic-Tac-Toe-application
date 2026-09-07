using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Api.Contracts;

namespace TicTacToe.Tests;

public sealed class GamesControllerUndoTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public GamesControllerUndoTests(WebApplicationFactory<Program> factory) => client = factory.CreateClient();

    [Fact]
    public async Task CompletedGameUndoReturnsConflictWithCode()
    {
        var response = await client.PostAsJsonAsync("/api/games", new { mode = "TwoPlayer" });
        var game = (await response.Content.ReadFromJsonAsync<GameStateResponse>())!;
        foreach (var move in new[] { new { player = "X", row = 0, column = 0 }, new { player = "O", row = 1, column = 0 }, new { player = "X", row = 0, column = 1 }, new { player = "O", row = 1, column = 1 }, new { player = "X", row = 0, column = 2 } })
            await client.PostAsJsonAsync($"/api/games/{game.GameId}/moves", move);

        var undo = await client.PostAsJsonAsync($"/api/games/{game.GameId}/undo", new { });
        var error = await undo.Content.ReadFromJsonAsync<ApiErrorResponse>();

        Assert.Equal(HttpStatusCode.Conflict, undo.StatusCode);
        Assert.Equal("UndoUnavailable", error!.Code);
    }
}
