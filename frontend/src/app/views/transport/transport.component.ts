import { Component, computed, inject } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-transport-view',
  standalone: true,
  templateUrl: './transport.component.html',
  styleUrl: './transport.component.css',
})
export class TransportComponent {
  private readonly dashboardService = inject(DashboardService);

  readonly stopName = computed(() => this.dashboardService.data()?.transport.stopName ?? '');
  readonly departures = computed(() => this.dashboardService.data()?.transport.departures ?? []);
}
