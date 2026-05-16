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
  templateUrl: './settings.component.html'
})
export class SettingsComponent {
  protected readonly entries: SettingsEntry[] = [
    { label: 'Administrar comercios', description: 'Creá, editá y deshabilitá los comercios.', icon: 'store', route: '/settings/stores' },
    { label: 'Administrar productos', description: 'Creá, editá y eliminá productos del catálogo.', icon: 'shopping_bag', route: '/settings/products' },
    { label: 'Importar productos', description: 'Cargá tu catálogo desde un archivo .xlsx.', icon: 'upload_file', route: '/settings/import-products' },
    { label: 'Historial de auditoría', description: 'Revisá todos los cambios de precios, stock y pedidos.', icon: 'receipt_long', route: '/settings/audit', disabled: true },
    { label: 'Usuarios y permisos', description: 'Gestioná quién puede operar el sistema.', icon: 'group', route: '/settings/users', disabled: true }
  ];
}
