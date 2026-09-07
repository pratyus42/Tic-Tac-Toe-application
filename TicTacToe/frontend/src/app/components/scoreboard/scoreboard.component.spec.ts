import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ScoreboardComponent } from './scoreboard.component';

describe('ScoreboardComponent', () => {
  let fixture: ComponentFixture<ScoreboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ScoreboardComponent] }).compileComponents();
    fixture = TestBed.createComponent(ScoreboardComponent);
    fixture.componentInstance.score = { xWins: 2, oWins: 1, draws: 3 };
    fixture.detectChanges();
  });

  it('renders scoreboard values', () => {
    const values = fixture.nativeElement.querySelectorAll('b');
    expect(Array.from(values as NodeListOf<Element>).map((value) => value.textContent)).toEqual(['2', '1', '3']);
  });

  it('emits reset when the reset button is clicked', () => {
    const reset = jasmine.createSpy('reset');
    fixture.componentInstance.reset.subscribe(reset);
    fixture.nativeElement.querySelector('button').click();
    expect(reset).toHaveBeenCalled();
  });
});
