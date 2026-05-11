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
  templateUrl: './store-detail.component.html'
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
