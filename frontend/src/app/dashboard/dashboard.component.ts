import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { DashboardService } from '../core/services/dashboard.service';
import { StatusIndicatorComponent } from '../shared/components/status-indicator/status-indicator.component';
import { HomeComponent } from '../views/home/home.component';

const REFRESH_INTERVAL_MS = 30_000;

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [HomeComponent, StatusIndicatorComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  private refreshTimer?: ReturnType<typeof setInterval>;

  readonly status = this.dashboardService.connectionStatus;

  ngOnInit(): void {
    this.dashboardService.refresh();
    this.refreshTimer = setInterval(() => this.dashboardService.refresh(), REFRESH_INTERVAL_MS);
  }

  ngOnDestroy(): void {
    clearInterval(this.refreshTimer);
  }
}
