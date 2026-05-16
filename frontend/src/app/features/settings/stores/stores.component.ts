import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, debounceTime, switchMap, takeUntil } from 'rxjs';
import { StoreService } from '@core/services/store.service';
import { Store } from '@core/models/store.model';
import { PagedResult } from '@core/models/product.model';
import {
  StoreDialogResult,
  StoreFormDialogComponent
} from './store-form-dialog.component';

@Component({
  selector: 'app-stores',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatCardModule, MatButtonModule, MatIconModule,
    MatFormFieldModule, MatInputModule, MatTableModule,
    MatPaginatorModule, MatProgressBarModule, MatChipsModule,
    MatTooltipModule, MatMenuModule, MatDialogModule
  ],
  templateUrl: './stores.component.html'
})
export class StoresComponent implements OnInit, OnDestroy {
  private readonly svc = inject(StoreService);
  private readonly dialog = inject(MatDialog);
  private readonly snack = inject(MatSnackBar);
  private readonly destroy$ = new Subject<void>();
  private readonly refresh$ = new Subject<void>();

  protected readonly columns = ['name', 'contact', 'products', 'status', 'actions'];
  protected readonly result = signal<PagedResult<Store> | null>(null);
  protected readonly loading = signal(false);
  protected readonly search = new FormControl('', { nonNullable: true });

  protected readonly pageSize = 25;
  private page = 1;

  ngOnInit(): void {
    // Both the search input and paginator changes feed the same reload pipeline.
    this.search.valueChanges
      .pipe(debounceTime(300), takeUntil(this.destroy$))
      .subscribe(() => { this.page = 1; this.refresh$.next(); });

    this.refresh$
      .pipe(
        switchMap(() => {
          this.loading.set(true);
          return this.svc.search({
            search: this.search.value || undefined,
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

    // Initial load.
    this.refresh$.next();
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
    const ref = this.dialog.open<StoreFormDialogComponent, any, StoreDialogResult>(
      StoreFormDialogComponent,
      { data: { mode: 'create' } }
    );
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.svc.create({
        name: result.name,
        slug: result.slug!,
        description: result.description,
        logoUrl: result.logoUrl,
        address: result.address,
        phone: result.phone,
        email: result.email
      }).subscribe({
        next: created => {
          this.snack.open(`Comercio "${created.name}" creado.`, 'Cerrar', { duration: 4000 });
          this.refresh$.next();
        }
      });
    });
  }

  openEdit(store: Store): void {
    const ref = this.dialog.open<StoreFormDialogComponent, any, StoreDialogResult>(
      StoreFormDialogComponent,
      { data: { mode: 'edit', store } }
    );
    ref.afterClosed().subscribe(result => {
      if (!result) return;
      this.svc.update(store.id, {
        name: result.name,
        description: result.description,
        logoUrl: result.logoUrl,
        address: result.address,
        phone: result.phone,
        email: result.email,
        isActive: result.isActive ?? store.isActive
      }).subscribe({
        next: () => {
          this.snack.open(`Comercio actualizado.`, 'Cerrar', { duration: 3000 });
          this.refresh$.next();
        }
      });
    });
  }

  toggleActive(store: Store): void {
    this.svc.update(store.id, {
      name: store.name,
      description: store.description,
      logoUrl: store.logoUrl,
      address: store.address,
      phone: store.phone,
      email: store.email,
      isActive: !store.isActive
    }).subscribe({
      next: () => {
        this.snack.open(
          `Comercio ${store.isActive ? 'desactivado' : 'activado'}.`,
          'Cerrar', { duration: 3000 }
        );
        this.refresh$.next();
      }
    });
  }

  remove(store: Store): void {
    const confirmed = window.confirm(
      `¿Eliminar "${store.name}"? Es un soft-delete: queda registrado en el historial de auditoría.`
    );
    if (!confirmed) return;
    this.svc.delete(store.id).subscribe({
      next: () => {
        this.snack.open(`Comercio "${store.name}" eliminado.`, 'Cerrar', { duration: 3000 });
        this.refresh$.next();
      }
    });
  }
}
