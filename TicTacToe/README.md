# Local Tic Tac Toe

A small browser Tic Tac Toe assessment solution. The Angular standalone client renders backend snapshots; the .NET 8 Web API owns rules, history, game state, deterministic computer play, and the in-memory session scoreboard.

## Stack and setup

Prerequisites: .NET 8 SDK, Node.js 20+, and npm. The backend listens on `http://localhost:5050`; Angular runs on `http://localhost:4200`.

```powershell
cd backend
dotnet run --project TicTacToe.Api
# in another terminal
cd frontend
npm install
npm start
```

Tests and builds:

```powershell
dotnet build backend/TicTacToe.sln
dotnet test backend/TicTacToe.sln
cd frontend; npm run build; npm test
```

## Features

Two-player and deterministic computer mode, row/column/diagonal wins, draws, chronological move history, winning-cell highlights, reset game, mode-specific undo, scoreboard reset, invalid-move protection, and local CORS. Computer mode plays X for the human and O for the computer. O prioritizes a winning move, then blocks X, center, corners in top-left/top-right/bottom-left/bottom-right order, then the first row-major cell.

Undo follows Option A: it is unavailable after Won or Draw. In TwoPlayer mode it removes one move; in Computer mode it removes the latest O and preceding X pair. Reset game keeps the game ID but replaces its state and preserves the scoreboard.

## API and Swagger

When the backend runs in the Development environment, Swagger UI is available at [http://localhost:5050/swagger](http://localhost:5050/swagger). It lists every game and scoreboard endpoint, JSON request/response schema, route parameter, validation error, and documented HTTP status code. Use `Try it out` in Swagger UI to create a game, copy its `gameId`, and test the remaining calls locally.

- `POST /api/games` body `{ "mode": "TwoPlayer" | "Computer" }`
- `GET /api/games/{gameId}`
- `POST /api/games/{gameId}/moves` body `{ "player": "X", "row": 0, "column": 0 }`
- `POST /api/games/{gameId}/undo`
- `POST /api/games/{gameId}/reset`
- `GET /api/scoreboard`
- `POST /api/scoreboard/reset`

Successful game responses include `gameId`, a 3x3 `board`, `currentPlayer`, `mode`, `status`, `winner`, `winningCells`, `moveHistory`, `scoreboard`, and `canUndo`. Invalid moves return `400` with `{ code, message }`; unknown games return `404`; unavailable undo returns `409` with `UndoUnavailable`.

## Design and assumptions

The singleton `InMemoryGameStore` is intentionally session-scoped and is cleared when the API restarts. A lock protects compound game and scoreboard mutations. The frontend never infers winners, turns, computer moves, or scores; failed requests leave the last valid state visible. CORS allows only the documented Angular origin. There is no persistence, authentication, multi-user isolation, or production deployment configuration.

## Known limitations and future improvements

The application is local-only, stores one in-memory session, and has no authentication, persistence, multi-user isolation, or production deployment configuration. Future improvements could add durable storage, player sessions, richer computer strategy, and production hosting configuration.

## AI workflow summary

AI assistance was used to turn the approved specification into a narrow domain service, DTO boundary, REST controllers, Angular components, and focused tests. Manual review concentrated on exact endpoint names, unchanged state on invalid moves, exactly-once completion scoring, computer priority order, Option A undo, and CORS. Trade-offs favor readable fixed-size algorithms and no external state-management or persistence dependencies.

## Verification checklist

- [ ] Start both local processes and create each mode.
- [ ] Verify valid turns, occupied/out-of-range/wrong-player errors, and history.
- [ ] Verify all rows, columns, diagonals, draw, highlights, and post-completion lock.
- [ ] Verify computer winning/blocking/center/corner/fallback behavior.
- [ ] Verify both undo behaviors and completed-game disablement.
- [ ] Verify game reset preserves scoreboard and scoreboard reset preserves game.
