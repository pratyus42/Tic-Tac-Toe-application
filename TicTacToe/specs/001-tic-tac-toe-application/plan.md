# Implementation Plan: Local Tic Tac Toe Application

**Branch**: `001-tic-tac-toe-application` | **Date**: 2026-09-07 | **Spec**: [spec.md](spec.md)

**Input**: Feature specification from `/specs/001-tic-tac-toe-application/spec.md`, based on `requirement/Round 2 - Problem Statement.docx`.

## Summary

Build a local browser game with an Angular + TypeScript frontend and a .NET 8 Web API backend. The backend owns the authoritative game and session scoreboard state in memory. A small domain service will apply moves, detect wins and draws, maintain history, perform mode-specific undo, and select computer moves. REST controllers expose complete state responses so the Angular client can render state rather than duplicate game rules.

The implementation will use two independently runnable projects, simple in-memory collections, standard framework testing tools, and no database, authentication, cloud infrastructure, state-management library, or additional backend framework.

## Technical Context

**Language/Version**: C# / .NET 8; TypeScript with the Angular version selected by the installed Angular CLI; modern browser APIs only

**Primary Dependencies**: ASP.NET Core Web API, built-in dependency injection and JSON serialization, Angular, Angular HttpClient, Angular standalone components, xUnit, and the standard Angular test runner configured by the CLI

**Storage**: In-memory backend state; a singleton session store containing games and one scoreboard. No persistence across backend restarts.

**Testing**: .NET unit tests for domain logic, .NET API integration/contract tests using `WebApplicationFactory`, and focused Angular component/service tests

**Target Platform**: Local Windows development with a laptop browser; local ASP.NET Core API and Angular development server

**Project Type**: Web application with separate frontend and backend projects

**Performance Goals**: Immediate interaction for a single local user; all game operations are constant-size 3 x 3 board operations and should complete in one local request

**Constraints**: No cloud services or external runtime dependencies; backend is the source of truth; API calls use JSON over HTTP; development CORS permits only the configured local Angular origin; completed-game Undo is disabled under Option A

**Scale/Scope**: One local assessment session, a small number of in-memory games, one active scoreboard, one screen, and two game modes

## Constitution Check

*GATE: Pass before implementation and re-check after design.*

- **Separation of concerns**: Pass. Angular owns presentation and HTTP orchestration; .NET owns game rules, state, validation, and scoreboard.
- **Correct gameplay**: Pass. The domain service explicitly covers moves, history, reset, undo, win/draw detection, scoreboard updates, and the computer opponent.
- **Local-first operation**: Pass. The solution uses local Angular and ASP.NET Core processes with in-memory state.
- **REST boundary**: Pass. All game actions cross the documented HTTP/JSON API; no shared runtime code or direct state access is planned.
- **Testability**: Pass. Domain services are isolated behind interfaces and API behavior is exercised through HTTP-level tests.
- **Simplicity**: Pass. No repository abstraction, database, event bus, state-management framework, authentication, or generalized rules engine is needed for a 3 x 3 assessment game.

## Architecture

### Backend Responsibilities

1. Accept game creation, move, undo, reset, and scoreboard requests.
2. Locate the requested game by ID and validate request shape and game rules.
3. Apply state transitions through one game service so controllers do not contain game logic.
4. Update the session scoreboard once when a game first enters `Won` or `Draw`.
5. Return a complete state snapshot after every successful action.

### Frontend Responsibilities

1. Present the board, selected mode, current player, status, history, scoreboard, and controls.
2. Send user actions to the API and replace the displayed state with the returned snapshot.
3. Disable cells and controls based on the returned state and surface API errors without mutating state optimistically.
4. Keep only view state such as loading, error message, and selected mode; do not calculate winners, turns, scoreboard values, or computer moves.

## Project Structure

```text
TicTacToe/
├── backend/
│   ├── TicTacToe.Api/
│   │   ├── Controllers/
│   │   │   ├── GamesController.cs
│   │   │   └── ScoreboardController.cs
│   │   ├── Domain/
│   │   │   ├── Game.cs
│   │   │   ├── GameEnums.cs
│   │   │   ├── Move.cs
│   │   │   └── Scoreboard.cs
│   │   ├── Services/
│   │   │   ├── IGameService.cs
│   │   │   ├── GameService.cs
│   │   │   ├── IComputerMoveSelector.cs
│   │   │   ├── ComputerMoveSelector.cs
│   │   │   └── InMemoryGameStore.cs
│   │   ├── Contracts/
│   │   │   ├── CreateGameRequest.cs
│   │   │   ├── MakeMoveRequest.cs
│   │   │   ├── GameStateResponse.cs
│   │   │   └── ApiErrorResponse.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   └── TicTacToe.Tests/
│       ├── Domain/
│       ├── Services/
│       ├── Controllers/
│       └── TicTacToe.Tests.csproj
├── frontend/
│   ├── src/app/
│   │   ├── models/
│   │   ├── services/
│   │   ├── components/
│   │   │   ├── game-board/
│   │   │   ├── move-history/
│   │   │   └── scoreboard/
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   └── app.component.css
│   ├── src/environments/
│   └── package.json
├── README.md
└── specs/001-tic-tac-toe-application/
```

**Structure Decision**: Use separate `backend/` and `frontend/` projects because the constitution requires a clear Angular/.NET boundary and the specification requires independent local startup. Keep the frontend feature small and component-based, and keep backend domain logic independent from controllers and HTTP types.

## Backend Design

### Domain Models

- **Game**: `GameId`, 3 x 3 board, `CurrentPlayer`, `Mode`, `Status`, nullable `Winner`, winning cell list, ordered move history, and a completion-counted flag or equivalent invariant.
- **Move**: sequential move number, player, row, and column.
- **Scoreboard**: `XWins`, `OWins`, and `Draws`.
- **Enums**: `Player` (`X`, `O`), `GameMode` (`TwoPlayer`, `Computer`), and `GameStatus` (`InProgress`, `Won`, `Draw`).
- **Cell position**: a small row/column value object or contract representation constrained to zero-based values 0 through 2.

The domain model should expose enough state for the service to validate and transition a game, but the API should return DTOs rather than domain objects. The board may be represented internally as a two-dimensional array or a nine-cell collection; choose the simplest representation that preserves clear row/column checks.

### In-Memory State

`InMemoryGameStore` is registered as a singleton and owns a thread-safe dictionary of game IDs to games plus the session scoreboard. It should expose focused operations needed by `GameService`, rather than leaking a mutable dictionary to controllers. A lock around compound read/modify/write operations is sufficient for local correctness and avoids inconsistent scoreboard updates if requests overlap.

No game deletion endpoint is needed. Reset Game creates a fresh game state for the requested game ID or replaces the active state while preserving the shared scoreboard; the chosen ID behavior must be documented in the API implementation and README. The simplest approach is to reset the existing game ID to a new empty game state and return it.

### Game Service and State Transitions

`GameService` is the only application service allowed to mutate games or scoreboard state.

**Create**:

1. Validate the mode.
2. Generate a unique ID.
3. Create an empty board with current player X and status `InProgress`.
4. Save the game and return a complete response including the current scoreboard.

**Make move**:

1. Resolve the game or return not found.
2. Validate player, row/column bounds, status, current turn, and cell emptiness before mutation.
3. Append one move and place the mark.
4. Check all rows, columns, and both diagonals for a winner.
5. If there is a winner, set status, winner, winning cells, and increment the matching scoreboard exactly once.
6. Otherwise, if all cells are filled, set `Draw` and increment draws exactly once.
7. Otherwise switch the current player.
8. In Computer mode, if the game remains `InProgress` and the next turn is O, select and apply exactly one computer move through the same validated move path, then evaluate completion and turn state again.
9. Return a complete state response.

**Reset**:

1. Resolve the game.
2. Replace its board, history, status, winner, winning cells, and current player with a new initial state.
3. Preserve the shared scoreboard.
4. Return the complete reset state.

**Undo**:

1. Reject if the game is missing, completed, or has no undoable moves.
2. In TwoPlayer mode remove one latest move.
3. In Computer mode remove the latest O move and the immediately preceding X move; if only a human move is present, remove that human move and restore X's turn.
4. Rebuild the board from the remaining history rather than relying on reverse mutation.
5. Recalculate status, winner, winning cells, current player, and `canUndo`.
6. Never change the scoreboard because Option A makes completed games non-undoable and incomplete games have not contributed to it.

**Scoreboard reset**:

1. Replace all session totals with zero.
2. Leave every game unchanged.
3. Return the zeroed scoreboard.

### Win and Draw Detection

Use a fixed list of eight winning lines: three rows, three columns, and two diagonals. After every accepted move, inspect only those lines. Return the exact row/column positions for the first completed line; on a valid Tic Tac Toe board only one winner should be possible after the latest move. Check draw only after win detection.

### Computer Move Selection

`ComputerMoveSelector` receives a board and returns a legal O position without mutating the game. It evaluates candidates in this deterministic order:

1. Try each empty cell as O; choose the first candidate that produces an O win.
2. Try each empty cell as X; choose the first candidate that prevents an X win.
3. Choose center `(1,1)` if empty.
4. Choose the first available corner in a documented stable order, such as top-left, top-right, bottom-left, bottom-right.
5. Choose the first remaining empty cell in row-major order.

The service then applies the selected position through normal move validation, so the computer cannot bypass bounds, occupancy, turn, completion, history, or scoreboard rules.

## API Design

Implement the endpoints in the specification exactly:

| Method | Route | Responsibility | Success | Main failures |
|---|---|---|---|---|
| POST | `/api/games` | Create a game in `TwoPlayer` or `Computer` mode | `201` + complete state | `400` invalid mode |
| GET | `/api/games/{gameId}` | Read current game state | `200` + complete state | `404` unknown game |
| POST | `/api/games/{gameId}/moves` | Validate and apply human/player move, plus computer response if applicable | `200` + complete state | `400` invalid move, `404` unknown game |
| POST | `/api/games/{gameId}/undo` | Apply mode-specific undo | `200` + complete state | `404` unknown game, `409` unavailable undo |
| POST | `/api/games/{gameId}/reset` | Reset one game while preserving scoreboard | `200` + complete state | `404` unknown game |
| GET | `/api/scoreboard` | Read session scoreboard | `200` + scoreboard | none expected |
| POST | `/api/scoreboard/reset` | Clear session scoreboard only | `200` + zeroed scoreboard | none expected |

Use request DTO validation for required fields and ranges, and domain validation for rules. Return a consistent `ApiErrorResponse` with an error code such as `InvalidMove`, `InvalidMode`, `GameNotFound`, or `UndoUnavailable`, plus a readable message. Do not return partially mutated state on errors. Controllers should translate service results/exceptions to HTTP status codes and leave game decisions in the service.

Every successful game response includes `gameId`, 3 x 3 `board`, `currentPlayer`, `mode`, `status`, nullable `winner`, `winningCells`, chronological `moveHistory`, `scoreboard`, and `canUndo`. Configure JSON naming consistently, preferably camelCase to match the Angular models.

Enable development CORS for the configured Angular localhost origin only. Keep ports and the API base URL in one frontend environment configuration and document both in the README.

## Frontend Design

### Models

Define TypeScript interfaces/enums matching the API DTOs: `Player`, `GameMode`, `GameStatus`, `CellPosition`, `Move`, `Scoreboard`, `GameState`, `CreateGameRequest`, `MakeMoveRequest`, and `ApiError`. Keep these as transport/view models; do not recreate backend rule logic in the client.

### Services

- **GameApiService**: Wrap `HttpClient` calls for create, get, move, undo, and reset game operations. Return typed observables and map HTTP errors into a small UI-friendly error shape.
- **ScoreboardApiService**: Wrap get/reset scoreboard calls, or keep these two calls in `GameApiService` if that keeps the feature smaller. Prefer one service if no independent reuse emerges.
- **GameFacade/service state holder**: Maintain the current `GameState`, loading flag, and error message; expose methods used by the component. It must replace state only after successful responses and clear or update error state deliberately.

Avoid a global state-management package. A single feature service with observable state or simple component state is sufficient for one screen.

### Components and Interaction

- **AppComponent**: Coordinates game creation, mode selection, reset, undo, scoreboard reset, loading, and error display.
- **GameBoardComponent**: Renders nine cells from `GameState.board`, emits a selected row/column, disables occupied cells and input when status is not `InProgress` or it is not a permitted player turn, and highlights positions in `winningCells`.
- **MoveHistoryComponent**: Renders move number, player, and row/column position in chronological order.
- **ScoreboardComponent**: Renders X wins, O wins, and draws and exposes Reset Scoreboard.

The mode selector should create a fresh game when the user selects a different mode, with a clear loading state. Buttons should use `canUndo` and current request state to prevent duplicate actions. The UI should display winner/draw/current-turn status from the API response, not locally inferred state.

## Error Handling and Validation

- Validate malformed JSON, missing fields, invalid enum values, and out-of-range coordinates at the API boundary.
- Validate unknown game IDs as `404 Not Found`.
- Validate occupied cells, wrong player, completed games, and invalid turns as `400 Bad Request` with unchanged backend state.
- Return `409 Conflict` for unavailable undo, including no moves and completed games under Option A.
- The Angular client should preserve the last valid state when a request fails, show a concise error message, and re-enable controls after the request completes.
- Disable controls optimistically only for the duration of an in-flight request; authoritative availability comes from `canUndo`, `status`, and the returned state.
- Configure a centralized ASP.NET Core exception/error response path only as needed for unexpected failures; do not add a broad logging or observability framework for this assessment.

## Testing Strategy

### Backend Unit Tests

Test the domain/service in isolation with an in-memory store and deterministic computer selector. Cover:

- initial state and both modes;
- valid move and alternating turns;
- occupied, out-of-range, wrong-player, and completed-game rejection with unchanged state;
- each row, column, and diagonal win;
- draw detection and winning-cell positions;
- exactly-once scoreboard updates;
- reset game preserving scoreboard;
- scoreboard reset preserving game state;
- one-move TwoPlayer undo;
- paired Computer undo and recalculation;
- undo disabled for no moves, win, and draw;
- computer winning, blocking, center, corner, and fallback choices;
- no computer response after a completed human move.

### Backend API/Contract Tests

Use `WebApplicationFactory` or the minimal equivalent to call each route over HTTP. Verify status codes, JSON shape/casing, complete response snapshots, error payloads, and that failed moves do not mutate state. Keep the store isolated per test so scoreboard and games do not leak between tests.

### Frontend Tests

Use Angular’s standard test tooling to cover:

- board renders nine cells and marks returned X/O values;
- current player, winner/draw message, and winning-cell classes render from state;
- occupied/completed cells and unavailable undo are disabled;
- move history and scoreboard display returned values;
- API service sends the correct routes and payloads;
- successful actions replace state, while API failures preserve state and show an error.

### Manual Verification

The README should include a short browser checklist for starting each mode, completing row/column/diagonal wins and a draw, invalid move behavior, both undo behaviors, resets, scoreboard behavior, and winning-cell highlighting.

## README and Documentation Plan

Create a root `README.md` after the project structure is established. Include:

1. Overview and assessment scope.
2. Prerequisites, chosen .NET/Node/Angular versions, and local ports.
3. Backend install/build/run commands.
4. Frontend install/build/run commands.
5. How to run backend, API, and frontend tests.
6. Feature summary and two game modes.
7. API endpoint table with sample requests and response fields.
8. Architecture and backend-state ownership explanation.
9. Option A completed-game Undo decision.
10. In-memory storage, CORS, and other assumptions.
11. AI tools/prompts used, generated material, manual changes, review points, and trade-offs.
12. Known limitations and future improvements.
13. Manual review checklist.

## Implementation Sequence for `/speckit-tasks`

1. **Setup**: Create the .NET solution/projects, Angular application, test projects, local configuration, and root README outline.
2. **Backend foundation**: Add enums, domain models, DTOs, in-memory store, dependency injection, CORS, and consistent error response plumbing.
3. **Core game rules**: Implement game creation, move validation, win/draw evaluation, history, reset, and scoreboard transitions in the service.
4. **Computer mode and undo**: Add deterministic selector, automatic O response, TwoPlayer undo, Computer paired undo, and Option A completion guard.
5. **REST API**: Add controllers and route-level validation, status mappings, and complete state responses.
6. **Backend tests**: Add service tests first, then HTTP contract/integration tests for all endpoints and error paths.
7. **Angular integration**: Add typed models, API/state service, environment URL, and error/loading handling.
8. **Angular UI**: Build board, mode selection, status, history, scoreboard, action controls, disabled states, and winning-cell styles.
9. **Frontend tests**: Cover rendering, interaction outputs, API calls, error preservation, and disabled controls.
10. **Documentation and review**: Complete README, run all tests, manually verify acceptance scenarios, and record known limitations.

## Complexity Tracking

No constitution violations require justification. The plan intentionally avoids a database, repository layer, mediator/CQRS pattern, global frontend state library, authentication, real-time transport, or advanced AI because none is required by the specification or assessment scope.
