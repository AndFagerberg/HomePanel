import { Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { MusicApiService } from '../api/music-api.service';
import { MusicSearchResult, RadioStation } from '../models/music.model';

@Injectable({
  providedIn: 'root',
})
export class MusicService {
  private readonly radioStations = signal<RadioStation[]>([]);
  private readonly searchResults = signal<MusicSearchResult[]>([]);

  readonly stations = this.radioStations.asReadonly();
  readonly results = this.searchResults.asReadonly();

  constructor(private readonly musicApiService: MusicApiService) {}

  async refreshStations(): Promise<void> {
    const stations = await firstValueFrom(this.musicApiService.getRadioStations());
    this.radioStations.set(stations.filter((station, index, allStations) =>
      allStations.findIndex((candidate) => candidate.id === station.id) === index));
  }

  async searchSpotify(query: string): Promise<void> {
    this.searchResults.set(await firstValueFrom(this.musicApiService.searchSpotify(query)));
  }

  async playSpotify(uri: string): Promise<void> {
    await firstValueFrom(this.musicApiService.playSpotify({ uri }));
  }

  async playRadio(stationId: string): Promise<void> {
    await firstValueFrom(this.musicApiService.playRadio({ stationId }));
  }
}
