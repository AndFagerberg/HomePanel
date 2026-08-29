import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Dashboard } from '../models/dashboard.model';

// Only place in the app allowed to talk HTTP. Views must never call this directly.
@Injectable({
  providedIn: 'root',
})
export class DashboardApiService {
  constructor(private readonly httpClient: HttpClient) {}

  getDashboard(): Observable<Dashboard> {
    return this.httpClient.get<Dashboard>('/api/dashboard');
  }
}
