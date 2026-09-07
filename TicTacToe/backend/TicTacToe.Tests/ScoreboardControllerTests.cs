using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TicTacToe.Api.Contracts;

namespace TicTacToe.Tests;

public sealed class ScoreboardControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public ScoreboardControllerTests(WebApplicationFactory<Program> factory) => client = factory.CreateClient();

    [Fact]
    public async Task ScoreboardResetReturnsZerosWithoutChangingActiveGame()
    {
        var initial = await client.GetFromJsonAsync<ScoreboardResponse>("/api/scoreboard");
        var gameResponse = await client.PostAsJsonAsync("/api/games", new { mode = "TwoPlayer" });
        var game = (await gameResponse.Content.ReadFromJsonAsync<GameStateResponse>())!;
        await client.PostAsJsonAsync($"/api/games/{game.GameId}/moves", new { player = "X", row = 0, column = 0 });

        var reset = await client.PostAsJsonAsync("/api/scoreboard/reset", new { });
        var score = await reset.Content.ReadFromJsonAsync<ScoreboardResponse>();
        var active = await client.GetFromJsonAsync<GameStateResponse>($"/api/games/{game.GameId}");

        Assert.Equal(new ScoreboardResponse(0, 0, 0), initial);
        Assert.Equal(HttpStatusCode.OK, reset.StatusCode);
        Assert.Equal(new ScoreboardResponse(0, 0, 0), score);
        Assert.Single(active!.MoveHistory);
        Assert.Equal("X", active.Board[0][0]);
    }
}
