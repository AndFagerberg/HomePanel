import { Component, computed, inject, output } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';

type HomeNavigationTarget = 'weather' | 'transport' | 'calendar';

@Component({
  selector: 'app-home-view',
  standalone: true,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent {
  private readonly dashboardService = inject(DashboardService);

  readonly navigate = output<HomeNavigationTarget>();
  readonly dashboard = this.dashboardService.data;
  readonly nextDeparture = computed(() => this.dashboard()?.transport.departures[0] ?? null);
  readonly nextEvent = computed(() => this.dashboard()?.calendar[0] ?? null);

}
