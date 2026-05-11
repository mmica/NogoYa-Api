import { Component, Input, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Product } from '@core/models/product.model';
import { CartService } from '@core/services/cart.service';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule],
  template: `
    <mat-card class="h-full flex flex-col hover:shadow-md transition-shadow">
      <div class="aspect-video bg-slate-100 rounded-t overflow-hidden flex items-center justify-center">
        @if (product.imageUrl) {
          <img [src]="product.imageUrl" [alt]="product.name" class="object-cover w-full h-full" />
        } @else {
          <mat-icon class="!text-slate-300 !text-5xl !w-16 !h-16">shopping_bag</mat-icon>
        }
      </div>
      <mat-card-content class="flex-1 p-3">
        <h3 class="font-medium text-slate-900 line-clamp-1">{{ product.name }}</h3>
        <p class="text-xs text-slate-500 mt-0.5">{{ product.storeName }}</p>
        <div class="mt-2 flex items-baseline gap-2">
          @if (product.discountPercent > 0) {
            <span class="text-xs line-through text-slate-400">
              {{ product.price | currency:'ARS':'symbol-narrow':'1.0-0' }}
            </span>
            <span class="text-lg font-semibold text-brand-600">
              {{ product.effectivePrice | currency:'ARS':'symbol-narrow':'1.0-0' }}
            </span>
            <span class="ml-auto text-xs bg-amber-100 text-amber-800 px-1.5 py-0.5 rounded">
              -{{ product.discountPercent | number:'1.0-0' }}%
            </span>
          } @else {
            <span class="text-lg font-semibold">
              {{ product.price | currency:'ARS':'symbol-narrow':'1.0-0' }}
            </span>
          }
        </div>
      </mat-card-content>
      <mat-card-actions class="!px-3 !pb-3">
        <button mat-flat-button color="primary" class="w-full" (click)="add()" [disabled]="product.stock <= 0">
          <mat-icon>add_shopping_cart</mat-icon>
          {{ product.stock > 0 ? 'Agregar' : 'Sin stock' }}
        </button>
      </mat-card-actions>
    </mat-card>
  `
})
export class ProductCardComponent {
  @Input({ required: true }) product!: Product;
  private readonly cart = inject(CartService);
  add(): void { this.cart.add(this.product, 1); }
}
