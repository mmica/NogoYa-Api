import { Component, Inject, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { toSignal } from '@angular/core/rxjs-interop';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { Product } from '@core/models/product.model';
import { Store } from '@core/models/store.model';

export interface ProductDialogData {
  mode: 'create' | 'edit';
  product?: Product;
  stores: Store[];
}

export interface ProductDialogResult {
  storeId: string;
  name: string;
  description?: string | null;
  imageUrl?: string | null;
  sku?: string | null;
  price: number;
  discountPercent: number;
  stock: number;
  isAvailable?: boolean;
}

@Component({
  selector: 'app-product-form-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule,
    MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule,
    MatSlideToggleModule, MatIconModule, MatDividerModule
  ],
  templateUrl: './product-form-dialog.component.html',
  styles: [`
    :host ::ng-deep .mat-mdc-dialog-content { min-width: 560px; max-width: 80vw; }
    @media (max-width: 640px) {
      :host ::ng-deep .mat-mdc-dialog-content { min-width: unset; width: 90vw; }
    }
  `]
})
export class ProductFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(MatDialogRef<ProductFormDialogComponent, ProductDialogResult>);

  protected readonly form;

  /** Live-computed final price after discount, shown as a hint to the user. */
  protected readonly effectivePrice = signal<number | null>(null);

  constructor(@Inject(MAT_DIALOG_DATA) protected readonly data: ProductDialogData) {
    const p = data.product;
    this.form = this.fb.nonNullable.group({
      storeId: [
        { value: p?.storeId ?? '', disabled: data.mode === 'edit' },
        [Validators.required]
      ],
      name: [p?.name ?? '', [Validators.required, Validators.maxLength(150)]],
      sku: [p?.sku ?? ''],
      stock: [p?.stock ?? 0, [Validators.required, Validators.min(0)]],
      price: [p?.price ?? 0, [Validators.required, Validators.min(0)]],
      discountPercent: [p?.discountPercent ?? 0, [Validators.min(0), Validators.max(100)]],
      description: [p?.description ?? ''],
      imageUrl: [p?.imageUrl ?? ''],
      isAvailable: [p?.isAvailable ?? true]
    });

    // Recalculate effective price whenever price or discount change.
    this.form.controls.price.valueChanges.subscribe(() => this.recalc());
    this.form.controls.discountPercent.valueChanges.subscribe(() => this.recalc());
    this.recalc();
  }

  private recalc(): void {
    const price = Number(this.form.controls.price.value) || 0;
    const discount = Number(this.form.controls.discountPercent.value) || 0;
    if (price <= 0 || discount <= 0 || discount > 100) {
      this.effectivePrice.set(null);
      return;
    }
    this.effectivePrice.set(Math.round((price - (price * discount / 100)) * 100) / 100);
  }

  submit(): void {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    const norm = (v: string) => (v?.trim() ? v.trim() : null);

    const result: ProductDialogResult = {
      storeId: raw.storeId,
      name: raw.name.trim(),
      description: norm(raw.description),
      imageUrl: norm(raw.imageUrl),
      sku: norm(raw.sku),
      price: Number(raw.price),
      discountPercent: Number(raw.discountPercent) || 0,
      stock: Number(raw.stock)
    };

    if (this.data.mode === 'edit') {
      result.isAvailable = raw.isAvailable;
    }

    this.ref.close(result);
  }
}
