# Feature Specification: Local Tic Tac Toe Application

**Feature Branch**: `001-tic-tac-toe-application`

**Created**: 2026-09-07

**Status**: Draft

**Input**: Authoritative requirements: `requirement/Round 2 - Problem Statement.docx`

## Scope and Goals

Build a browser-based Tic Tac Toe application for a technical assessment. The Angular + TypeScript frontend runs locally and communicates with a locally running .NET Web API through REST APIs. The backend is the source of truth for the game session, rules, move history, and session scoreboard.

The solution MUST favor simple, readable, maintainable code and MUST avoid cloud infrastructure, external services, authentication, or unnecessary persistence. In-memory backend storage is sufficient.

## User Scenarios & Testing

### User Story 1 - Play a Two Player Game (Priority: P1)

As two players, we want to play a valid game on a 3 x 3 board so that the application identifies turns, wins, and draws correctly.

**Why this priority**: This is the core assessment behavior and the minimum usable product.

**Independent Test**: Create a two-player game, make valid and invalid moves, and complete games by a row, column, diagonal, or full board.

**Acceptance Scenarios**:

1. **Given** a new two-player game, **When** the board is displayed, **Then** it contains nine empty clickable cells, the mode is shown, and X is identified as the current player.
2. **Given** an empty cell and the displayed current player, **When** the player selects the cell, **Then** the backend records the move, the cell displays X or O, the move history grows by one, and the turn changes.
3. **Given** an occupied cell, an out-of-range position, a wrong player, or a completed game, **When** a move is submitted, **Then** the backend rejects it, the board and move history remain unchanged, and the current turn does not change.
4. **Given** a player completes a row, column, or diagonal, **When** the move is accepted, **Then** the game is Won, the winner and winning cells are returned and shown, further moves are prevented, and the scoreboard increments once.
5. **Given** all nine cells are filled without a winner, **When** the final move is accepted, **Then** the game is Draw, a draw message is shown, further moves are prevented, and the scoreboard increments once.

### User Story 2 - Reset and Review a Game (Priority: P1)

As a player, we want to reset the current game and review its moves so that we can start another game without losing the session scoreboard.

**Independent Test**: Complete or partially play a game, inspect move history, reset the game, and verify the board and history reset while scoreboard totals remain unchanged.

**Acceptance Scenarios**:

1. **Given** a game with moves or a completed result, **When** Reset Game is selected, **Then** a fresh game is created with an empty board, empty history, status InProgress, no winner or winning cells, and X as current player.
2. **Given** a game with valid moves, **When** the move history is displayed, **Then** each entry shows its move number, player, and cell position in chronological order.
3. **Given** a completed game, **When** Reset Game is selected, **Then** the scoreboard is unchanged.

### User Story 3 - Undo Moves (Priority: P1)

As a player, we want to undo the latest action according to the selected mode so that we can correct a recent move.

**Independent Test**: Exercise undo in both modes before completion and verify board, turn, history, and status restoration.

**Acceptance Scenarios**:

1. **Given** a two-player game with at least one move and status InProgress, **When** Undo Last Move is selected, **Then** only the latest move is removed, the board and history are restored, and the removed player's turn is restored.
2. **Given** a computer-mode game where X and the computer O have both moved and status InProgress, **When** Undo Last Move is selected, **Then** the latest O move and the preceding X move are removed together and it is X's turn again.
3. **Given** a game with no moves, **When** the UI is displayed, **Then** Undo Last Move is disabled.
4. **Given** a Won or Draw game, **When** the UI is displayed, **Then** Undo Last Move is disabled because this specification selects Option A: completed-game undo is not allowed.
5. **Given** an incomplete game with an undoable move, **When** undo succeeds, **Then** the scoreboard is unchanged and the backend returns the recalculated InProgress state.

### User Story 4 - Play Against the Computer (Priority: P1)

As a human player, we want to play as X against a basic computer-controlled O so that the application supports a single-player mode.

**Independent Test**: Create computer mode, make human moves, and verify automatic valid O responses and the documented priority order.

**Acceptance Scenarios**:

1. **Given** computer mode, **When** a new game is created, **Then** the human is X, the computer is O, and X is the current player.
2. **Given** a valid human X move that does not complete the game, **When** the backend processes it, **Then** the computer makes one automatic valid O move and the returned state includes both moves.
3. **Given** O can win, **When** the computer selects a move, **Then** it takes the winning move.
4. **Given** O cannot win but X can win next, **When** the computer selects a move, **Then** it blocks X.
5. **Given** no winning or blocking move, **When** the center is available, **Then** the computer takes center; otherwise it chooses an available corner, then any available cell.
6. **Given** the human move completes the game, **When** it is processed, **Then** the computer does not make another move.

### User Story 5 - Review and Run the Assessment Solution (Priority: P2)

As a reviewer, we want clear setup, API, test, design, and AI workflow documentation so that we can run and discuss the submission locally.

**Independent Test**: Follow README instructions from a clean checkout to start both applications, run tests, review endpoints, and understand assumptions and limitations.

**Acceptance Scenarios**:

1. **Given** a clean local checkout with the documented prerequisites, **When** the README steps are followed, **Then** the backend and frontend start locally without cloud services.
2. **Given** the README, **When** a reviewer reads it, **Then** it includes overview, stack, features, run commands, API summary, test commands, AI tools/prompts summary, design decisions, assumptions, known limitations, and future improvements.

## Functional Requirements

### Game and Rules

- **FR-001**: The system MUST create a new game with a unique game ID, an empty 3 x 3 board, mode, status `InProgress`, no winner, no winning cells, empty move history, and current player X.
- **FR-002**: The system MUST support `TwoPlayer` and `Computer` modes. In Computer mode, the human is X and the computer is O.
- **FR-003**: The board MUST contain exactly nine cells. A valid move MUST place the current player's mark in one empty cell and lock that cell for the current game state.
- **FR-004**: Players MUST alternate turns after each valid move. Invalid moves MUST NOT change the current player or state.
- **FR-005**: The system MUST detect wins across every complete row, column, and diagonal, return the winner and all winning cell positions, and prevent further moves.
- **FR-006**: The system MUST detect a draw when all nine cells are occupied without a winner, return status `Draw`, and prevent further moves.
- **FR-007**: A completed game MUST update the session scoreboard exactly once: X wins, O wins, or draws.
- **FR-008**: Reset Game MUST clear the board, move history, winner, winning cells, and completion status; set current player to X; start a fresh game session; and leave the scoreboard unchanged.
- **FR-009**: Reset Scoreboard MUST clear X wins, O wins, and draws for the current backend session without changing the current game's board or history.

### Move History and Undo

- **FR-010**: The system MUST record every valid move with move number, player, and cell position, and return history in chronological order.
- **FR-011**: In TwoPlayer mode, Undo Last Move MUST remove exactly the latest valid move and restore the corresponding turn.
- **FR-012**: In Computer mode, Undo Last Move MUST remove the latest computer O move and the immediately preceding human X move as one action, restoring X's turn.
- **FR-013**: Undo MUST restore the board and accurately recalculate status, winner, winning cells, current player, and history.
- **FR-014**: Undo MUST be unavailable when there are no moves or when status is `Won` or `Draw` (Option A). The scoreboard remains final after completion.

### Computer Opponent

- **FR-015**: After a valid human move in Computer mode, the backend MUST make one automatic O move unless the human move completed the game.
- **FR-016**: The computer MUST select moves in this order: winning move for O; block an imminent X win; center; an available corner; any available cell.
- **FR-017**: The computer MUST never select an occupied or out-of-range cell and MUST never move after completion.

### Validation and State Ownership

- **FR-018**: The backend MUST own and return the authoritative game ID, board, current player, mode, status, winner, winning cells, move history, and scoreboard.
- **FR-019**: The backend MUST reject moves that are outside the board, target an occupied cell, occur after completion, or are submitted by the wrong player.
- **FR-020**: The frontend MUST render the latest backend state after every successful action and MUST not independently decide game rules or scoreboard results.
- **FR-021**: The frontend MUST show the board, current player, mode, winner/draw message, winning-cell highlights, move history, scoreboard, Reset Game, Undo Last Move, and Reset Scoreboard controls.
- **FR-022**: The UI MUST be usable on a laptop browser and MUST disable controls when the corresponding operation is unavailable.

## REST API Contract

The endpoint names below are the required contract for this specification. All endpoints are local HTTP REST endpoints and use JSON. The exact local ports MAY be chosen during planning and MUST be documented in the README.

### `POST /api/games`

Creates a new game.

Request:

```json
{ "mode": "TwoPlayer" }
```

`mode` MUST be `TwoPlayer` or `Computer`.

Response: `201 Created` with the complete Game State Response.

### `GET /api/games/{gameId}`

Returns the complete current game state for the specified game.

Response: `200 OK` with the complete Game State Response, or `404 Not Found` for an unknown game ID.

### `POST /api/games/{gameId}/moves`

Submits a player move. In Computer mode, a successful human move also triggers the computer move when appropriate.

Request:

```json
{ "player": "X", "row": 0, "column": 2 }
```

`player` MUST be `X` or `O`; `row` and `column` MUST be zero-based values from 0 through 2.

Response: `200 OK` with the complete updated state. Invalid moves MUST return `400 Bad Request` with a machine-readable error code and human-readable message; the state MUST remain unchanged.

### `POST /api/games/{gameId}/undo`

Undoes the latest action according to the game mode. It MUST return `409 Conflict` when no undo is available, including completed games under Option A.

Response: `200 OK` with the complete updated state, or `404 Not Found` for an unknown game ID.

### `POST /api/games/{gameId}/reset`

Resets the current game and returns the new complete game state.

Response: `200 OK` with the new game state, or `404 Not Found` for an unknown game ID.

### `GET /api/scoreboard`

Returns the session scoreboard:

```json
{ "xWins": 0, "oWins": 0, "draws": 0 }
```

Response: `200 OK`.

### `POST /api/scoreboard/reset`

Resets all session scoreboard totals and returns the zeroed scoreboard. It MUST NOT change the active game's state.

Response: `200 OK`.

### Game State Response

Every successful game action MUST return enough data for the frontend to render without reconstructing rules:

```json
{
  "gameId": "string",
  "board": [[null, null, null], [null, null, null], [null, null, null]],
  "currentPlayer": "X",
  "mode": "TwoPlayer",
  "status": "InProgress",
  "winner": null,
  "winningCells": [],
  "moveHistory": [],
  "scoreboard": { "xWins": 0, "oWins": 0, "draws": 0 },
  "canUndo": false
}
```

Allowed statuses are `InProgress`, `Won`, and `Draw`. A move-history item MUST include `moveNumber`, `player`, and cell position (`row` and `column`, or an equivalent documented cell index).

## UI Requirements

- The primary screen MUST present a clear 3 x 3 board and current game status.
- The selected mode MUST be visible and users MUST be able to start a game in either supported mode.
- Empty cells MUST be clickable only when a move is allowed; occupied cells and completed games MUST be visibly non-interactive.
- Winning cells MUST be visually highlighted and the winner or draw message MUST be visible.
- Move history and scoreboard MUST be visible without requiring developer tools.
- Reset Game, Undo Last Move, and Reset Scoreboard MUST be clearly labeled controls with disabled states matching backend state.
- The layout MUST remain comfortable to use on a laptop browser.
- The Angular client MUST call the REST APIs for game creation and all game actions, handle API errors without corrupting displayed state, and render the returned state.

## Testing Requirements

Automated tests MUST include core backend game logic and state transitions. At minimum, cover:

- valid move;
- invalid move and unchanged turn/state;
- turn switching;
- row, column, and diagonal wins;
- draw;
- reset game;
- undo in TwoPlayer mode;
- undo in Computer mode;
- scoreboard update and exactly-once completion update;
- computer move selection for winning, blocking, center, corner, and fallback behavior;
- no computer move after completion;
- move after game completion rejection.

Backend unit tests are preferred for rules and transitions. API/contract tests MUST verify the documented REST behavior and error responses. Frontend tests SHOULD cover board/status rendering, disabled controls, winning-cell presentation, and API integration points. Tests MUST be runnable locally using commands documented in the README.

## README Requirements

The repository README MUST include:

- project overview;
- technology stack;
- implemented features;
- backend local setup and run instructions;
- frontend local setup and run instructions;
- API endpoint summary and example usage;
- test commands;
- AI tools and prompt/workflow summary, including what was generated, manually changed, reviewed, assumptions, and trade-offs;
- design decisions, including Option A for completed-game Undo;
- clarifications and assumptions;
- known limitations;
- future improvements.

## Acceptance Criteria

The feature is complete only when all of the following are true:

- The Angular application runs locally.
- The .NET API runs locally.
- The frontend communicates with the backend through REST APIs.
- A new game can be created in both supported modes.
- Two Player Mode and Computer Mode work correctly.
- Turns alternate correctly and invalid moves are handled without state changes.
- Row, column, diagonal, and draw detection work.
- Winning cells are highlighted.
- Move history is shown and remains accurate after reset and undo.
- Undo follows the selected mode and is disabled after a win or draw.
- Scoreboard and Reset Scoreboard work correctly.
- Reset Game preserves the scoreboard.
- Basic backend and API tests are included and pass.
- README explains how to run, test, review, and understand the solution.
- The candidate can explain the implementation, AI-assisted workflow, assumptions, and trade-offs during panel review.

## Assumptions and Out of Scope

- The application is a local technical-assessment solution, not a multi-user production service.
- In-memory storage is acceptable; persistence across backend restarts is out of scope.
- Authentication, authorization, cloud deployment, external services, real-time networking, and advanced AI strategy are out of scope.
- A game session is represented by a backend game ID; session scoreboard state is maintained by the running backend instance.
- The backend may expose standard development CORS configuration so the local Angular origin can call the local API; production hosting configuration is out of scope.
- The exact Angular and .NET supported versions, local ports, project folders, and test framework are implementation-plan decisions and MUST be documented before implementation.

## Success Criteria

- **SC-001**: A reviewer can start both local applications by following the README without cloud infrastructure or undocumented services.
- **SC-002**: All acceptance criteria pass in automated tests or an explicitly documented manual verification step.
- **SC-003**: Every valid action produces a backend-authoritative state that the frontend can render without applying separate game-rule decisions.
- **SC-004**: A reviewer can identify the API contract, run the tests, and understand the design decisions and known limitations from the README.