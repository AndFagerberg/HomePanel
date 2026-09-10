import { Component, inject, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { MusicSearchResult } from '../../core/models/music.model';
import { MusicService } from '../../core/services/music.service';

@Component({
  selector: 'app-music-view',
  standalone: true,
  templateUrl: './music.component.html',
  styleUrl: './music.component.css',
})
export class MusicComponent implements OnInit {
  private readonly musicService = inject(MusicService);

  readonly stations = this.musicService.stations;
  readonly results = this.musicService.results;
  readonly query = signal('');
  readonly loading = signal(false);
  readonly playing = signal('');
  readonly message = signal('');
  readonly error = signal('');

  ngOnInit(): void {
    void this.loadStations();
  }

  updateQuery(event: Event): void {
    this.query.set((event.target as HTMLInputElement).value);
  }

  async search(): Promise<void> {
    if (this.query().trim().length < 2) {
      this.error.set('Skriv minst två tecken.');
      return;
    }

    this.loading.set(true);
    this.message.set('');
    this.error.set('');

    try {
      await this.musicService.searchSpotify(this.query().trim());
      if (!this.results().length) {
        this.message.set('Inga träffar. Kontrollera Spotify-konfigurationen om sökningen borde ge resultat.');
      }
    } catch {
      this.error.set('Det gick inte att söka på Spotify.');
    } finally {
      this.loading.set(false);
    }
  }

  async playSpotify(result: MusicSearchResult): Promise<void> {
    await this.play(result.uri, result.title, () => this.musicService.playSpotify(result.uri));
  }

  async playRadio(stationId: string, stationName: string): Promise<void> {
    await this.play(stationId, stationName, () => this.musicService.playRadio(stationId));
  }

  private async loadStations(): Promise<void> {
    try {
      await this.musicService.refreshStations();
    } catch {
      this.error.set('Kunde inte läsa radiokanaler.');
    }
  }

  private async play(id: string, title: string, action: () => Promise<void>): Promise<void> {
    this.playing.set(id);
    this.message.set('');
    this.error.set('');

    try {
      await action();
      this.message.set(`Startar ${title} på Google Home.`);
    } catch (error) {
      this.error.set(this.getErrorMessage(error));
    } finally {
      this.playing.set('');
    }
  }

  private getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse && typeof error.error?.error === 'string') {
      return error.error.error;
    }

    return 'Det gick inte att starta musiken. Kontrollera Google Home- och Spotify-konfigurationen.';
  }
}
