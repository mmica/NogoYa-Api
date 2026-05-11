import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { StoreService } from '@core/services/store.service';
import { ProductImportService } from '@core/services/product-import.service';
import { Store } from '@core/models/store.model';
import { ImportMode, ProductImportResult } from '@core/models/product-import.model';

@Component({
  selector: 'app-import-products',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule, MatFormFieldModule,
    MatSelectModule, MatProgressBarModule, MatTableModule, MatDividerModule
  ],
  templateUrl: './import-products.component.html'
})
export class ImportProductsComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly storesSvc = inject(StoreService);
  private readonly importSvc = inject(ProductImportService);
  private readonly snack = inject(MatSnackBar);

  protected readonly stores = signal<Store[]>([]);
  protected readonly selectedFile = signal<File | null>(null);
  protected readonly uploading = signal(false);
  protected readonly progress = signal(0);
  protected readonly result = signal<ProductImportResult | null>(null);
  protected readonly errorCols = ['rowNumber', 'sku', 'message'];

  protected readonly form = this.fb.nonNullable.group({
    storeId: ['', Validators.required],
    mode: ['Upsert' as ImportMode, Validators.required]
  });

  ngOnInit(): void {
    this.storesSvc.list().subscribe(list => this.stores.set(list));
  }

  onFileSelected(ev: Event): void {
    const file = (ev.target as HTMLInputElement).files?.[0] ?? null;
    this.setFile(file);
  }

  onDrop(ev: DragEvent): void {
    ev.preventDefault();
    this.setFile(ev.dataTransfer?.files?.[0] ?? null);
  }

  private setFile(file: File | null): void {
    if (!file) return;
    if (!/\.xlsx$/i.test(file.name)) {
      this.snack.open('Formato no soportado. Use .xlsx.', 'Cerrar', { duration: 4000 });
      return;
    }
    this.selectedFile.set(file);
    this.result.set(null);
  }

  submit(): void {
    if (this.form.invalid || !this.selectedFile()) return;
    this.uploading.set(true);
    this.progress.set(0);
    this.result.set(null);

    const { storeId, mode } = this.form.getRawValue();
    this.importSvc.upload(storeId, this.selectedFile()!, mode).subscribe({
      next: p => {
        this.progress.set(p.progress);
        if (p.result) {
          this.result.set(p.result);
          const { imported, updated, failed } = p.result;
          this.snack.open(
            `Importación finalizada. ${imported} nuevos, ${updated} actualizados, ${failed} errores.`,
            'Cerrar', { duration: 6000 });
        }
      },
      complete: () => this.uploading.set(false),
      error: () => this.uploading.set(false)
    });
  }
}
