import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, combineLatest, debounceTime, startWith, switchMap, takeUntil } from 'rxjs';
import { ProductService } from '@core/services/product.service';
import { StoreService } from '@core/services/store.service';
import { PagedResult, Product } from '@core/models/product.model';
import { Store } from '@core/models/store.model';
import {
  ProductDialogResult, ProductFormDialogComponent
} from './product-form-dialog.component';

@Component({
  selector: 'app-products-admin',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatTableModule, MatPaginatorModule,
    MatProgressBarModule, MatChipsModule, MatTooltipModule, MatMenuModule,
    MatDialogModule
  ],
  templateUrl: './products.component.html'
})
export class ProductsAdminComponent implements OnInit, OnDestroy {
  private readonly productsSvc = inject(ProductService);
  private readonly storesSvc = inject(StoreService);
  private readonly route = inject(ActivatedRoute);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);
  private readonly destroy$ = new Subject<void>();
  private readonly refresh$ = new Subject<void>();

  protected readonly columns = ['product', 'store', 'price', 'stock', 'status', 'actions'];

  protected readonly result = signal<PagedResult<Product> | null>(null);
  protected readonly loading = signal(false);
  protected readonly stores = signal<Store[]>([]);
  protected readonly loadingStores = signal(true);

  protected readonly search = new FormControl('', { nonNullable: true });
  protected readonly storeFilter = new FormControl<string>('', { nonNullable: true });
  protected readonly availabilityFilter = new FormControl<string | boolean>('', { nonNullable: true });

  protected readonly pageSize = 25;
  private page = 1;

  ngOnInit(): void {
    // Pre-fill the store filter from `?storeId=...` query param (used by
    // shortcuts from the stores admin row menu).
    const initialStoreId = this.route.snapshot.queryParamMap.get('storeId');
    if (initialStoreId) {
      this.storeFilter.setValue(initialStoreId, { emitEvent: false });
    }

    // Load store list once for the dropdown filter and the create dialog.
    this.storesSvc.list().subscribe({
      next: list => this.stores.set(list),
      complete: () => this.loadingStores.set(false),
      error: () => this.loadingStores.set(false)
    });

    // Any filter change triggers a debounce + page reset.
    combineLatest([
      this.search.valueChanges.pipe(startWith(this.search.value)),
      this.storeFilter.valueChanges.pipe(startWith(this.storeFilter.value)),
      this.availabilityFilter.valueChanges.pipe(startWith(this.availabilityFilter.value))
    ])
      .pipe(debounceTime(250), takeUntil(this.destroy$))
      .subscribe(() => {
        this.page = 1;
        this.refresh$.next();
      });

    this.refresh$
      .pipe(
        switchMap(() => {
          this.loading.set(true);
          const availability = this.availabilityFilter.value;
          return this.productsSvc.search({
            search: this.search.value || undefined,
            storeId: this.storeFilter.value || undefined,
            isAvailable: availability === '' ? undefined : Boolean(availability),
            page: this.page,
            pageSize: this.pageSize
          });
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: r => { this.result.set(r); this.loading.set(false); },
        error: () => this.loading.set(false)
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onPage(e: PageEvent): void {
    this.page = e.pageIndex + 1;
    this.refresh$.next();
  }

  openCreate(): void {
    const ref = this.dialog.open<ProductFormDialogComponent, any, ProductDialogResult>(
      ProductFormDialogComponent,
      { data: { mode: 'create', stores: this.stores() } }
    );
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.productsSvc.create({
        storeId: result.storeId,
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
          this.refresh$.next();
        }
      });
    });
  }

  openEdit(product: Product): void {
    const ref = this.dialog.open<ProductFormDialogComponent, any, ProductDialogResult>(
      ProductFormDialogComponent,
      { data: { mode: 'edit', product, stores: this.stores() } }
    );
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.productsSvc.update(product.id, {
        name: result.name,
        description: result.description,
        imageUrl: result.imageUrl,
        sku: result.sku,
        price: result.price,
        discountPercent: result.discountPercent,
        stock: result.stock,
        isAvailable: result.isAvailable ?? product.isAvailable
      }).subscribe({
        next: () => {
          this.snack.open(`Producto actualizado.`, 'Cerrar', { duration: 3000 });
          this.refresh$.next();
        }
      });
    });
  }

  toggleAvailable(product: Product): void {
    this.productsSvc.update(product.id, {
      name: product.name,
      description: product.description,
      imageUrl: product.imageUrl,
      sku: product.sku,
      price: product.price,
      discountPercent: product.discountPercent,
      stock: product.stock,
      isAvailable: !product.isAvailable
    }).subscribe({
      next: () => {
        this.snack.open(
          `Producto ${product.isAvailable ? 'marcado como no disponible' : 'marcado como disponible'}.`,
          'Cerrar', { duration: 3000 }
        );
        this.refresh$.next();
      }
    });
  }

  remove(product: Product): void {
    const confirmed = window.confirm(
      `¿Eliminar "${product.name}"? Es un soft-delete: queda registrado en el historial de auditoría.`
    );
    if (!confirmed) return;
    this.productsSvc.delete(product.id).subscribe({
      next: () => {
        this.snack.open(`Producto "${product.name}" eliminado.`, 'Cerrar', { duration: 3000 });
        this.refresh$.next();
      }
    });
  }
}
