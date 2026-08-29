import { Component, Input } from '@angular/core';
import { ConnectionStatus } from '../../../core/services/dashboard.service';

@Component({
  selector: 'app-status-indicator',
  standalone: true,
  templateUrl: './status-indicator.component.html',
  styleUrl: './status-indicator.component.css',
})
export class StatusIndicatorComponent {
  @Input({ required: true }) status!: ConnectionStatus;
}
