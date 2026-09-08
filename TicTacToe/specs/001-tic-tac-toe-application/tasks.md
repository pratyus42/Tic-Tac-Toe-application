---

description: "Implementation tasks for the local Tic Tac Toe application"

---

# Tasks: Local Tic Tac Toe Application

**Input**: Design documents from `/specs/001-tic-tac-toe-application/`

**Prerequisites**: [spec.md](spec.md), [plan.md](plan.md)

**Tests**: Required by the specification. Backend unit tests, API contract tests, and focused Angular tests are included below.

**Organization**: Tasks are grouped by shared foundation and prioritized user story. Each task includes an exact target path and uses `[P]` only when it can be performed independently of other active tasks.

## Path Conventions

- Backend API: `backend/TicTacToe.Api/`
- Backend tests: `backend/TicTacToe.Tests/`
- Frontend: `frontend/src/`
- Frontend tests: colocated with Angular components/services
- Documentation: `README.md` and `specs/001-tic-tac-toe-application/`

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create the two local projects and the testable development baseline.

- [x] T001 Create the .NET solution at `backend/TicTacToe.sln` with `backend/TicTacToe.Api/TicTacToe.Api.csproj` and `backend/TicTacToe.Tests/TicTacToe.Tests.csproj` targeting .NET 8.
- [x] T002 [P] Scaffold the Angular + TypeScript application under `frontend/` using standalone components and the standard Angular CLI test configuration.
- [x] T003 [P] Add backend test dependencies, including xUnit, test SDK, and ASP.NET Core test hosting support, to `backend/TicTacToe.Tests/TicTacToe.Tests.csproj`.
- [x] T004 [P] Configure local backend launch settings and the Angular development environment/API base URL in `backend/TicTacToe.Api/Properties/launchSettings.json` and `frontend/src/environments/environment.ts`.
- [x] T005 [P] Add the root `README.md` outline with prerequisites, local project commands, API documentation, testing, AI workflow, assumptions, and limitations.

## Phase 2: Foundational Backend and API Infrastructure

**Purpose**: Establish shared types, state ownership, serialization, and HTTP plumbing before story implementation.

**CRITICAL**: No user story implementation should begin until this phase is complete.

- [x] T006 Create backend game enums and value types for `Player`, `GameMode`, `GameStatus`, and cell positions in `backend/TicTacToe.Api/Domain/GameEnums.cs`.
- [x] T007 Create domain models for `Game`, `Move`, and `Scoreboard` in `backend/TicTacToe.Api/Domain/Game.cs`, `backend/TicTacToe.Api/Domain/Move.cs`, and `backend/TicTacToe.Api/Domain/Scoreboard.cs`, including the 3 x 3 board and completion-counting invariant.
- [x] T008 Create API request/response DTOs for game creation, moves, complete game state, scoreboard, and consistent errors in `backend/TicTacToe.Api/Contracts/CreateGameRequest.cs`, `MakeMoveRequest.cs`, `GameStateResponse.cs`, and `ApiErrorResponse.cs`.
- [x] T009 Implement the singleton in-memory game/session store in `backend/TicTacToe.Api/Services/InMemoryGameStore.cs`, including game lookup, game replacement, scoreboard access/reset, and synchronization for compound mutations.
- [x] T010 Define service contracts in `backend/TicTacToe.Api/Services/IGameService.cs` and `IComputerMoveSelector.cs` so game rules and computer selection can be tested without controllers.
- [x] T011 Configure ASP.NET Core JSON camelCase serialization, dependency injection, local-only development CORS, controller routing, and consistent unexpected-error handling in `backend/TicTacToe.Api/Program.cs`.
**Checkpoint**: Projects build, tests can instantiate the service with an in-memory store, and the API can start locally with the documented Angular origin allowed.

## Phase 3: User Story 1 - Play a Two Player Game (Priority: P1) MVP

**Goal**: Deliver a correct backend-authoritative two-player game with valid moves, turn switching, win/draw handling, and API access.

**Independent Test**: Create a `TwoPlayer` game, make legal and illegal moves, complete row/column/diagonal wins and a draw, and verify complete state responses.

### Tests for User Story 1
- [x] T015 [P] [US1] Add API contract tests for `POST /api/games`, `GET /api/games/{gameId}`, and `POST /api/games/{gameId}/moves`, including status codes, camelCase response fields, validation errors, and unchanged state on failure in `backend/TicTacToe.Tests/GamesApiContractTests.cs`.

### Implementation for User Story 1

- [x] T016 [US1] Implement game creation and complete state mapping in `backend/TicTacToe.Api/Services/GameService.cs`, generating a unique ID and initializing an empty board with X as current player.
- [x] T017 [US1] Implement move validation and application in `backend/TicTacToe.Api/Services/GameService.cs`, rejecting invalid coordinates, occupied cells, wrong player, and completed games before mutation.
- [x] T018 [US1] Implement fixed eight-line row/column/diagonal win detection, winning-cell output, draw detection, turn switching, and exactly-once scoreboard completion updates in `backend/TicTacToe.Api/Services/GameService.cs`.
- [x] T019 [US1] Implement `GamesController` routes for create, get, and move in `backend/TicTacToe.Api/Controllers/GamesController.cs`, mapping not-found, invalid-request, and successful results to the documented HTTP responses.
- [x] T020 [US1] Implement initial scoreboard retrieval in `backend/TicTacToe.Api/Controllers/ScoreboardController.cs` with `GET /api/scoreboard`.

**Checkpoint**: The backend supports the core P1 game independently through REST, and all US1 tests pass.

## Phase 4: User Story 2 - Reset and Review a Game (Priority: P1)

**Goal**: Reset the active game without changing the session scoreboard and expose chronological move history.

**Independent Test**: Play and complete a game, inspect history, reset it, and verify a fresh game state with preserved scoreboard totals.

### Tests for User Story 2
- [x] T022 [P] [US2] Add API tests for `POST /api/games/{gameId}/reset`, unknown-game handling, and complete reset response shape in `backend/TicTacToe.Tests/Controllers/GamesControllerResetTests.cs`.
- [x] T023 [P] [US2] Add scoreboard tests for `GET /api/scoreboard` and active-game state preservation when scoreboard reset is implemented in `backend/TicTacToe.Tests/Controllers/ScoreboardControllerTests.cs`.

### Implementation for User Story 2

- [x] T024 [US2] Implement reset behavior in `backend/TicTacToe.Api/Services/GameService.cs`, replacing the game with a fresh state while preserving the shared scoreboard and returning `canUndo: false`.
- [x] T025 [US2] Implement `POST /api/games/{gameId}/reset` in `backend/TicTacToe.Api/Controllers/GamesController.cs`.
- [x] T026 [US2] Implement `POST /api/scoreboard/reset` in `backend/TicTacToe.Api/Controllers/ScoreboardController.cs` and the corresponding store/service operation without modifying any game.

**Checkpoint**: Reset Game, Reset Scoreboard, and chronological move history work independently without corrupting each other.

## Phase 5: User Story 3 - Undo Moves (Priority: P1)

**Goal**: Restore the previous valid state with mode-specific undo and Option A completed-game behavior.

**Independent Test**: Undo one move in TwoPlayer mode and an X/O pair in Computer mode, then verify board, history, turn, status, and `canUndo`.

### Implementation for User Story 3

- [x] T030 [US3] Implement history-based board reconstruction and recalculation of status, winner, winning cells, current player, and `canUndo` in `backend/TicTacToe.Api/Services/GameService.cs`.
- [x] T031 [US3] Implement TwoPlayer one-move undo and Computer paired undo in `backend/TicTacToe.Api/Services/GameService.cs`, explicitly rejecting completed games under Option A.
- [x] T032 [US3] Implement `POST /api/games/{gameId}/undo` and map unavailable undo to `409 Conflict` in `backend/TicTacToe.Api/Controllers/GamesController.cs`.

**Checkpoint**: Undo behavior matches the selected mode and cannot reverse a completed scoreboard result.

## Phase 6: User Story 4 - Play Against the Computer (Priority: P1)

**Goal**: Add deterministic, valid computer O moves while keeping all rules in the backend.

**Independent Test**: Create Computer mode, submit human X moves, and verify automatic O moves follow the specified priority and stop after completion.

### Tests for User Story 4

- [x] T033 [P] [US4] Add isolated selector tests for O winning move, X blocking move, center, first documented corner, and row-major fallback in `backend/TicTacToe.Tests/Services/ComputerMoveSelectorTests.cs`.
- [x] T035 [P] [US4] Extend API contract tests to verify a successful human move in Computer mode returns both moves and complete authoritative state in `backend/TicTacToe.Tests/GamesApiContractTests.cs`.

### Implementation for User Story 4

- [x] T036 [US4] Implement deterministic priority selection in `backend/TicTacToe.Api/Services/ComputerMoveSelector.cs`, using legal candidate simulation without mutating the game.
- [x] T037 [US4] Inject the selector and implement automatic O response through the normal validated move path in `backend/TicTacToe.Api/Services/GameService.cs`, including no response when the human move completes the game.
- [x] T038 [US4] Complete Computer mode validation and state mapping so mode, current player, move history, scoreboard, and `canUndo` are correct after both human and computer moves in `backend/TicTacToe.Api/Services/GameService.cs`.

**Checkpoint**: Both modes deliver complete backend-authoritative gameplay, and all P1 backend behavior tests pass.

## Phase 7: Frontend Integration and User Interface (P1)

**Purpose**: Provide the usable Angular browser experience against the REST API.

### Tests for Frontend Integration

- [x] T039 [P] Add typed HTTP service tests for routes and payloads for create, get, move, reset, undo, scoreboard get, and scoreboard reset in `frontend/src/app/services/game-api.service.spec.ts`.
- [x] T040 [P] Add state/facade service tests proving successful responses replace state and failed responses preserve the last valid state while exposing an error in `frontend/src/app/services/game-state.service.spec.ts`.
- [x] T041 [P] Add component tests for nine cells, returned marks, current-player/status text, winning-cell highlighting, move history, scoreboard values, disabled occupied/completed cells, disabled unavailable Undo, and action events in `frontend/src/app/components/game-board/game-board.component.spec.ts`, `move-history.component.spec.ts`, `scoreboard.component.spec.ts`, and `frontend/src/app/app.component.spec.ts`.

### Implementation for Frontend Integration

- [x] T042 [P] Define TypeScript transport models and enums matching the backend in `frontend/src/app/models/game.models.ts`.
- [x] T043 [US1] Implement typed REST calls and API error mapping in `frontend/src/app/services/game-api.service.ts`, using the environment API base URL.
- [x] T044 [US1] Implement the frontend state holder for current game, loading, selected mode, and error state in `frontend/src/app/services/game-state.service.ts`; replace state only from successful backend responses.
- [x] T045 [P] [US1] Implement the 3 x 3 board component in `frontend/src/app/components/game-board/`, including cell rendering, valid click events, occupied/completed disabled states, and winning-cell classes.
- [x] T046 [P] [US2] Implement move history rendering in `frontend/src/app/components/move-history/` with move number, player, row, and column.
- [x] T047 [P] [US2] Implement scoreboard rendering and Reset Scoreboard event in `frontend/src/app/components/scoreboard/`.
- [x] T048 [US1] Compose the main screen in `frontend/src/app/app.component.ts`, `app.component.html`, and `app.component.css` with mode selection, current player, winner/draw message, board, history, scoreboard, Reset Game, Undo Last Move, and Reset Scoreboard controls.
- [x] T049 [US1] Wire mode selection, cell clicks, reset, undo, scoreboard reset, loading state, and API errors in `frontend/src/app/app.component.ts` without duplicating backend game rules.
- [x] T050 [US1] Add responsive laptop-friendly layout and visible disabled/highlighted states in `frontend/src/app/app.component.css` and component styles.

**Checkpoint**: A reviewer can start both local processes, create either mode, play through the browser, see authoritative state, and use every required control.

## Phase 8: User Story 5 - Review and Run the Assessment Solution (Priority: P2)

**Goal**: Make the repository easy to run, test, review, and explain.

- [x] T051 [US5] Complete `README.md` with project overview, Angular/.NET stack, implemented features, prerequisites, chosen versions, local ports, backend/frontend startup commands, and test commands.
- [x] T052 [US5] Add the REST endpoint summary, request examples, response fields, error status behavior, and backend-state ownership explanation to `README.md`.
- [x] T053 [US5] Document Option A completed-game Undo, in-memory storage, CORS assumptions, local-only scope, and known limitations in `README.md`.
- [x] T054 [US5] Document AI tools/prompts used, generated material, manual changes, careful review areas, assumptions, trade-offs, and the panel-review explanation points in `README.md`.
- [x] T055 [US5] Add a manual verification checklist to `README.md` covering both modes, row/column/diagonal wins, draw, invalid moves, move history, both undo behaviors, reset actions, scoreboard behavior, and winning-cell highlights.

## Dependencies and Execution Order

### Phase Dependencies

- **Phase 1 Setup**: No dependencies; all project scaffolding tasks can begin immediately.
- **Phase 2 Foundation**: Depends on Phase 1; blocks every user story.
- **Phase 3 US1**: Depends on Phases 1-2; establishes core gameplay and is the MVP.
- **Phase 4 US2**: Depends on the game service from US1; can begin once game state exists.
- **Phase 5 US3**: Depends on move history and state transitions from US1; should be complete before frontend undo controls.
- **Phase 6 US4**: Depends on core move validation and history from US1; can be developed alongside reset/undo after foundation, but integration depends on shared `GameService`.
- **Phase 7 Frontend**: Depends on the REST endpoints from Phases 3-6; frontend models and component shells may start after Phase 2, but end-to-end integration waits for APIs.
- **Phase 8 US5**: Can begin during implementation, but final README verification depends on the completed local commands and API behavior.

### Parallel Opportunities

- T002-T005 can run in parallel after the repository root is established.
- T006-T008 can run in parallel; T009-T011 depend on the contracts/domain types.
- T039-T042 and T045-T047 can be developed in parallel after the frontend scaffold, subject to the shared model/API service interfaces.
- README sections can be drafted in parallel with implementation and finalized after the local commands are verified.

### Within Each User Story

- Write focused tests before or alongside implementation and run them at the story checkpoint.
- Create domain models and contracts before services; create services before controllers and frontend integration.
- Keep each story testable through its documented independent test before moving to the next priority.

## Implementation Strategy

### MVP First

1. Complete Phases 1-2.
2. Complete Phase 3 US1.
3. Run backend unit/API tests and manually exercise a two-player win and draw.
4. Continue with reset, undo, computer mode, and frontend integration.

### Completion Gate

The implementation is ready for review when the required backend and frontend tests are green, both local processes run from the README instructions, and the README verification checklist has been reviewed.
