import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { StoreService } from '@core/services/store.service';
import { Store } from '@core/models/store.model';

@Component({
  selector: 'app-store-list',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatIconModule, MatProgressSpinnerModule],
  template: `
    <header class="mb-6">
      <h1 class="text-2xl font-semibold">Comercios de Nogoyá</h1>
      <p class="text-slate-600">Explorá los negocios locales adheridos a Nogo-Ya.</p>
    </header>
    @if (loading()) {
      <div class="flex justify-center py-10"><mat-spinner diameter="40"></mat-spinner></div>
    } @else {
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        @for (store of stores(); track store.id) {
          <a [routerLink]="['/stores', store.slug]" class="no-underline">
            <mat-card class="h-full hover:shadow-md transition-shadow">
              <mat-card-header>
                <div class="flex items-center gap-2">
                  <mat-icon class="!text-brand-600">storefront</mat-icon>
                  <mat-card-title class="!text-base">{{ store.name }}</mat-card-title>
                </div>
              </mat-card-header>
              <mat-card-content class="text-slate-600 text-sm">
                <p>{{ store.description || 'Comercio local en Nogoyá.' }}</p>
                <p class="mt-2 text-xs text-slate-500">{{ store.productsCount }} productos</p>
              </mat-card-content>
            </mat-card>
          </a>
        } @empty {
          <p class="text-slate-500">Todavía no hay comercios cargados.</p>
        }
      </div>
    }
  `
})
export class StoreListComponent implements OnInit {
  private readonly svc = inject(StoreService);
  protected readonly stores = signal<Store[]>([]);
  protected readonly loading = signal(true);

  ngOnInit(): void {
    this.svc.list().subscribe({
      next: data => this.stores.set(data),
      complete: () => this.loading.set(false),
      error: () => this.loading.set(false)
    });
  }
}
