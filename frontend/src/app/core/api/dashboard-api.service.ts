import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AirPatrolInfo,
  CalendarSection,
  Dashboard,
  IndoorInfo,
  NewsSection,
  TransportInfo,
  WeatherSection,
} from '../models/dashboard.model';

// Only place in the app allowed to talk HTTP. Views must never call this directly.
@Injectable({
  providedIn: 'root',
})
export class DashboardApiService {
  constructor(private readonly httpClient: HttpClient) {}

  getDashboard(): Observable<Dashboard> {
    return this.httpClient.get<Dashboard>('/api/dashboard');
  }

  getWeather(): Observable<WeatherSection> {
    return this.httpClient.get<WeatherSection>('/api/dashboard/weather');
  }

  getTransport(): Observable<TransportInfo> {
    return this.httpClient.get<TransportInfo>('/api/dashboard/transport');
  }

  getCalendar(): Observable<CalendarSection> {
    return this.httpClient.get<CalendarSection>('/api/dashboard/calendar');
  }

  getNews(): Observable<NewsSection> {
    return this.httpClient.get<NewsSection>('/api/dashboard/news');
  }

  getCabin(): Observable<AirPatrolInfo | null> {
    return this.httpClient.get<AirPatrolInfo | null>('/api/dashboard/cabin');
  }

  getIndoor(): Observable<IndoorInfo> {
    return this.httpClient.get<IndoorInfo>('/api/dashboard/indoor');
  }
}

