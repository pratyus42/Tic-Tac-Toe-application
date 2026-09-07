# Tic Tac Toe Application

## 1. Project overview

 Tic Tac Toe Application is a browser game with a standalone Angular frontend and a .NET Web API backend. The backend is authoritative for board state, turns, validation, win/draw detection, move history, computer moves, undo behavior, and the session scoreboard. The application is intended for local assessment and demonstration use.

## 2. Tech stack

- .NET 8 ASP.NET Core Web API
- C# domain and service layer
- xUnit and ASP.NET Core test hosting for backend tests
- Angular 18 standalone components
- TypeScript and RxJS
- Karma/Jasmine for Angular tests
- Swashbuckle for Swagger/OpenAPI documentation
- In-memory storage with no external database

## 3. Features implemented

- Two-player mode and deterministic computer mode
- 3 x 3 board with row, column, and diagonal win detection
- Draw detection and winning-cell highlighting
- Backend validation for invalid coordinates, occupied cells, wrong turns, and completed games
- Chronological move history with player, move number, row, and column
- Reset Game without changing the scoreboard
- Mode-specific undo: one move in TwoPlayer mode and the latest X/O pair in Computer mode
- Option A behavior: undo is unavailable after a win or draw
- Deterministic computer priority: winning move, block, center, corners, then row-major fallback
- Scoreboard display and scoreboard reset
- Responsive Angular UI with disabled and highlighted states
- Swagger UI for interactive API testing

## 4. Run the backend locally

Prerequisite: install the .NET 8 SDK and confirm it is available with `dotnet --version`.

1. Open a terminal.
2. Change to your local repository root, the folder containing `README.md`, `backend`, and `frontend`. Replace the placeholder below with the path to your own checkout:

	```powershell
	cd "<your-local-repository-path>"
	```

3. Restore backend dependencies:

	```powershell
	dotnet restore backend\TicTacToe.sln
	```

4. Start the API:

	```powershell
	dotnet run --project backend\TicTacToe.Api
	```

5. Leave this terminal running. A successful startup displays a message similar to:

	```text
	Now listening on: http://localhost:5050
	```

The API base URL is [http://localhost:5050](http://localhost:5050). Swagger UI is available at [http://localhost:5050/swagger](http://localhost:5050/swagger), and the raw OpenAPI document is available at [http://localhost:5050/swagger/v1/swagger.json](http://localhost:5050/swagger/v1/swagger.json).

To stop the backend, return to its terminal and press `Ctrl+C`.

## 5. Run the frontend locally

Prerequisites: install Node.js 20 or newer, which includes npm. Confirm both are available with `node --version` and `npm.cmd --version`.

Use a second terminal while the backend terminal remains running:

1. From the repository root, install frontend dependencies once:

	```powershell
	cd "<your-local-repository-path>\frontend"
	npm.cmd install
	```

2. Start Angular:

	```powershell
	npm.cmd start
	```

3. Leave this terminal running. A successful startup displays a local URL similar to:

	```text
	Local: http://localhost:4200/
	```

4. Open [http://localhost:4200](http://localhost:4200) in a browser. The page should load the game from the API running on port `5050`.

The frontend expects the backend at `http://localhost:5050`; local CORS allows requests from `http://localhost:4200`. To stop Angular, return to its terminal and press `Ctrl+C`.

### Run the complete application

Reviewers need two terminals:

**Terminal 1, backend:**

```powershell
cd "<your-local-repository-path>"
dotnet run --project backend\TicTacToe.Api
```

**Terminal 2, frontend:**

```powershell
cd "<your-local-repository-path>\frontend"
npm.cmd install
npm.cmd start
```

Then use the browser UI at `http://localhost:4200` or test the API interactively at `http://localhost:5050/swagger`.

## 6. API endpoint summary

| Method | Endpoint | Purpose |
| --- | --- | --- |
| `POST` | `/api/games` | Create a `TwoPlayer` or `Computer` game |
| `GET` | `/api/games/{gameId}` | Get complete authoritative game state |
| `POST` | `/api/games/{gameId}/moves` | Submit a player move |
| `POST` | `/api/games/{gameId}/undo` | Undo the latest valid action |
| `POST` | `/api/games/{gameId}/reset` | Reset one game while preserving its scoreboard |
| `GET` | `/api/scoreboard` | Get session scoreboard totals |
| `POST` | `/api/scoreboard/reset` | Reset scoreboard totals without changing games |

Create-game request:

```json
{
	"mode": "TwoPlayer"
}
```

Move request. Rows and columns are zero-based and must be between `0` and `2`:

```json
{
	"player": "X",
	"row": 0,
	"column": 0
}
```

Successful game responses include `gameId`, `board`, `currentPlayer`, `mode`, `status`, `winner`, `winningCells`, `moveHistory`, `scoreboard`, and `canUndo`. Invalid requests return `400`, unknown games return `404`, and unavailable undo returns `409` with an `UndoUnavailable` error code. Error responses use `{ "code": "...", "message": "..." }`.

## 7. Run tests and generate reports

Backend tests:

```powershell
dotnet test backend/TicTacToe.sln --results-directory reports\backend --logger "console;verbosity=normal" --logger "trx;LogFileName=backend-tests.trx"
```

Frontend tests and report:

```powershell
cd frontend
npm.cmd test -- --progress=false --browsers=ChromeHeadless
npm.cmd run test:report
```

The backend test result displays the number of passed, failed, skipped, and total
tests. The frontend test command runs once in headless Chrome and displays
`Executed X of X` in the result. 
Generated reports are ignored by git.

Report files:

- Backend: `reports\backend\backend-tests.trx`
- Frontend: `reports\frontend\frontend-tests.xml`
- Combined HTML: `reports\test-report.html`

Open `reports\test-report.html` in a browser for a combined, visually formatted
report with summary cards and aligned backend/frontend test tables. 
Run the
backend and frontend test commands first so the source reports are current.

## 8. Assumptions and known limitations

Assumptions:

- `X` always starts a new game.
- In Computer mode, the human is `X` and the computer is `O`.
- Rows and columns are zero-based values from `0` to `2`.
- Completed games cannot be undone.
- Resetting a game preserves the session scoreboard.
- The API is intended for one local in-memory session.

Known limitations:

- Game and scoreboard data are lost when the API process restarts.
- There is no authentication, authorization, persistent database, or multi-user isolation.
- The computer strategy is deterministic and intentionally lightweight rather than adaptive.
- The API and frontend are configured for local development rather than production deployment.
- No automated browser end-to-end test suite is included.

## 9. Prompt summary and AI workflow

Development followed a spec-driven workflow using GitHub Spec Kit. The project
artifacts are stored in `specs/001-tic-tac-toe-application/`:

- `spec.md` defines the scope, user stories, acceptance scenarios, and behavior
	requirements.
- `plan.md` defines the technical context, architecture, project structure, and
	design constraints for the Angular and .NET applications.
- `tasks.md` breaks the work into implementation and testing tasks grouped by
	project foundation and user story.

AI assistance was used during the spec-driven workflow to refine requirements,
derive the implementation plan, generate focused code and tests, and review the
result against the acceptance scenarios. Prompts emphasized backend-authoritative
game state, exact API contracts, deterministic computer behavior, unchanged state
after rejected moves, mode-specific undo, scoreboard correctness, CORS, and
local run instructions.

The completed implementation was validated with the backend and frontend unit
test suites and the generated HTML test report.

## 10. Verification checklist

- [ ] Start the Angular application locally at `http://localhost:4200`.
- [ ] Start the .NET API locally at `http://localhost:5050`.
- [ ] Confirm frontend actions communicate with the backend through REST APIs.
- [ ] Create a new game in both TwoPlayer and Computer modes.
- [ ] Verify turns alternate correctly and invalid moves leave state unchanged.
- [ ] Verify occupied, out-of-range, wrong-player, and completed-game errors.
- [ ] Verify row, column, and diagonal wins, draw detection, and post-completion lock.
- [ ] Verify winning cells are highlighted and move history is shown chronologically.
- [ ] Verify computer winning/blocking/center/corner/fallback behavior.
- [ ] Verify both undo behaviors and completed-game disablement.
- [ ] Verify scoreboard updates, Reset Game, and Reset Scoreboard behavior.
- [ ] Run the backend and frontend unit test commands and review the HTML report.
