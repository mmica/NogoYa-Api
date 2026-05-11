import { Injectable, computed, signal } from '@angular/core';
import { Product } from '@core/models/product.model';

export interface CartLine {
  product: Product;
  quantity: number;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  private readonly storageKey = 'nogoya_cart_v1';
  private readonly _lines = signal<CartLine[]>(this.load());

  readonly lines = this._lines.asReadonly();
  readonly itemCount = computed(() => this._lines().reduce((n, l) => n + l.quantity, 0));
  readonly total = computed(() =>
    this._lines().reduce((sum, l) => sum + l.product.effectivePrice * l.quantity, 0));
  readonly storeId = computed(() => this._lines()[0]?.product.storeId ?? null);

  add(product: Product, quantity = 1): void {
    if (this.storeId() && this.storeId() !== product.storeId) {
      this._lines.set([{ product, quantity }]);
    } else {
      const lines = [...this._lines()];
      const existing = lines.find(l => l.product.id === product.id);
      if (existing) existing.quantity += quantity;
      else lines.push({ product, quantity });
      this._lines.set(lines);
    }
    this.persist();
  }

  updateQty(productId: string, quantity: number): void {
    const lines = this._lines().map(l => l.product.id === productId ? { ...l, quantity } : l);
    this._lines.set(lines.filter(l => l.quantity > 0));
    this.persist();
  }

  remove(productId: string): void {
    this._lines.set(this._lines().filter(l => l.product.id !== productId));
    this.persist();
  }

  clear(): void { this._lines.set([]); this.persist(); }

  private persist(): void {
    try { localStorage.setItem(this.storageKey, JSON.stringify(this._lines())); } catch { /* ignore */ }
  }
  private load(): CartLine[] {
    try { const raw = localStorage.getItem(this.storageKey); return raw ? JSON.parse(raw) as CartLine[] : []; }
    catch { return []; }
  }
}
