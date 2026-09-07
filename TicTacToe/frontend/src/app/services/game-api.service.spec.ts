import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { GameApiService } from './game-api.service';

describe('GameApiService', () => { let api: GameApiService; let http: HttpTestingController;
  beforeEach(() => { TestBed.configureTestingModule({ providers: [GameApiService, provideHttpClient(), provideHttpClientTesting()] }); api = TestBed.inject(GameApiService); http = TestBed.inject(HttpTestingController); });
  afterEach(() => http.verify());
  it('posts game creation to the documented route', () => { api.create('Computer').subscribe(); const request = http.expectOne('http://localhost:5050/api/games'); expect(request.request.method).toBe('POST'); expect(request.request.body).toEqual({ mode: 'Computer' }); request.flush({}); });
  it('posts moves with the typed payload', () => { api.move('abc', 'X', 2, 1).subscribe(); const request = http.expectOne('http://localhost:5050/api/games/abc/moves'); expect(request.request.body).toEqual({ player: 'X', row: 2, column: 1 }); request.flush({}); });
  it('uses the documented routes for game actions and scoreboard operations', () => {
    api.get('abc').subscribe(); expect(http.expectOne('http://localhost:5050/api/games/abc').request.method).toBe('GET');
    api.reset('abc').subscribe(); expect(http.expectOne('http://localhost:5050/api/games/abc/reset').request.method).toBe('POST');
    api.undo('abc').subscribe(); expect(http.expectOne('http://localhost:5050/api/games/abc/undo').request.method).toBe('POST');
    api.scoreboard().subscribe(); expect(http.expectOne('http://localhost:5050/api/scoreboard').request.method).toBe('GET');
    api.resetScoreboard().subscribe(); expect(http.expectOne('http://localhost:5050/api/scoreboard/reset').request.method).toBe('POST');
  });
  it('maps API errors to the typed error shape', () => {
    let error: unknown;
    api.get('missing').subscribe({ error: value => error = value });
    http.expectOne('http://localhost:5050/api/games/missing').flush({ code: 'GameNotFound', message: 'Missing game' }, { status: 404, statusText: 'Not Found' });
    expect(error).toEqual({ code: 'GameNotFound', message: 'Missing game' });
  });
});
