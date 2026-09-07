import { AsyncPipe, NgIf } from '@angular/common';
import { Component, inject } from '@angular/core';
import { GameBoardComponent } from './components/game-board/game-board.component';
import { MoveHistoryComponent } from './components/move-history/move-history.component';
import { ScoreboardComponent } from './components/scoreboard/scoreboard.component';
import { GameMode, GameState } from './models/game.models';
import { GameStateService } from './services/game-state.service';

@Component({ selector: 'app-root', standalone: true, imports: [AsyncPipe, NgIf, GameBoardComponent, MoveHistoryComponent, ScoreboardComponent], templateUrl: './app.component.html', styleUrl: './app.component.css' })
export class AppComponent {
  readonly game = inject(GameStateService); readonly modes: GameMode[] = ['TwoPlayer', 'Computer'];
  constructor() { this.game.create('TwoPlayer'); }
  chooseMode(mode: GameMode) { this.game.create(mode); }
  message(state: GameState) { return state.status === 'Won' ? `${state.winner} wins` : state.status === 'Draw' ? 'Draw game' : `${state.currentPlayer}'s turn`; }
}
