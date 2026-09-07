import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MoveHistoryComponent } from './move-history.component';

describe('MoveHistoryComponent', () => {
  let fixture: ComponentFixture<MoveHistoryComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [MoveHistoryComponent] }).compileComponents();
    fixture = TestBed.createComponent(MoveHistoryComponent);
    fixture.componentInstance.moves = [
      { moveNumber: 1, player: 'X', row: 0, column: 2 },
      { moveNumber: 2, player: 'O', row: 1, column: 1 }
    ];
    fixture.detectChanges();
  });

  it('renders chronological move details', () => {
    const entries = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(entries.length).toBe(2);
    expect(entries[0].querySelectorAll('td')[0].textContent).toBe('1');
    expect(entries[0].querySelectorAll('td')[1].textContent).toBe('X');
    expect(entries[0].querySelectorAll('td')[2].textContent).toBe('1');
    expect(entries[0].querySelectorAll('td')[3].textContent).toBe('3');
    expect(entries[1].querySelectorAll('td')[0].textContent).toBe('2');
    expect(entries[1].querySelectorAll('td')[1].textContent).toBe('O');
  });
});
