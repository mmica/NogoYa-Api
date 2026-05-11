import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Store, CreateStorePayload } from '@core/models/store.model';

@Injectable({ providedIn: 'root' })
export class StoreService {
  private readonly http = inject(HttpClient);
  private readonly base = 'stores';

  list(): Observable<Store[]> { return this.http.get<Store[]>(this.base); }
  getById(id: string): Observable<Store> { return this.http.get<Store>(`${this.base}/${id}`); }
  getBySlug(slug: string): Observable<Store> { return this.http.get<Store>(`${this.base}/by-slug/${slug}`); }
  create(payload: CreateStorePayload): Observable<Store> { return this.http.post<Store>(this.base, payload); }
}
