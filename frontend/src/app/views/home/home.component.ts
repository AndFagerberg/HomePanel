import { Component, computed, inject, OnDestroy, OnInit, output, signal } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';
import { NewsArticleInfo } from '../../core/models/dashboard.model';
import { WeatherIconPipe } from '../../shared/pipes/weather-icon.pipe';

type HomeNavigationTarget = 'weather' | 'transport' | 'calendar' | 'timers' | 'music' | 'news';

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
  readonly latestNews = computed(() => this.getLatestNews());

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

  private getLatestNews(): NewsArticleInfo[] {
    const dashboard = this.dashboard();
    if (!dashboard) {
      return [];
    }

    return [...dashboard.localNews, ...dashboard.nationalNews]
      .sort((first, second) => Date.parse(second.publishedAt) - Date.parse(first.publishedAt))
      .slice(0, 3);
  }

}
