import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Category, Product, ProductOption } from './catalog.models';

export interface CatalogQuery { search: string; categoryIds: string[]; subcategoryIds: string[]; optionIds: string[]; availability: string[]; minimumPrice: number | null; maximumPrice: number | null; minimumDiscount: number | null; sort: string; page: number; pageSize: number; }
export interface CatalogPage { items: Product[]; totalCount: number; page: number; pageSize: number; totalPages: number; }

@Injectable({ providedIn: 'root' })
export class StorefrontCatalogService {
  private readonly baseUrl = '/api';
  constructor(private readonly http: HttpClient) {}
  getProducts(query: CatalogQuery): Observable<CatalogPage> {
    let params = new HttpParams().set('sort', query.sort).set('page', query.page).set('pageSize', query.pageSize);
    if (query.search) params = params.set('search', query.search);
    query.categoryIds.forEach(id => params = params.append('categoryIds', id)); query.subcategoryIds.forEach(id => params = params.append('subcategoryIds', id)); query.optionIds.forEach(id => params = params.append('optionIds', id));
    if (query.availability.length === 1) params = params.set('isAvailable', query.availability[0] === 'available');
    if (query.minimumPrice !== null) params = params.set('minimumPrice', query.minimumPrice); if (query.maximumPrice !== null) params = params.set('maximumPrice', query.maximumPrice); if (query.minimumDiscount !== null) params = params.set('minimumDiscount', query.minimumDiscount);
    return this.http.get<CatalogPage>(`${this.baseUrl}/products`, { params });
  }
  getProduct(id: string): Observable<Product> { return this.http.get<Product>(`${this.baseUrl}/products/${id}`); }
  getCustomProducts(): Observable<Product[]> { return this.http.get<Product[]>(`${this.baseUrl}/products/custom`); }
  getCategories(): Observable<Category[]> { return this.http.get<Category[]>(`${this.baseUrl}/categories`); }
  getCustomCategories(): Observable<Category[]> { return this.http.get<Category[]>(`${this.baseUrl}/categories`, { params: new HttpParams().set('custom', true) }); }
  getOptions(): Observable<ProductOption[]> { return this.http.get<ProductOption[]>(`${this.baseUrl}/product-options`); }
}
