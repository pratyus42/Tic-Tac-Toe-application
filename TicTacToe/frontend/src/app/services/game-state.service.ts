import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { ApiError, GameMode, GameState, Player, Scoreboard } from '../models/game.models';
import { GameApiService } from './game-api.service';

@Injectable({ providedIn: 'root' })
export class GameStateService {
  private readonly api = inject(GameApiService); private readonly stateSubject = new BehaviorSubject<GameState | null>(null);
  readonly state$ = this.stateSubject.asObservable(); readonly loading$ = new BehaviorSubject(false); readonly error$ = new BehaviorSubject<string | null>(null);
  private request<T>(call: Observable<T>, update: (value: T) => void) { this.loading$.next(true); this.error$.next(null); call.subscribe({ next: value => update(value), error: (error: ApiError) => { this.error$.next(error.message); this.loading$.next(false); }, complete: () => this.loading$.next(false) }); }
  create(mode: GameMode) { this.request(this.api.create(mode), state => this.stateSubject.next(state)); }
  move(row: number, column: number) { const state = this.stateSubject.value; if (state) this.request(this.api.move(state.gameId, state.currentPlayer, row, column), next => this.stateSubject.next(next)); }
  reset() { const state = this.stateSubject.value; if (state) this.request(this.api.reset(state.gameId), next => this.stateSubject.next(next)); }
  undo() { const state = this.stateSubject.value; if (state) this.request(this.api.undo(state.gameId), next => this.stateSubject.next(next)); }
  resetScoreboard() { this.request(this.api.resetScoreboard(), score => { const state = this.stateSubject.value; if (state) this.stateSubject.next({ ...state, scoreboard: score }); }); }
}
