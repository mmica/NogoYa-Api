import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { debounceTime, startWith, switchMap } from 'rxjs';
import { ProductService } from '@core/services/product.service';
import { PagedResult, Product } from '@core/models/product.model';
import { ProductCardComponent } from './product-card.component';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatCheckboxModule,
    MatProgressSpinnerModule, MatPaginatorModule,
    ProductCardComponent
  ],
  template: `
    <header class="mb-6">
      <h1 class="text-2xl font-semibold">Productos</h1>
      <p class="text-slate-600">Explorá todos los productos publicados en Nogo-Ya.</p>
    </header>
    <div class="flex flex-wrap gap-3 mb-5">
      <mat-form-field appearance="outline" class="flex-1 min-w-[240px]">
        <mat-label>Buscar</mat-label>
        <input matInput placeholder="Ej: manzanas, panificados…" [formControl]="search" />
      </mat-form-field>
      <mat-checkbox [formControl]="onSale" class="self-center">Sólo con descuento</mat-checkbox>
    </div>

    @if (loading()) {
      <div class="flex justify-center py-10"><mat-spinner diameter="40"></mat-spinner></div>
    } @else {
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        @for (p of result()?.items ?? []; track p.id) {
          <app-product-card [product]="p"></app-product-card>
        } @empty {
          <p class="text-slate-500">No encontramos productos con esos filtros.</p>
        }
      </div>
      @if ((result()?.totalItems ?? 0) > 0) {
        <mat-paginator class="mt-4 bg-transparent"
          [length]="result()!.totalItems"
          [pageIndex]="(result()!.page - 1)"
          [pageSize]="result()!.pageSize"
          [pageSizeOptions]="[12, 24, 48]"
          (page)="onPage($event)">
        </mat-paginator>
      }
    }
  `
})
export class ProductListComponent implements OnInit {
  private readonly products = inject(ProductService);
  protected readonly search = new FormControl('', { nonNullable: true });
  protected readonly onSale = new FormControl(false, { nonNullable: true });
  protected readonly result = signal<PagedResult<Product> | null>(null);
  protected readonly loading = signal(true);

  private page = 1;
  private pageSize = 12;

  ngOnInit(): void {
    this.search.valueChanges.pipe(
      startWith(this.search.value),
      debounceTime(300),
      switchMap(() => {
        this.loading.set(true);
        return this.products.search({
          search: this.search.value || undefined,
          onSale: this.onSale.value || undefined,
          page: this.page,
          pageSize: this.pageSize
        });
      })
    ).subscribe({
      next: r => { this.result.set(r); this.loading.set(false); },
      error: () => this.loading.set(false)
    });

    this.onSale.valueChanges.subscribe(() => this.search.updateValueAndValidity({ emitEvent: true }));
  }

  onPage(e: PageEvent): void {
    this.page = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.search.updateValueAndValidity({ emitEvent: true });
  }
}
