import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { DashboardService } from '../core/services/dashboard.service';
import { StatusIndicatorComponent } from '../shared/components/status-indicator/status-indicator.component';
import { HomeComponent } from '../views/home/home.component';
import { WeatherComponent } from '../views/weather/weather.component';
import { TransportComponent } from '../views/transport/transport.component';
import { CalendarComponent } from '../views/calendar/calendar.component';
import { TimersComponent } from '../views/timers/timers.component';
import { MusicComponent } from '../views/music/music.component';
import { NewsComponent } from '../views/news/news.component';
import { SettingsComponent } from '../views/settings/settings.component';

const REFRESH_INTERVAL_MS = 30_000;

const VIEW_ORDER = ['home', 'weather', 'transport', 'calendar', 'timers', 'music'] as const;
type ViewName = (typeof VIEW_ORDER)[number] | 'news' | 'settings';
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [HomeComponent, WeatherComponent, TransportComponent, CalendarComponent, TimersComponent, MusicComponent, NewsComponent, SettingsComponent, StatusIndicatorComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly dashboardService = inject(DashboardService);
  private refreshTimer?: ReturnType<typeof setInterval>;

  readonly status = this.dashboardService.connectionStatus;
  readonly activeView = signal<ViewName>('home');
  readonly menuOpen = signal(false);
  readonly now = signal(new Date());
  private clockTimer?: ReturnType<typeof setInterval>;

  ngOnInit(): void {
    this.dashboardService.refresh();
    this.refreshTimer = setInterval(() => this.dashboardService.refresh(), REFRESH_INTERVAL_MS);
    this.clockTimer = setInterval(() => this.now.set(new Date()), 30_000);
  }

  ngOnDestroy(): void {
    clearInterval(this.refreshTimer);
    clearInterval(this.clockTimer);
  }

  toggleMenu(): void {
    this.menuOpen.update((isOpen) => !isOpen);
  }

  closeMenu(): void {
    this.menuOpen.set(false);
  }

  selectView(view: ViewName): void {
    this.activeView.set(view);
    this.closeMenu();
  }

  formattedTime(): string {
    return new Intl.DateTimeFormat('sv-SE', {
      hour: '2-digit',
      minute: '2-digit',
    }).format(this.now());
  }

  formattedDate(): string {
    return new Intl.DateTimeFormat('sv-SE', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
    }).format(this.now());
  }

}
