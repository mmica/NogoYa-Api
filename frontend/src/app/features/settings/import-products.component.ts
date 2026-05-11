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
  template: `
    <div class="mb-6 flex items-center gap-2 text-sm text-slate-500">
      <a routerLink="/settings" class="hover:underline">Configuración</a>
      <mat-icon class="!text-base !w-4 !h-4">chevron_right</mat-icon>
      <span class="text-slate-700">Importar productos</span>
    </div>
    <header class="mb-6">
      <h1 class="text-2xl font-semibold">Importar productos desde Excel</h1>
      <p class="text-slate-600">
        Cargá un archivo <strong>.xlsx</strong> con tu catálogo. Columnas esperadas:
        <code class="bg-slate-100 px-1 rounded">Sku</code>,
        <code class="bg-slate-100 px-1 rounded">Name</code>,
        <code class="bg-slate-100 px-1 rounded">Description</code>,
        <code class="bg-slate-100 px-1 rounded">Price</code>,
        <code class="bg-slate-100 px-1 rounded">DiscountPercent</code>,
        <code class="bg-slate-100 px-1 rounded">Stock</code>,
        <code class="bg-slate-100 px-1 rounded">ImageUrl</code>,
        <code class="bg-slate-100 px-1 rounded">IsAvailable</code>.
      </p>
    </header>

    <mat-card class="max-w-3xl">
      <mat-card-content class="p-6">
        <form [formGroup]="form" (ngSubmit)="submit()" class="space-y-4">
          <mat-form-field appearance="outline" class="w-full">
            <mat-label>Comercio</mat-label>
            <mat-select formControlName="storeId" required>
              @for (s of stores(); track s.id) {
                <mat-option [value]="s.id">{{ s.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline" class="w-full">
            <mat-label>Modo de importación</mat-label>
            <mat-select formControlName="mode" required>
              <mat-option value="Upsert">Actualizar existentes + insertar nuevos</mat-option>
              <mat-option value="InsertOnly">Solo insertar nuevos (ignorar SKU duplicado)</mat-option>
            </mat-select>
          </mat-form-field>

          <div (dragover)="$event.preventDefault()" (drop)="onDrop($event)"
               class="border-2 border-dashed border-slate-300 rounded-lg p-8 text-center bg-slate-50 hover:border-brand-500 transition-colors">
            <mat-icon class="!text-5xl !w-16 !h-16 !text-slate-400">cloud_upload</mat-icon>
            <p class="mt-2 text-slate-700">Arrastrá aquí tu archivo .xlsx o</p>
            <button type="button" mat-stroked-button color="primary" (click)="fileInput.click()">
              Elegir archivo
            </button>
            <input type="file" hidden #fileInput accept=".xlsx,.xls,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                   (change)="onFileSelected($event)" />
            @if (selectedFile()) {
              <p class="mt-3 text-sm text-slate-700">
                <mat-icon class="align-middle !text-base !w-4 !h-4">description</mat-icon>
                {{ selectedFile()!.name }} ({{ (selectedFile()!.size / 1024) | number:'1.0-0' }} KB)
              </p>
            }
          </div>

          @if (uploading()) {
            <div>
              <mat-progress-bar mode="determinate" [value]="progress()"></mat-progress-bar>
              <p class="text-xs text-slate-500 mt-1">Subiendo archivo… {{ progress() }}%</p>
            </div>
          }

          <div class="flex justify-end gap-2 pt-2">
            <a mat-button routerLink="/settings">Cancelar</a>
            <button mat-flat-button color="primary"
                    [disabled]="form.invalid || !selectedFile() || uploading()">
              <mat-icon>upload</mat-icon> Importar
            </button>
          </div>
        </form>
      </mat-card-content>
    </mat-card>

    @if (result(); as r) {
      <mat-card class="max-w-3xl mt-6">
        <mat-card-content class="p-6">
          <h2 class="font-semibold text-lg flex items-center gap-2">
            <mat-icon class="!text-brand-600">task_alt</mat-icon>
            Resultado de la importación
          </h2>
          <div class="grid grid-cols-2 sm:grid-cols-5 gap-3 mt-4">
            <div class="rounded-lg p-3 bg-slate-100">
              <p class="text-xs text-slate-500">Total filas</p>
              <p class="text-xl font-semibold">{{ r.totalRows }}</p>
            </div>
            <div class="rounded-lg p-3 bg-emerald-50 text-emerald-800">
              <p class="text-xs">Insertados</p><p class="text-xl font-semibold">{{ r.imported }}</p>
            </div>
            <div class="rounded-lg p-3 bg-sky-50 text-sky-800">
              <p class="text-xs">Actualizados</p><p class="text-xl font-semibold">{{ r.updated }}</p>
            </div>
            <div class="rounded-lg p-3 bg-amber-50 text-amber-800">
              <p class="text-xs">Saltados</p><p class="text-xl font-semibold">{{ r.skipped }}</p>
            </div>
            <div class="rounded-lg p-3 bg-rose-50 text-rose-800">
              <p class="text-xs">Fallidos</p><p class="text-xl font-semibold">{{ r.failed }}</p>
            </div>
          </div>

          @if (r.errors.length > 0) {
            <mat-divider class="!my-4"></mat-divider>
            <h3 class="font-medium mb-2">Filas con errores</h3>
            <table mat-table [dataSource]="r.errors" class="w-full">
              <ng-container matColumnDef="rowNumber">
                <th mat-header-cell *matHeaderCellDef>Fila</th>
                <td mat-cell *matCellDef="let e">{{ e.rowNumber }}</td>
              </ng-container>
              <ng-container matColumnDef="sku">
                <th mat-header-cell *matHeaderCellDef>SKU</th>
                <td mat-cell *matCellDef="let e">{{ e.sku || '—' }}</td>
              </ng-container>
              <ng-container matColumnDef="message">
                <th mat-header-cell *matHeaderCellDef>Mensaje</th>
                <td mat-cell *matCellDef="let e" class="text-rose-700">{{ e.message }}</td>
              </ng-container>
              <tr mat-header-row *matHeaderRowDef="errorCols"></tr>
              <tr mat-row *matRowDef="let row; columns: errorCols;"></tr>
            </table>
          }
        </mat-card-content>
      </mat-card>
    }
  `
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
