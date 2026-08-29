import { Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { DashboardApiService } from '../api/dashboard-api.service';
import { Dashboard } from '../models/dashboard.model';

export type ConnectionStatus = 'online' | 'stale' | 'unavailable';

// Central dashboard state. Views read from here instead of calling the API themselves.
@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private readonly dashboard = signal<Dashboard | null>(null);
  private readonly status = signal<ConnectionStatus>('unavailable');
  private readonly lastUpdated = signal<Date | null>(null);

  readonly data = this.dashboard.asReadonly();
  readonly connectionStatus = this.status.asReadonly();
  readonly updatedAt = this.lastUpdated.asReadonly();

  constructor(private readonly dashboardApiService: DashboardApiService) {}

  async refresh(): Promise<void> {
    try {
      const dashboard = await firstValueFrom(this.dashboardApiService.getDashboard());
      this.dashboard.set(dashboard);
      this.lastUpdated.set(new Date());
      this.status.set('online');
    } catch {
      // Backend unreachable: keep the last known-good data visible per offline requirements.
      this.status.set(this.dashboard() ? 'stale' : 'unavailable');
    }
  }
}
