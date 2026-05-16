import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
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
    CommonModule, ReactiveFormsModule, RouterLink,
    MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatCheckboxModule,
    MatProgressSpinnerModule, MatPaginatorModule,
    ProductCardComponent
  ],
  templateUrl: './product-list.component.html'
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
          isAvailable: true, // public listing: only show products available for sale
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
