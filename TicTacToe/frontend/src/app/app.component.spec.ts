import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { AppComponent } from './app.component';
import { GameStateService } from './services/game-state.service';

const state = {
  gameId: '1',
  board: [[null, null, null], [null, null, null], [null, null, null]],
  currentPlayer: 'X' as const,
  mode: 'TwoPlayer' as const,
  status: 'InProgress' as const,
  winner: null,
  winningCells: [],
  moveHistory: [],
  scoreboard: { xWins: 0, oWins: 0, draws: 0 },
  canUndo: false
};

describe('AppComponent', () => {
  let fixture: ComponentFixture<AppComponent>;
  let game: jasmine.SpyObj<GameStateService>;

  beforeEach(async () => {
    game = jasmine.createSpyObj<GameStateService>('GameStateService', ['create', 'move', 'reset', 'undo', 'resetScoreboard']);
    Object.defineProperties(game, {
      state$: { value: of(state) },
      loading$: { value: of(false) },
      error$: { value: of(null) }
    });
    await TestBed.configureTestingModule({ imports: [AppComponent], providers: [{ provide: GameStateService, useValue: game }] }).compileComponents();
    fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
  });

  it('shows the authoritative status and disables undo when unavailable', () => {
    expect(fixture.nativeElement.querySelector('.status').textContent).toContain("X's turn");
    expect(fixture.nativeElement.querySelector('.actions button:nth-child(2)').disabled).toBeTrue();
  });

  it('delegates mode changes and reset actions to the state service', () => {
    const select = fixture.nativeElement.querySelector('select');
    select.value = 'Computer';
    select.dispatchEvent(new Event('change'));
    fixture.nativeElement.querySelector('.actions button').click();
    expect(game.create).toHaveBeenCalledWith('Computer');
    expect(game.reset).toHaveBeenCalled();
  });
});
