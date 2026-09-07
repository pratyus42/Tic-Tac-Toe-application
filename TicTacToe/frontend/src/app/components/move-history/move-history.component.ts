import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Move } from '../../models/game.models';
@Component({ selector: 'app-move-history', standalone: true, imports: [CommonModule], templateUrl: './move-history.component.html' }) export class MoveHistoryComponent { @Input() moves: Move[] = []; }
