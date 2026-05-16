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
  templateUrl: './checkout.component.html'
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
