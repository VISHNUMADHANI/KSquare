import { Routes } from '@angular/router';
import { adminGuard } from './core/guards/auth.guard';

export const appRoutes: Routes = [
  { path: 'login', data: { mode: 'login' }, loadComponent: () => import('./features/storefront/auth/auth-page.component').then(m => m.AuthPageComponent) },
  { path: 'register', data: { mode: 'register' }, loadComponent: () => import('./features/storefront/auth/auth-page.component').then(m => m.AuthPageComponent) },
  { path: 'verify-email', data: { mode: 'verify' }, loadComponent: () => import('./features/storefront/auth/auth-page.component').then(m => m.AuthPageComponent) },
  { path: 'forgot-password', data: { mode: 'forgot' }, loadComponent: () => import('./features/storefront/auth/auth-page.component').then(m => m.AuthPageComponent) },
  { path: 'reset-password', data: { mode: 'reset' }, loadComponent: () => import('./features/storefront/auth/auth-page.component').then(m => m.AuthPageComponent) },
  {
    path: '',
    loadComponent: () =>
      import('./layout/storefront-layout/storefront-layout.component').then(
        ({ StorefrontLayoutComponent }) => StorefrontLayoutComponent
      ),
    loadChildren: () =>
      import('./features/storefront/storefront.routes').then(({ storefrontRoutes }) => storefrontRoutes)
  },
  {
    path: 'admin',
    canMatch: [adminGuard],
    loadComponent: () =>
      import('./layout/admin-layout/admin-layout.component').then(
        ({ AdminLayoutComponent }) => AdminLayoutComponent
      ),
    loadChildren: () => import('./features/admin/admin.routes').then(({ adminRoutes }) => adminRoutes)
  },
  { path: '**', redirectTo: '' }
];
