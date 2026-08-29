import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-home-view',
  standalone: true,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  private readonly clock = signal(new Date());
  private clockTimer?: ReturnType<typeof setInterval>;

  readonly dashboard = this.dashboardService.data;
  readonly time = computed(() =>
    this.clock().toLocaleTimeString('sv-SE', { hour: '2-digit', minute: '2-digit' }),
  );
  readonly date = computed(() =>
    this.clock().toLocaleDateString('sv-SE', { weekday: 'long', day: 'numeric', month: 'long' }),
  );
  readonly nextDeparture = computed(() => this.dashboard()?.transport.departures[0] ?? null);
  readonly nextEvent = computed(() => this.dashboard()?.calendar[0] ?? null);

  ngOnInit(): void {
    this.clockTimer = setInterval(() => this.clock.set(new Date()), 1000);
  }

  ngOnDestroy(): void {
    clearInterval(this.clockTimer);
  }
}
