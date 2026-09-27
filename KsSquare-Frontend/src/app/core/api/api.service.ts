import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private readonly http: HttpClient) {}

  getProducts(): Observable<unknown[]> {
    return this.http.get<unknown[]>('/api/products');
  }
}
