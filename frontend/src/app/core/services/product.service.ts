import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PagedResult, Product, ProductFilter } from '@core/models/product.model';

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

  getById(id: string): Observable<Product> { return this.http.get<Product>(`${this.base}/${id}`); }
  getByStore(storeId: string): Observable<Product[]> {
    return this.http.get<Product[]>(`${this.base}/by-store/${storeId}`);
  }
}
