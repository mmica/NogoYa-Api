import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule],
  template: `
    <section class="relative overflow-hidden rounded-2xl bg-gradient-to-br from-brand-600 to-brand-700 text-white p-10 md:p-16 shadow-sm">
      <div class="max-w-2xl">
        <h1 class="text-3xl md:text-5xl font-semibold tracking-tight leading-tight">
          El marketplace de <span class="text-amber-300">Nogoyá</span>.
        </h1>
        <p class="mt-4 text-lg text-brand-50/90">
          Comprá en los comercios de tu ciudad. Descubrí ofertas, productos frescos y recibilos donde quieras.
        </p>
        <div class="mt-6 flex flex-wrap gap-3">
          <a mat-flat-button color="accent" routerLink="/products">
            <mat-icon>search</mat-icon> Ver productos
          </a>
          <a mat-stroked-button routerLink="/stores" class="!text-white !border-white/70">
            Explorar comercios
          </a>
        </div>
      </div>
    </section>

    <section class="grid grid-cols-1 md:grid-cols-3 gap-4 mt-8">
      <article class="p-6 bg-white rounded-xl shadow-sm">
        <mat-icon class="!text-brand-600">local_shipping</mat-icon>
        <h3 class="mt-2 font-semibold">Envíos locales</h3>
        <p class="text-slate-600 text-sm mt-1">Coordiná la entrega directamente con el comercio.</p>
      </article>
      <article class="p-6 bg-white rounded-xl shadow-sm">
        <mat-icon class="!text-brand-600">local_offer</mat-icon>
        <h3 class="mt-2 font-semibold">Descuentos reales</h3>
        <p class="text-slate-600 text-sm mt-1">Promociones publicadas por cada comercio, siempre vigentes.</p>
      </article>
      <article class="p-6 bg-white rounded-xl shadow-sm">
        <mat-icon class="!text-brand-600">verified</mat-icon>
        <h3 class="mt-2 font-semibold">Compra auditada</h3>
        <p class="text-slate-600 text-sm mt-1">Tu pedido queda registrado con trazabilidad completa.</p>
      </article>
    </section>
  `
})
export class HomeComponent {}
