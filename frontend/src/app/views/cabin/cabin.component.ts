import { Component, computed, inject } from '@angular/core';
import { AirPatrolHistoryPoint } from '../../core/models/dashboard.model';
import { DashboardService } from '../../core/services/dashboard.service';

interface ChartData {
  temperaturePoints: string;
  targetPoints: string;
  humidityPoints: string;
  minimumTemperature: number;
  maximumTemperature: number;
  minimumHumidity: number;
  maximumHumidity: number;
  firstTimestamp: string;
  lastTimestamp: string;
}

@Component({
  selector: 'app-cabin-view',
  standalone: true,
  templateUrl: './cabin.component.html',
  styleUrl: './cabin.component.css',
})
export class CabinComponent {
  private readonly dashboardService = inject(DashboardService);

  readonly status = computed(() => this.dashboardService.data()?.airPatrol ?? null);
  readonly chart = computed(() => this.createChart(this.status()?.history ?? []));

  modeLabel(mode: string): string {
    const labels: Record<string, string> = {
      heat: 'Värme',
      cool: 'Kyla',
      lowheat: 'Underhållsvärme',
      off: 'Av',
    };
    return labels[mode.toLowerCase()] ?? mode;
  }

  updatedLabel(updatedAt: string): string {
    return new Intl.DateTimeFormat('sv-SE', {
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(updatedAt));
  }

  private createChart(history: AirPatrolHistoryPoint[]): ChartData | null {
    if (history.length < 2) {
      return null;
    }

    const temperatures = history.flatMap((point) =>
      point.targetTemperature === null
        ? [point.temperature]
        : [point.temperature, point.targetTemperature],
    );
    const humidityValues = history
      .map((point) => point.humidity)
      .filter((value): value is number => value !== null);
    const minimumTemperature = Math.floor(Math.min(...temperatures) - 1);
    const maximumTemperature = Math.ceil(Math.max(...temperatures) + 1);
    const minimumHumidity = humidityValues.length ? Math.floor(Math.min(...humidityValues) - 5) : 0;
    const maximumHumidity = humidityValues.length ? Math.ceil(Math.max(...humidityValues) + 5) : 100;

    return {
      temperaturePoints: this.toPoints(history, (point) => point.temperature, minimumTemperature, maximumTemperature),
      targetPoints: this.toPoints(history, (point) => point.targetTemperature, minimumTemperature, maximumTemperature),
      humidityPoints: this.toPoints(history, (point) => point.humidity, minimumHumidity, maximumHumidity),
      minimumTemperature,
      maximumTemperature,
      minimumHumidity,
      maximumHumidity,
      firstTimestamp: this.historyTimeLabel(history[0].timestamp),
      lastTimestamp: this.historyTimeLabel(history.at(-1)!.timestamp),
    };
  }

  private toPoints(
    history: AirPatrolHistoryPoint[],
    selectValue: (point: AirPatrolHistoryPoint) => number | null,
    minimum: number,
    maximum: number,
  ): string {
    const start = Date.parse(history[0].timestamp);
    const duration = Math.max(Date.parse(history.at(-1)!.timestamp) - start, 1);
    const range = Math.max(maximum - minimum, 1);

    return history
      .filter((point) => selectValue(point) !== null)
      .map((point) => {
        const value = selectValue(point)!;
        const x = 12 + ((Date.parse(point.timestamp) - start) / duration) * 576;
        const y = 10 + (1 - (value - minimum) / range) * 110;
        return `${x.toFixed(1)},${y.toFixed(1)}`;
      })
      .join(' ');
  }

  private historyTimeLabel(timestamp: string): string {
    return new Intl.DateTimeFormat('sv-SE', {
      day: 'numeric',
      month: 'short',
    }).format(new Date(timestamp));
  }
}
