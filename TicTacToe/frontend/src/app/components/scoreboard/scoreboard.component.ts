import { Component, EventEmitter, Input, Output } from '@angular/core';
import { Scoreboard } from '../../models/game.models';
@Component({ selector: 'app-scoreboard', standalone: true, templateUrl: './scoreboard.component.html' }) export class ScoreboardComponent { @Input({ required: true }) score!: Scoreboard; @Output() reset = new EventEmitter<void>(); }
