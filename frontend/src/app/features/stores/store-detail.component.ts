import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { StoreService } from '@core/services/store.service';
import { ProductService } from '@core/services/product.service';
import { Store } from '@core/models/store.model';
import { Product } from '@core/models/product.model';
import { ProductCardComponent } from '@features/products/product-card.component';
import {
  ProductDialogResult,
  ProductFormDialogComponent
} from '@features/settings/products/product-form-dialog.component';

@Component({
  selector: 'app-store-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDialogModule,
    ProductCardComponent
  ],
  templateUrl: './store-detail.component.html'
})
export class StoreDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly stores = inject(StoreService);
  private readonly productsSvc = inject(ProductService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);

  protected readonly store = signal<Store | null>(null);
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);

  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug')!;
    this.stores.getBySlug(slug).subscribe({
      next: s => {
        this.store.set(s);
        this.reloadProducts(s.id);
      },
      error: () => this.loading.set(false)
    });
  }

  /**
   * Open the product creation dialog with the current store pre-selected and locked
   * (the dropdown shows only this store so it cannot be reassigned by mistake).
   */
  openCreateProduct(): void {
    const current = this.store();
    if (!current) return;

    const ref = this.dialog.open<ProductFormDialogComponent, any, ProductDialogResult>(
      ProductFormDialogComponent,
      { data: { mode: 'create', stores: [current] } }
    );

    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.productsSvc.create({
        storeId: current.id,
        name: result.name,
        description: result.description,
        imageUrl: result.imageUrl,
        sku: result.sku,
        price: result.price,
        discountPercent: result.discountPercent,
        stock: result.stock
      }).subscribe({
        next: created => {
          this.snack.open(`Producto "${created.name}" creado.`, 'Cerrar', { duration: 4000 });
          this.reloadProducts(current.id);
        }
      });
    });
  }

  private reloadProducts(storeId: string): void {
    this.loading.set(true);
    this.productsSvc.getByStore(storeId).subscribe({
      next: ps => this.products.set(ps),
      complete: () => this.loading.set(false),
      error: () => this.loading.set(false)
    });
  }
}
