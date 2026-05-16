import { Routes } from '@angular/router';
import { MainLayoutComponent } from './layouts/main-layout/main-layout.component';

export const routes: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'home' },
      { path: 'home', loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent), title: 'Nogo-Ya · Marketplace local' },
      { path: 'stores', loadComponent: () => import('./features/stores/store-list.component').then(m => m.StoreListComponent), title: 'Comercios · Nogo-Ya' },
      { path: 'stores/:slug', loadComponent: () => import('./features/stores/store-detail.component').then(m => m.StoreDetailComponent) },
      { path: 'products', loadComponent: () => import('./features/products/product-list.component').then(m => m.ProductListComponent), title: 'Productos · Nogo-Ya' },
      { path: 'cart', loadComponent: () => import('./features/cart/cart.component').then(m => m.CartComponent), title: 'Mi carrito · Nogo-Ya' },
      { path: 'checkout', loadComponent: () => import('./features/cart/checkout.component').then(m => m.CheckoutComponent), title: 'Finalizar compra · Nogo-Ya' },
      { path: 'settings', loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent), title: 'Configuración · Nogo-Ya' },
      { path: 'settings/import-products', loadComponent: () => import('./features/settings/import-products.component').then(m => m.ImportProductsComponent), title: 'Importar productos · Nogo-Ya' },
      { path: 'settings/stores', loadComponent: () => import('./features/settings/stores/stores.component').then(m => m.StoresComponent), title: 'Administrar comercios · Nogo-Ya' },
      { path: 'settings/products', loadComponent: () => import('./features/settings/products/products.component').then(m => m.ProductsAdminComponent), title: 'Administrar productos · Nogo-Ya' }
    ]
  },
  { path: '**', redirectTo: 'home' }
];
