import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { StoreService } from '@core/services/store.service';
import { ProductService } from '@core/services/product.service';
import { Store } from '@core/models/store.model';
import { Product } from '@core/models/product.model';
import { ProductCardComponent } from '@features/products/product-card.component';

@Component({
  selector: 'app-store-detail',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatProgressSpinnerModule, ProductCardComponent],
  template: `
    @if (loading()) {
      <div class="flex justify-center py-10"><mat-spinner diameter="40"></mat-spinner></div>
    } @else if (store()) {
      <section class="bg-white rounded-xl shadow-sm p-6 mb-6">
        <div class="flex items-start gap-4">
          <div class="p-3 rounded-lg bg-brand-50 text-brand-600"><mat-icon>storefront</mat-icon></div>
          <div>
            <h1 class="text-2xl font-semibold">{{ store()!.name }}</h1>
            <p class="text-slate-600 mt-1">{{ store()!.description }}</p>
            <p class="text-slate-500 text-sm mt-2">{{ store()!.address }}</p>
          </div>
        </div>
      </section>
      <h2 class="text-lg font-semibold mb-3">Productos</h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        @for (p of products(); track p.id) {
          <app-product-card [product]="p"></app-product-card>
        } @empty {
          <p class="text-slate-500">Este comercio aún no publicó productos.</p>
        }
      </div>
    }
  `
})
export class StoreDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly stores = inject(StoreService);
  private readonly productsSvc = inject(ProductService);

  protected readonly store = signal<Store | null>(null);
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug')!;
    this.stores.getBySlug(slug).subscribe({
      next: s => {
        this.store.set(s);
        this.productsSvc.getByStore(s.id).subscribe({
          next: ps => this.products.set(ps),
          complete: () => this.loading.set(false)
        });
      },
      error: () => this.loading.set(false)
    });
  }
}
