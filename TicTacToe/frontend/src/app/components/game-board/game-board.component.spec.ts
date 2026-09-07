import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GameBoardComponent } from './game-board.component';
import { GameState } from '../../models/game.models';

const state: GameState = { gameId: '1', board: [['X', null, null], [null, 'O', null], [null, null, null]], currentPlayer: 'X', mode: 'TwoPlayer', status: 'InProgress', winner: null, winningCells: [{ row: 0, column: 0 }], moveHistory: [], scoreboard: { xWins: 0, oWins: 0, draws: 0 }, canUndo: true };
describe('GameBoardComponent', () => { let fixture: ComponentFixture<GameBoardComponent>;
  beforeEach(async () => { await TestBed.configureTestingModule({ imports: [GameBoardComponent] }).compileComponents(); fixture = TestBed.createComponent(GameBoardComponent); fixture.componentInstance.state = state; fixture.detectChanges(); });
  it('renders nine cells and highlights winning cells', () => { expect(fixture.nativeElement.querySelectorAll('.cell').length).toBe(9); expect(fixture.nativeElement.querySelector('.winner')).not.toBeNull(); });
  it('disables occupied cells', () => { expect(fixture.nativeElement.querySelectorAll('.cell:disabled').length).toBe(2); });
  it('emits a selected empty cell and ignores disabled cells', () => {
    const selected = jasmine.createSpy('selected');
    fixture.componentInstance.cellSelected.subscribe(selected);
    const cells = fixture.nativeElement.querySelectorAll('.cell');
    cells[2].click();
    cells[0].click();
    expect(selected).toHaveBeenCalledOnceWith([0, 2]);
  });
  it('disables every cell after completion', () => {
    fixture.componentInstance.state = { ...state, status: 'Won', winner: 'X' };
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.cell:disabled').length).toBe(9);
  });
});
