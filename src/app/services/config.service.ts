import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../models/api.interface';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  private apiUrl = 'http://192.168.1.82:4343/api/Display/GetDisplay';

  constructor(private http: HttpClient) {}

  getFields(): Observable<ApiResponse> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/x-www-form-urlencoded'
    });
    return this.http.get<ApiResponse>(this.apiUrl, { headers });
  }
}