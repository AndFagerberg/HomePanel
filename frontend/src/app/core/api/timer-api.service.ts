import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateTimerRequest, TimerInfo } from '../models/timer.model';

@Injectable({
  providedIn: 'root',
})
export class TimerApiService {
  constructor(private readonly httpClient: HttpClient) {}

  getTimers(): Observable<TimerInfo[]> {
    return this.httpClient.get<TimerInfo[]>('/api/timers');
  }

  createTimer(request: CreateTimerRequest): Observable<TimerInfo> {
    return this.httpClient.post<TimerInfo>('/api/timers', request);
  }

  cancelTimer(id: string): Observable<void> {
    return this.httpClient.delete<void>(`/api/timers/${id}`);
  }
}
