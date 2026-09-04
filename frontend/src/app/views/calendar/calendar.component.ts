import { Component, computed, inject } from '@angular/core';
import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-calendar-view',
  standalone: true,
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.css',
})
export class CalendarComponent {
  private readonly dashboardService = inject(DashboardService);

  readonly upcomingEvents = computed(() => this.dashboardService.data()?.calendar ?? []);
}
