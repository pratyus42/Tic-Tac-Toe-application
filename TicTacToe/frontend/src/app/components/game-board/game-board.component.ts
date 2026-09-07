import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { GameState, Player } from '../../models/game.models';

@Component({ selector: 'app-game-board', standalone: true, imports: [CommonModule], templateUrl: './game-board.component.html', styleUrl: './game-board.component.css' })
export class GameBoardComponent {
  @Input({ required: true }) state!: GameState; @Output() cellSelected = new EventEmitter<[number, number]>();
  cells = Array.from({ length: 9 }, (_, index) => [Math.floor(index / 3), index % 3]);
  disabled(row: number, column: number) { return this.state.status !== 'InProgress' || this.state.board[row][column] !== null; }
  winning(row: number, column: number) { return this.state.winningCells.some(cell => cell.row === row && cell.column === column); }
  select(row: number, column: number) { if (!this.disabled(row, column)) this.cellSelected.emit([row, column]); }
}
