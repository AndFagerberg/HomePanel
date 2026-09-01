import { Component, computed, inject } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';
import { WeatherIconPipe } from '../../shared/pipes/weather-icon.pipe';

@Component({
  selector: 'app-weather-view',
  standalone: true,
  imports: [WeatherIconPipe],
  templateUrl: './weather.component.html',
  styleUrl: './weather.component.css',
})
export class WeatherComponent {
  private readonly dashboardService = inject(DashboardService);

  readonly primary = computed(() => this.dashboardService.data()?.weatherLocations[0] ?? null);
  readonly otherLocations = computed(
    () => this.dashboardService.data()?.weatherLocations.slice(1) ?? [],
  );
}
