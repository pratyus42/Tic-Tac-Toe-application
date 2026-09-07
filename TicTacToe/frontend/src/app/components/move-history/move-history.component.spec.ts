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
    const entries = fixture.nativeElement.querySelectorAll('li');
    expect(entries.length).toBe(2);
    expect(entries[0].textContent).toContain('1. X');
    expect(entries[0].textContent).toContain('row 1, column 3');
    expect(entries[1].textContent).toContain('2. O');
  });
});
