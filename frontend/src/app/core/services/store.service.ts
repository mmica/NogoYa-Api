import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  Store, CreateStorePayload, UpdateStorePayload, StoreFilter
} from '@core/models/store.model';
import { PagedResult } from '@core/models/product.model';

@Injectable({ providedIn: 'root' })
export class StoreService {
  private readonly http = inject(HttpClient);
  private readonly base = 'stores';

  list(): Observable<Store[]> {
    return this.http.get<Store[]>(this.base);
  }

  search(filter: StoreFilter): Observable<PagedResult<Store>> {
    let params = new HttpParams();
    for (const [k, v] of Object.entries(filter)) {
      if (v !== undefined && v !== null && v !== '') {
        params = params.set(k, String(v));
      }
    }
    return this.http.get<PagedResult<Store>>(`${this.base}/search`, { params });
  }

  getById(id: string): Observable<Store> {
    return this.http.get<Store>(`${this.base}/${id}`);
  }

  getBySlug(slug: string): Observable<Store> {
    return this.http.get<Store>(`${this.base}/by-slug/${slug}`);
  }

  create(payload: CreateStorePayload): Observable<Store> {
    return this.http.post<Store>(this.base, payload);
  }

  update(id: string, payload: UpdateStorePayload): Observable<Store> {
    return this.http.put<Store>(`${this.base}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
