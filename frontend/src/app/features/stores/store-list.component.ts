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
  templateUrl: './store-list.component.html'
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
