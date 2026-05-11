import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CartService } from '@core/services/cart.service';
import { OrderService } from '@core/services/order.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, RouterLink,
    MatFormFieldModule, MatInputModule, MatButtonModule
  ],
  template: `
    <h1 class="text-2xl font-semibold mb-4">Finalizar compra</h1>
    @if (cart.lines().length === 0) {
      <div class="bg-white rounded-xl p-10 text-center shadow-sm">
        <p class="text-slate-600">Tu carrito está vacío.</p>
        <a mat-flat-button color="primary" routerLink="/products" class="mt-4">Explorar productos</a>
      </div>
    } @else {
      <form [formGroup]="form" (ngSubmit)="submit()" class="bg-white rounded-xl shadow-sm p-6 max-w-xl space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <mat-form-field appearance="outline">
            <mat-label>Nombre</mat-label>
            <input matInput formControlName="customerName" required />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Email</mat-label>
            <input matInput type="email" formControlName="customerEmail" required />
          </mat-form-field>
          <mat-form-field appearance="outline">
            <mat-label>Teléfono</mat-label>
            <input matInput formControlName="customerPhone" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="md:col-span-2">
            <mat-label>Dirección de envío</mat-label>
            <input matInput formControlName="shippingAddress" />
          </mat-form-field>
          <mat-form-field appearance="outline" class="md:col-span-2">
            <mat-label>Notas</mat-label>
            <textarea matInput formControlName="notes" rows="3"></textarea>
          </mat-form-field>
        </div>
        <div class="flex justify-between border-t pt-4">
          <span class="text-slate-600">Total a pagar</span>
          <strong>{{ cart.total() | currency:'ARS':'symbol-narrow':'1.0-0' }}</strong>
        </div>
        <button mat-flat-button color="primary" class="w-full" [disabled]="form.invalid || submitting()">
          {{ submitting() ? 'Procesando…' : 'Confirmar pedido' }}
        </button>
      </form>
    }
  `
})
export class CheckoutComponent {
  protected readonly cart = inject(CartService);
  private readonly orders = inject(OrderService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  protected readonly submitting = signal(false);

  protected readonly form = this.fb.nonNullable.group({
    customerName: ['', [Validators.required, Validators.maxLength(150)]],
    customerEmail: ['', [Validators.required, Validators.email]],
    customerPhone: [''],
    shippingAddress: [''],
    notes: ['']
  });

  submit(): void {
    if (this.form.invalid || !this.cart.storeId()) return;
    this.submitting.set(true);
    this.orders.create({
      storeId: this.cart.storeId()!,
      ...this.form.getRawValue(),
      items: this.cart.lines().map(l => ({ productId: l.product.id, quantity: l.quantity }))
    }).subscribe({
      next: order => {
        this.cart.clear();
        this.snack.open(`Pedido ${order.orderNumber} creado con éxito.`, 'Cerrar', { duration: 5000 });
        this.router.navigate(['/home']);
      },
      error: () => this.submitting.set(false),
      complete: () => this.submitting.set(false)
    });
  }
}
