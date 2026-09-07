import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import { ApiError, GameMode, GameState, Move, Player, Scoreboard } from '../models/game.models';

@Injectable({ providedIn: 'root' })
export class GameApiService {
  private readonly http = inject(HttpClient); private readonly base = environment.apiUrl;
  private errors(error: HttpErrorResponse) { const body = error.error as Partial<ApiError>; return throwError(() => ({ code: body?.code ?? 'RequestFailed', message: body?.message ?? 'The request could not be completed.' } as ApiError)); }
  create(mode: GameMode) { return this.http.post<GameState>(`${this.base}/games`, { mode }).pipe(catchError(e => this.errors(e))); }
  get(id: string) { return this.http.get<GameState>(`${this.base}/games/${id}`).pipe(catchError(e => this.errors(e))); }
  move(id: string, player: Player, row: number, column: number) { return this.http.post<GameState>(`${this.base}/games/${id}/moves`, { player, row, column }).pipe(catchError(e => this.errors(e))); }
  undo(id: string) { return this.http.post<GameState>(`${this.base}/games/${id}/undo`, {}).pipe(catchError(e => this.errors(e))); }
  reset(id: string) { return this.http.post<GameState>(`${this.base}/games/${id}/reset`, {}).pipe(catchError(e => this.errors(e))); }
  scoreboard() { return this.http.get<Scoreboard>(`${this.base}/scoreboard`).pipe(catchError(e => this.errors(e))); }
  resetScoreboard() { return this.http.post<Scoreboard>(`${this.base}/scoreboard/reset`, {}).pipe(catchError(e => this.errors(e))); }
}
