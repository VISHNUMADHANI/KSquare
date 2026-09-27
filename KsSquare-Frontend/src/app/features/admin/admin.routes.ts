import { Routes } from '@angular/router';
import { productDialogGuard } from '../../core/guards/product-dialog.guard';

export const adminRoutes: Routes = [
 {path:'customers',loadComponent:()=>import('./activity/admin-activity.component').then(m=>m.AdminActivityComponent)},
 {path:'payments',data:{payments:true},loadComponent:()=>import('./activity/admin-activity.component').then(m=>m.AdminActivityComponent)},
  { path:'reviews',loadComponent:()=>import('./reviews/admin-reviews.component').then(m=>m.AdminReviewsComponent) },
  { path:'returns',loadComponent:()=>import('./returns/admin-returns.component').then(m=>m.AdminReturnsComponent) },
  { path: 'orders', data:{admin:true}, loadComponent: () => import('../orders/orders.component').then(m => m.OrdersComponent) },
  { path: '', pathMatch: 'full', loadComponent: () => import('./overview/admin-overview.component').then(m => m.AdminOverviewComponent) },
  { path: 'categories', loadComponent: () => import('./categories/category-admin.component').then(m => m.CategoryAdminComponent) },
  { path: 'product-options', loadComponent: () => import('./product-options/product-option-admin.component').then(m => m.ProductOptionAdminComponent) },
  { path: 'custom-requests', loadComponent: () => import('./custom-requests/admin-custom-requests.component').then(m => m.AdminCustomRequestsComponent) },
  { path: 'products', canDeactivate: [productDialogGuard], loadComponent: () => import('./products/product-list.component').then(m => m.ProductListComponent) },
  { path: 'products/new', redirectTo: 'products' },
  { path: 'products/:id/edit', redirectTo: 'products' },
  { path: 'products/:id', redirectTo: 'products' }
];
