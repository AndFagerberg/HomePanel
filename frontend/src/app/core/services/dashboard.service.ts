import { Injectable, signal } from '@angular/core';
import { firstValueFrom, Observable } from 'rxjs';
import { DashboardApiService } from '../api/dashboard-api.service';
import { MusicApiService } from '../api/music-api.service';
import { TimerApiService } from '../api/timer-api.service';
import { Dashboard } from '../models/dashboard.model';

export type ConnectionStatus = 'online' | 'stale' | 'unavailable';

// How often each data domain is re-fetched. Fast-changing data (transport, timers, music) polls
// often; slow-changing data (weather, calendar, news, cabin) polls rarely to save bandwidth/battery
// on the backend integrations.
const POLL_INTERVALS_MS = {
  weather: 10 * 60_000,
  transport: 20_000,
  calendar: 5 * 60_000,
  news: 15 * 60_000,
  cabin: 2 * 60_000,
  indoor: 2 * 60_000,
  timers: 10_000,
  music: 10_000,
} as const;

// Central dashboard state. Views read from here instead of calling the API themselves.
@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private readonly dashboard = signal<Dashboard | null>(null);
  private readonly status = signal<ConnectionStatus>('unavailable');
  private readonly lastUpdated = signal<Date | null>(null);
  private readonly pollTimers: ReturnType<typeof setInterval>[] = [];

  readonly data = this.dashboard.asReadonly();
  readonly connectionStatus = this.status.asReadonly();
  readonly updatedAt = this.lastUpdated.asReadonly();

  constructor(
    private readonly dashboardApiService: DashboardApiService,
    private readonly musicApiService: MusicApiService,
    private readonly timerApiService: TimerApiService,
  ) {}

  /** Loads the full dashboard once, then starts one independent poller per data domain. */
  async start(): Promise<void> {
    await this.refresh();

    this.pollTimers.push(
      setInterval(() => void this.refreshWeather(), POLL_INTERVALS_MS.weather),
      setInterval(() => void this.refreshTransport(), POLL_INTERVALS_MS.transport),
      setInterval(() => void this.refreshCalendar(), POLL_INTERVALS_MS.calendar),
      setInterval(() => void this.refreshNews(), POLL_INTERVALS_MS.news),
      setInterval(() => void this.refreshCabin(), POLL_INTERVALS_MS.cabin),
      setInterval(() => void this.refreshIndoor(), POLL_INTERVALS_MS.indoor),
      setInterval(() => void this.refreshTimers(), POLL_INTERVALS_MS.timers),
      setInterval(() => void this.refreshMusic(), POLL_INTERVALS_MS.music),
    );
  }

  stop(): void {
    this.pollTimers.splice(0).forEach(clearInterval);
  }

  /** Full snapshot load, used for the initial bootstrap. */
  async refresh(): Promise<void> {
    try {
      const dashboard = await firstValueFrom(this.dashboardApiService.getDashboard());
      this.dashboard.set(dashboard);
      this.onFetchSucceeded();
    } catch {
      this.onFetchFailed();
    }
  }

  async refreshWeather(): Promise<void> {
    await this.patch(this.dashboardApiService.getWeather(), (section) => ({
      weather: section.primary,
      weatherLocations: section.locations,
    }));
  }

  async refreshTransport(): Promise<void> {
    await this.patch(this.dashboardApiService.getTransport(), (transport) => ({ transport }));
  }

  async refreshCalendar(): Promise<void> {
    await this.patch(this.dashboardApiService.getCalendar(), (section) => ({
      calendar: section.calendar,
      schedule: section.schedule,
    }));
  }

  async refreshNews(): Promise<void> {
    await this.patch(this.dashboardApiService.getNews(), (section) => ({
      nationalNews: section.nationalNews,
      localNews: section.localNews,
    }));
  }

  async refreshCabin(): Promise<void> {
    await this.patch(this.dashboardApiService.getCabin(), (airPatrol) => ({ airPatrol }));
  }

  async refreshIndoor(): Promise<void> {
    await this.patch(this.dashboardApiService.getIndoor(), (indoor) => ({ indoor }));
  }

  async refreshTimers(): Promise<void> {
    await this.patch(this.timerApiService.getTimers(), (timers) => ({ timers }));
  }

  async refreshMusic(): Promise<void> {
    await this.patch(this.musicApiService.getCurrentPlayback(), (music) => ({ music }));
  }

  private async patch<T>(
    source: Observable<T>,
    toPartial: (value: T) => Partial<Dashboard>,
  ): Promise<void> {
    const current = this.dashboard();
    if (!current) {
      // No bootstrap snapshot yet: nothing to merge into.
      return;
    }

    try {
      const value = await firstValueFrom(source);
      this.dashboard.set({ ...current, ...toPartial(value) });
      this.onFetchSucceeded();
    } catch {
      this.onFetchFailed();
    }
  }

  private onFetchSucceeded(): void {
    this.lastUpdated.set(new Date());
    this.status.set('online');
  }

  private onFetchFailed(): void {
    // Backend unreachable: keep the last known-good data visible per offline requirements.
    this.status.set(this.dashboard() ? 'stale' : 'unavailable');
  }
}

