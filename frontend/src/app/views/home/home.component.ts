import { Component, computed, inject, OnDestroy, OnInit, output, signal } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';
import { WeatherIconPipe } from '../../shared/pipes/weather-icon.pipe';

type HomeNavigationTarget = 'weather' | 'transport' | 'calendar' | 'timers';

@Component({
  selector: 'app-home-view',
  standalone: true,
  imports: [WeatherIconPipe],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css',
})
export class HomeComponent implements OnInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  private clockTimer?: ReturnType<typeof setInterval>;

  readonly navigate = output<HomeNavigationTarget>();
  readonly dashboard = this.dashboardService.data;
  readonly now = signal(Date.now());
  readonly nextDeparture = computed(() => this.dashboard()?.transport.departures[0] ?? null);
  readonly nextEvent = computed(() => this.dashboard()?.calendar[0] ?? null);

  ngOnInit(): void {
    this.clockTimer = setInterval(() => this.now.set(Date.now()), 1_000);
  }

  ngOnDestroy(): void {
    clearInterval(this.clockTimer);
  }

  remainingTime(endsAt: string): string {
    const remainingSeconds = Math.max(0, Math.ceil((Date.parse(endsAt) - this.now()) / 1000));
    const minutes = Math.floor(remainingSeconds / 60);
    const seconds = remainingSeconds % 60;

    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

}
