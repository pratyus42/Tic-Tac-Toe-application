using System.Collections.Concurrent;
using TicTacToe.Api.Domain;

namespace TicTacToe.Api.Services;

public sealed class InMemoryGameStore
{
    private readonly ConcurrentDictionary<Guid, Game> games = new();
    private readonly object sync = new();
    private Scoreboard scoreboard = new();
    public object Sync => sync;
    public bool TryGet(Guid id, out Game? game) => games.TryGetValue(id, out game);
    public void Save(Game game) => games[game.GameId] = game;
    public Scoreboard GetScoreboard() => scoreboard;
    public Scoreboard ResetScoreboard() { lock (sync) { scoreboard = new Scoreboard(); return scoreboard; } }
}
