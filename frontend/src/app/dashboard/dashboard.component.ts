import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { DashboardService } from '../core/services/dashboard.service';
import { StatusIndicatorComponent } from '../shared/components/status-indicator/status-indicator.component';
import { HomeComponent } from '../views/home/home.component';
import { WeatherComponent } from '../views/weather/weather.component';

const REFRESH_INTERVAL_MS = 30_000;

// View rotation order and how long each view stays on screen. See PROJECT.md §13.
const VIEW_ORDER = ['home', 'weather'] as const;
type ViewName = (typeof VIEW_ORDER)[number];
const VIEW_DURATIONS_MS: Record<ViewName, number> = {
  home: 15_000,
  weather: 10_000,
};

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [HomeComponent, WeatherComponent, StatusIndicatorComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  private refreshTimer?: ReturnType<typeof setInterval>;
  private rotationTimer?: ReturnType<typeof setTimeout>;
  private readonly viewIndex = signal(0);

  readonly status = this.dashboardService.connectionStatus;
  readonly activeView = computed<ViewName>(() => VIEW_ORDER[this.viewIndex()]);

  ngOnInit(): void {
    this.dashboardService.refresh();
    this.refreshTimer = setInterval(() => this.dashboardService.refresh(), REFRESH_INTERVAL_MS);
    this.scheduleNextView();
  }

  ngOnDestroy(): void {
    clearInterval(this.refreshTimer);
    clearTimeout(this.rotationTimer);
  }

  private scheduleNextView(): void {
    this.rotationTimer = setTimeout(() => {
      this.viewIndex.set((this.viewIndex() + 1) % VIEW_ORDER.length);
      this.scheduleNextView();
    }, VIEW_DURATIONS_MS[this.activeView()]);
  }
}
