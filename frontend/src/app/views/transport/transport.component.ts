import { Component, computed, inject } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';
import { IconComponent } from '../../shared/components/icon/icon.component';

@Component({
  selector: 'app-transport-view',
  standalone: true,
  imports: [IconComponent],
  templateUrl: './transport.component.html',
  styleUrl: './transport.component.css',
})
export class TransportComponent {
  private readonly dashboardService = inject(DashboardService);

  readonly stopName = computed(() => this.dashboardService.data()?.transport.stopName ?? '');
  readonly departures = computed(() => this.dashboardService.data()?.transport.departures ?? []);
}
