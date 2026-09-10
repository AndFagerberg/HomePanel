export interface MusicSearchResult {
  id: string;
  title: string;
  subtitle: string;
  uri: string;
  type: string;
  imageUrl?: string;
}

export interface RadioStation {
  id: string;
  name: string;
}

export interface PlaySpotifyRequest {
  uri: string;
}

export interface PlayRadioRequest {
  stationId: string;
}
