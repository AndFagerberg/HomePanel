import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { MusicSearchResult, PlayRadioRequest, PlaySpotifyRequest, RadioStation } from '../models/music.model';

@Injectable({
  providedIn: 'root',
})
export class MusicApiService {
  constructor(private readonly httpClient: HttpClient) {}

  getRadioStations(): Observable<RadioStation[]> {
    return this.httpClient.get<RadioStation[]>('/api/music/radio-stations');
  }

  searchSpotify(query: string): Observable<MusicSearchResult[]> {
    const params = new HttpParams().set('query', query);

    return this.httpClient.get<MusicSearchResult[]>('/api/music/spotify/search', { params });
  }

  playSpotify(request: PlaySpotifyRequest): Observable<void> {
    return this.httpClient.post<void>('/api/music/spotify/play', request);
  }

  playRadio(request: PlayRadioRequest): Observable<void> {
    return this.httpClient.post<void>('/api/music/radio/play', request);
  }
}
