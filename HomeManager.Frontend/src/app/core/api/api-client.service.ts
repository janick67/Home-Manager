import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiClientService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = environment.apiBaseUrl.replace(/\/$/, '');

  get<T>(path: string, params?: Record<string, string | number | boolean>): Observable<T> {
    return this.http.get<T>(this.buildUrl(path), {
      params: this.buildParams(params)
    });
  }

  post<T>(path: string, body: unknown): Observable<T> {
    return this.http.post<T>(this.buildUrl(path), body);
  }

  put<T>(path: string, body: unknown): Observable<T> {
    return this.http.put<T>(this.buildUrl(path), body);
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(this.buildUrl(path));
  }

  private buildUrl(path: string): string {
    return `${this.apiBaseUrl}/${path.replace(/^\//, '')}`;
  }

  private buildParams(params?: Record<string, string | number | boolean>): HttpParams | undefined {
    if (!params) {
      return undefined;
    }

    return Object.entries(params).reduce(
      (httpParams, [key, value]) => httpParams.set(key, String(value)),
      new HttpParams()
    );
  }
}
