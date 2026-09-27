import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateCustomRequestResult, CustomJewelryRequest, CustomRequestAccess } from './custom-request.models';

@Injectable({ providedIn: 'root' })
export class CustomRequestService {
  private readonly baseUrl = '/api'; private readonly storageKey = 'ksquare_custom_requests';
  constructor(private readonly http: HttpClient) {}
  create(body: FormData): Observable<CreateCustomRequestResult> { return this.http.post<CreateCustomRequestResult>(`${this.baseUrl}/custom-requests`, body); }
  getCustomer(access: CustomRequestAccess): Observable<CustomJewelryRequest> { return this.http.get<CustomJewelryRequest>(`${this.baseUrl}/custom-requests/${access.id}`, { headers: new HttpHeaders().set('X-Custom-Request-Token', access.token) }); }
  getAdminAll(): Observable<CustomJewelryRequest[]> { return this.http.get<CustomJewelryRequest[]>(`${this.baseUrl}/admin/custom-requests`); }
  getAdmin(id: string): Observable<CustomJewelryRequest> { return this.http.get<CustomJewelryRequest>(`${this.baseUrl}/admin/custom-requests/${id}`); }
  setQuote(id: string, fixedPrice: number): Observable<CustomJewelryRequest> { return this.http.put<CustomJewelryRequest>(`${this.baseUrl}/admin/custom-requests/${id}/quote`, { fixedPrice }); }
  updateStatus(id: string, status: 'Contacted' | 'Cancelled'): Observable<CustomJewelryRequest> { return this.http.put<CustomJewelryRequest>(`${this.baseUrl}/admin/custom-requests/${id}/status`, { status }); }
  remember(result: CreateCustomRequestResult): void { const current = this.accessList().filter(item => item.id !== result.request.id); current.unshift({ id: result.request.id, token: result.accessToken, requestNumber: result.request.requestNumber }); localStorage.setItem(this.storageKey, JSON.stringify(current)); }
  accessList(): CustomRequestAccess[] { try { const value = JSON.parse(localStorage.getItem(this.storageKey) ?? '[]'); return Array.isArray(value) ? value : []; } catch { return []; } }
}
