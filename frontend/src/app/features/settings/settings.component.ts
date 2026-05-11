import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

interface SettingsEntry {
  label: string;
  description: string;
  icon: string;
  route: string;
  disabled?: boolean;
}

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [RouterLink, MatCardModule, MatIconModule],
  template: `
    <header class="mb-6">
      <h1 class="text-2xl font-semibold">Configuración</h1>
      <p class="text-slate-600">Administrá tu comercio y tus datos en Nogo-Ya.</p>
    </header>
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
      @for (entry of entries; track entry.route) {
        <a [routerLink]="entry.disabled ? null : entry.route"
           [class.pointer-events-none]="entry.disabled"
           [class.opacity-60]="entry.disabled"
           class="no-underline">
          <mat-card class="h-full hover:shadow-md transition-shadow">
            <mat-card-content class="p-5">
              <div class="flex items-start gap-3">
                <div class="p-2.5 rounded-lg bg-brand-50 text-brand-600">
                  <mat-icon>{{ entry.icon }}</mat-icon>
                </div>
                <div>
                  <h3 class="font-medium">{{ entry.label }}</h3>
                  <p class="text-sm text-slate-600 mt-0.5">{{ entry.description }}</p>
                  @if (entry.disabled) {
                    <span class="text-xs text-slate-400 italic">Próximamente</span>
                  }
                </div>
              </div>
            </mat-card-content>
          </mat-card>
        </a>
      }
    </div>
  `
})
export class SettingsComponent {
  protected readonly entries: SettingsEntry[] = [
    { label: 'Importar productos', description: 'Cargá tu catálogo desde un archivo .xlsx.', icon: 'upload_file', route: '/settings/import-products' },
    { label: 'Administrar comercios', description: 'Editá los datos de tu comercio.', icon: 'store', route: '/settings/stores', disabled: true },
    { label: 'Historial de auditoría', description: 'Revisá todos los cambios de precios, stock y pedidos.', icon: 'receipt_long', route: '/settings/audit', disabled: true },
    { label: 'Usuarios y permisos', description: 'Gestioná quién puede operar el sistema.', icon: 'group', route: '/settings/users', disabled: true }
  ];
}
