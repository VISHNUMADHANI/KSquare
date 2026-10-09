import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Category, Product, ProductMedia, ProductOption, SaveCategory, SaveProduct, SaveProductOption } from './catalog.models';

@Injectable({ providedIn: 'root' })
export class AdminCatalogService {
  private readonly baseUrl = '/api/admin';
  constructor(private readonly http: HttpClient) {}
  getCategories(): Observable<Category[]> { return this.http.get<Category[]>(`${this.baseUrl}/categories`); }
  getCategory(id: string): Observable<Category> { return this.http.get<Category>(`${this.baseUrl}/categories/${id}`); }
  createCategory(request: SaveCategory): Observable<Category> { return this.http.post<Category>(`${this.baseUrl}/categories`, request); }
  updateCategory(id: string, request: SaveCategory): Observable<Category> { return this.http.put<Category>(`${this.baseUrl}/categories/${id}`, request); }
  deleteCategory(id: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/categories/${id}`); }
  uploadCategoryImage(id: string, file: File): Observable<Category> { const body = new FormData(); body.append('file', file); return this.http.post<Category>(`${this.baseUrl}/categories/${id}/image`, body); }
  deleteCategoryImage(id: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/categories/${id}/image`); }
  getProductOptions(): Observable<ProductOption[]> { return this.http.get<ProductOption[]>(`${this.baseUrl}/product-options`); }
  createProductOption(request: SaveProductOption): Observable<ProductOption> { return this.http.post<ProductOption>(`${this.baseUrl}/product-options`, request); }
  updateProductOption(id: string, request: SaveProductOption): Observable<ProductOption> { return this.http.put<ProductOption>(`${this.baseUrl}/product-options/${id}`, request); }
  deleteProductOption(id: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/product-options/${id}`); }
  uploadProductOptionImage(id: string, file: File): Observable<ProductOption> { const body = new FormData(); body.append('file', file); return this.http.post<ProductOption>(`${this.baseUrl}/product-options/${id}/image`, body); }
  uploadProductOptionSizeGuide(id: string, file: File): Observable<ProductOption> { const body = new FormData(); body.append('file', file); return this.http.post<ProductOption>(this.baseUrl + '/product-options/' + id + '/size-guide', body); }
  deleteProductOptionSizeGuide(id: string): Observable<void> { return this.http.delete<void>(this.baseUrl + '/product-options/' + id + '/size-guide'); }
  deleteProductOptionImage(id: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/product-options/${id}/image`); }
  getProducts(): Observable<Product[]> { return this.http.get<Product[]>(`${this.baseUrl}/products`); }
  getProduct(id: string): Observable<Product> { return this.http.get<Product>(`${this.baseUrl}/products/${id}`); }
  createProduct(request: SaveProduct): Observable<Product> { return this.http.post<Product>(`${this.baseUrl}/products`, request); }
  updateProduct(id: string, request: SaveProduct): Observable<Product> { return this.http.put<Product>(`${this.baseUrl}/products/${id}`, request); }
  deleteProduct(id: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/products/${id}`); }
  uploadMedia(productId: string, file: File, altText: string): Observable<ProductMedia> {
    const body = new FormData(); body.append('file', file); body.append('altText', altText);
    return this.http.post<ProductMedia>(`${this.baseUrl}/products/${productId}/media`, body);
  }
  downloadMedia(productId: string, mediaId: string): Observable<Blob> { return this.http.get(`${this.baseUrl}/products/${productId}/media/${mediaId}/download`, { responseType: 'blob' }); }
  deleteMedia(productId: string, mediaId: string): Observable<void> { return this.http.delete<void>(`${this.baseUrl}/products/${productId}/media/${mediaId}`); }
}
