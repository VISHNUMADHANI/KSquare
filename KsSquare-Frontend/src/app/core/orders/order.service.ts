import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
export interface OrderLine { personalizationNote?: string | null; productId: string; quantity: number; optionIds: string[]; personalizedName: string | null; }
export interface CheckoutRequest { lines: OrderLine[]; customRequestId: string | null; }
export interface OrderItem { personalizationNote?: string | null; productId: string | null; name: string; quantity: number; unitPrice: number; total: number; details: string[]; isReturnable?: boolean | null; returnRestriction?: string | null; canRequestReturn?: boolean; canIncludeInReturn?: boolean; }
export interface CheckoutPreview { items: OrderItem[]; total: number; currency: string; }
export interface CustomerOrder extends CheckoutPreview {
 id: string; orderNumber: string; customerId: string; customerName: string; customerEmail: string; customerPhone: string;
 courier?: string | null; trackingNumber?: string | null; trackingUrl?: string | null;
 deliveredAt?: string | null; returnDeadline?: string | null; returns?: ReturnRequest[]; deliveryAddress: string; status: string; paymentStatus: string; isDemo: boolean; paymentReference: string; createdAt: string; version: number;
}
export interface ReturnSelection { itemIndex: number; quantity: number; }
export interface ReturnRequest { items?: ReturnSelection[]; customerItems?: ReturnSelection[]; itemIndices?: number[]; id: string; itemIndex: number; reason: string; description: string; requestedAt: string; status: string; adminNote: string | null; refundAmount: number | null; refundReference: string | null; photos: { url: string | null; contentType: string }[]; history: { items?: ReturnSelection[]; itemIndices?: number[]; status: string; at: string; note: string; refundAmount: number | null }[]; }
export interface OrderPage { items: CustomerOrder[]; totalCount: number; page: number; pageSize: number; }
@Injectable({ providedIn: 'root' })
export class OrderService {
 private readonly http = inject(HttpClient);
 private readonly base = '/api';
 returns(page:number,status:string){return this.http.get<OrderPage>(this.base+'/admin/orders/returns',{params:new HttpParams().set('page',page).set('status',status)});}
 get(id: string, admin: boolean) { return this.http.get<CustomerOrder>(this.base + (admin ? '/admin/orders/' : '/orders/') + id); }
 requestReturn(id: string, body: FormData) { return this.http.post<CustomerOrder>(this.base + '/orders/' + id + '/returns', body); }
 reviewReturn(id: string, returnId: string, body: { action: string; note: string; refundAmount: number | null; inspected: boolean; contactConfirmed?: boolean; items?: ReturnSelection[]; version: number }) { return this.http.put<CustomerOrder>(this.base + '/admin/orders/' + id + '/returns/' + returnId, body); }
 preview(checkout: CheckoutRequest) { return this.http.post<CheckoutPreview>(this.base + '/orders/preview', checkout); }
 placeDemo(body: { checkoutKey: string; checkout: CheckoutRequest; expectedTotal: number; deliveryAddress: string }) { return this.http.post<CustomerOrder>(this.base + '/orders/demo', body); }
 list(admin: boolean, page: number, status: string) { return this.http.get<OrderPage>(this.base + (admin ? '/admin/orders' : '/orders'), { params: new HttpParams().set('page',page).set('status',status) }); }
 update(id: string, status: string, version: number, shipment: {courier?:string;trackingNumber?:string;deliveredAt?:string}={}) { return this.http.put<CustomerOrder>(this.base + '/admin/orders/' + id + '/status', { status, version, ...shipment }); }
}
