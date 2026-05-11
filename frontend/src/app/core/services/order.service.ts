import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { CreateOrderPayload, Order } from '@core/models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly base = 'orders';

  create(payload: CreateOrderPayload): Observable<Order> {
    return this.http.post<Order>(this.base, payload);
  }
  getById(id: string): Observable<Order> { return this.http.get<Order>(`${this.base}/${id}`); }
  cancel(id: string, reason?: string): Observable<void> {
    const qs = reason ? `?reason=${encodeURIComponent(reason)}` : '';
    return this.http.post<void>(`${this.base}/${id}/cancel${qs}`, {});
  }
}
