import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  CreateProductPayload, PagedResult, Product, ProductFilter, UpdateProductPayload
} from '@core/models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly base = 'products';

  search(filter: ProductFilter): Observable<PagedResult<Product>> {
    let params = new HttpParams();
    for (const [k, v] of Object.entries(filter)) {
      if (v !== undefined && v !== null && v !== '') params = params.set(k, String(v));
    }
    return this.http.get<PagedResult<Product>>(this.base, { params });
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.base}/${id}`);
  }

  getByStore(storeId: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.base}/by-store/${storeId}`);
  }

  create(payload: CreateProductPayload): Observable<Product> {
    return this.http.post<Product>(this.base, payload);
  }

  update(id: string, payload: UpdateProductPayload): Observable<Product> {
    return this.http.put<Product>(`${this.base}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
