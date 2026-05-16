import { Component, Inject, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { Store } from '@core/models/store.model';

export interface StoreDialogData {
  mode: 'create' | 'edit';
  store?: Store;
}

export interface StoreDialogResult {
  name: string;
  slug?: string;
  description?: string | null;
  logoUrl?: string | null;
  address?: string | null;
  phone?: string | null;
  email?: string | null;
  isActive?: boolean;
}

@Component({
  selector: 'app-store-form-dialog',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatDialogModule,
    MatFormFieldModule, MatInputModule, MatButtonModule,
    MatSlideToggleModule, MatIconModule, MatDividerModule
  ],
  templateUrl: './store-form-dialog.component.html',
  styles: [`
    :host ::ng-deep .mat-mdc-dialog-content { min-width: 520px; max-width: 80vw; }
    @media (max-width: 640px) {
      :host ::ng-deep .mat-mdc-dialog-content { min-width: unset; width: 90vw; }
    }
  `]
})
export class StoreFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly ref = inject(MatDialogRef<StoreFormDialogComponent, StoreDialogResult>);

  protected readonly form;

  constructor(@Inject(MAT_DIALOG_DATA) protected readonly data: StoreDialogData) {
    const s = data.store;
    this.form = this.fb.nonNullable.group({
      name: [s?.name ?? '', [Validators.required, Validators.maxLength(150)]],
      slug: [
        { value: s?.slug ?? '', disabled: data.mode === 'edit' },
        [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)]
      ],
      description: [s?.description ?? ''],
      phone: [s?.phone ?? ''],
      email: [s?.email ?? '', [Validators.email]],
      address: [s?.address ?? ''],
      logoUrl: [s?.logoUrl ?? ''],
      isActive: [s?.isActive ?? true]
    });
  }

  submit(): void {
    if (this.form.invalid) return;
    const raw = this.form.getRawValue();
    // Normalize empty strings to null so the backend stores them as NULL.
    const norm = (v: string) => (v?.trim() ? v.trim() : null);

    const result: StoreDialogResult = {
      name: raw.name.trim(),
      description: norm(raw.description),
      logoUrl: norm(raw.logoUrl),
      address: norm(raw.address),
      phone: norm(raw.phone),
      email: norm(raw.email)
    };

    if (this.data.mode === 'create') {
      result.slug = raw.slug.trim().toLowerCase();
    } else {
      result.isActive = raw.isActive;
    }

    this.ref.close(result);
  }
}
