import { TimerInfo } from './timer.model';

// Mirrors HouseholdPanel.Application.Dashboard.DashboardDto - the only shape the frontend knows about.
export interface Dashboard {
  timestamp: string;
  weather: WeatherInfo;
  weatherLocations: WeatherInfo[];
  indoor: IndoorInfo;
  transport: TransportInfo;
  calendar: CalendarEventInfo[];
  schedule: ScheduleItemInfo[];
  nationalNews: NewsArticleInfo[];
  localNews: NewsArticleInfo[];
  timers: TimerInfo[];
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

