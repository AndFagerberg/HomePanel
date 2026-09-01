// Mirrors HouseholdPanel.Application.Dashboard.DashboardDto - the only shape the frontend knows about.
export interface Dashboard {
  timestamp: string;
  weather: WeatherInfo;
  weatherLocations: WeatherInfo[];
  indoor: IndoorInfo;
  transport: TransportInfo;
  calendar: CalendarEventInfo[];
  schedule: ScheduleItemInfo[];
}

export interface WeatherInfo {
  name: string;
  temperature: number;
  minimumTemperature: number;
  maximumTemperature: number;
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
