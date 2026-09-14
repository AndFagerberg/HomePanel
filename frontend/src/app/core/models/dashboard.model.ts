import { TimerInfo } from './timer.model';

// Mirrors HouseholdPanel.Application.Dashboard.DashboardDto - the only shape the frontend knows about.
export interface Dashboard {
  timestamp: string;
  weather: WeatherInfo;
  weatherLocations: WeatherInfo[];
  indoor: IndoorInfo;
  airPatrol: AirPatrolInfo | null;
  transport: TransportInfo;
  calendar: CalendarEventInfo[];
  schedule: ScheduleItemInfo[];
  nationalNews: NewsArticleInfo[];
  localNews: NewsArticleInfo[];
  timers: TimerInfo[];
  music: MusicPlaybackInfo | null;
}

export interface MusicPlaybackInfo {
  title: string;
  artist: string;
  album: string | null;
  imageUrl?: string;
  isPlaying: boolean;
}

export interface WeatherInfo {
  name: string;
  temperature: number;
  minimumTemperature: number;
  maximumTemperature: number;
  tomorrowMinimumTemperature?: number;
  tomorrowMaximumTemperature?: number;
  tomorrowSymbol?: string;
  symbol: string;
  precipitationProbability: number;
  windSpeed: number;
}

export interface IndoorInfo {
  temperature: number;
  humidity: number;
}

export interface AirPatrolInfo {
  name: string;
  temperature: number;
  humidity: number | null;
  power: boolean;
  mode: string;
  targetTemperature: number | null;
  fanSpeed: string;
  swing: boolean;
  updatedAt: string;
  history: AirPatrolHistoryPoint[];
}

export interface AirPatrolHistoryPoint {
  timestamp: string;
  temperature: number;
  humidity: number | null;
  targetTemperature: number | null;
}

export interface TransportInfo {
  stopName: string;
  departures: DepartureInfo[];
}

export interface DepartureInfo {
  departure: string;
  destination: string;
  line: string;
  minutes: number;
}

export interface CalendarEventInfo {
  start: string;
  title: string;
}

export interface ScheduleItemInfo {
  start: string;
  title: string;
}

export interface NewsArticleInfo {
  title: string;
  summary: string;
  source: string;
  publishedAt: string;
  url: string;
}

// Per-domain payloads returned by the granular /api/dashboard/* endpoints (polled at different intervals).
export interface WeatherSection {
  primary: WeatherInfo;
  locations: WeatherInfo[];
}

export interface CalendarSection {
  calendar: CalendarEventInfo[];
  schedule: ScheduleItemInfo[];
}

export interface NewsSection {
  nationalNews: NewsArticleInfo[];
  localNews: NewsArticleInfo[];
}


