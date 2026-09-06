import { Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { TimerApiService } from '../api/timer-api.service';
import { CreateTimerRequest, TimerInfo } from '../models/timer.model';

@Injectable({
  providedIn: 'root',
})
export class TimerService {
  private readonly timers = signal<TimerInfo[]>([]);

  readonly data = this.timers.asReadonly();

  constructor(private readonly timerApiService: TimerApiService) {}

  async refresh(): Promise<void> {
    this.timers.set(await firstValueFrom(this.timerApiService.getTimers()));
  }

  async create(request: CreateTimerRequest): Promise<void> {
    await firstValueFrom(this.timerApiService.createTimer(request));
    await this.refresh();
  }

  async cancel(id: string): Promise<void> {
    await firstValueFrom(this.timerApiService.cancelTimer(id));
    await this.refresh();
  }
}
