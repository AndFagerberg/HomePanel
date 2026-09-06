import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { TimerService } from '../../core/services/timer.service';

@Component({
  selector: 'app-timers-view',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './timers.component.html',
  styleUrl: './timers.component.css',
})
export class TimersComponent implements OnInit, OnDestroy {
  private readonly timerService = inject(TimerService);
  private refreshTimer?: ReturnType<typeof setInterval>;

  readonly timers = this.timerService.data;
  readonly name = signal('Pasta');
  readonly hours = signal(0);
  readonly minutes = signal(10);
  readonly seconds = signal(0);
  readonly saving = signal(false);
  readonly cancelling = signal<string | null>(null);
  readonly error = signal('');
  readonly now = signal(Date.now());

  ngOnInit(): void {
    void this.refresh();
    this.refreshTimer = setInterval(() => {
      this.now.set(Date.now());
      void this.refresh();
    }, 30_000);
  }

  ngOnDestroy(): void {
    clearInterval(this.refreshTimer);
  }

  setPreset(name: string, minutes: number): void {
    this.name.set(name);
    this.hours.set(Math.floor(minutes / 60));
    this.minutes.set(minutes % 60);
    this.seconds.set(0);
  }

  updateName(event: Event): void {
    this.name.set((event.target as HTMLInputElement).value);
  }

  updateHours(event: Event): void {
    this.hours.set(Number((event.target as HTMLInputElement).value));
  }

  updateMinutes(event: Event): void {
    this.minutes.set(Number((event.target as HTMLInputElement).value));
  }

  updateSeconds(event: Event): void {
    this.seconds.set(Number((event.target as HTMLInputElement).value));
  }

  remainingTime(endsAt: string): string {
    const remainingSeconds = Math.max(0, Math.ceil((Date.parse(endsAt) - this.now()) / 1000));
    const hours = Math.floor(remainingSeconds / 3600);
    const minutes = Math.floor((remainingSeconds % 3600) / 60);
    const seconds = remainingSeconds % 60;

    return hours > 0
      ? `${hours}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
      : `${minutes}:${seconds.toString().padStart(2, '0')}`;
  }

  async createTimer(): Promise<void> {
    const durationSeconds = this.hours() * 3600 + this.minutes() * 60 + this.seconds();
    if (!this.name().trim() || durationSeconds <= 0) {
      this.error.set('Ange ett namn och en tid längre än noll.');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    try {
      await this.timerService.create({
        name: this.name().trim(),
        duration: `${this.hours().toString().padStart(2, '0')}:${this.minutes().toString().padStart(2, '0')}:${this.seconds().toString().padStart(2, '0')}`,
      });
    } catch {
      this.error.set('Det gick inte att skapa timern.');
    } finally {
      this.saving.set(false);
    }
  }

  async cancelTimer(id: string): Promise<void> {
    this.cancelling.set(id);
    this.error.set('');

    try {
      await this.timerService.cancel(id);
    } catch {
      this.error.set('Det gick inte att avbryta timern.');
    } finally {
      this.cancelling.set(null);
    }
  }

  private async refresh(): Promise<void> {
    try {
      await this.timerService.refresh();
      this.error.set('');
    } catch {
      this.error.set('Kunde inte läsa timers.');
    }
  }
}
