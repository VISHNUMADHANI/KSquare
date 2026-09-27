
import { Routes } from '@angular/router';
import { customerGuard } from '../../core/guards/auth.guard';

export const storefrontRoutes: Routes = [
  { path: 'grills', redirectTo: 'grillz', pathMatch: 'full' },
  { path: 'return-policy', loadComponent: () => import('./return-policy/return-policy.component').then(m => m.ReturnPolicyComponent) },
  { path: 'checkout/demo', redirectTo: 'checkout', pathMatch: 'full' },
  { path: 'checkout', canActivate:[customerGuard], loadComponent: () => import('./demo-checkout/demo-checkout.component').then(m => m.DemoCheckoutComponent) },
  { path: 'cart', loadComponent: () => import('./cart/cart.component').then(m => m.CartComponent) },
  { path: 'orders', canActivate:[customerGuard], loadComponent: () => import('../orders/orders.component').then(m => m.OrdersComponent) },
  ...[
    { path: 'watches', category: 'watch', title: 'Make every moment yours.', description: 'Explore watches that bring a distinctive finish to your everyday style.' },
    { path: 'grillz', category: 'grill', title: 'A statement in every smile.', description: 'Discover grillz with bold detail and a personal point of view.' },
    { path: 'rings', category: 'ring', title: 'A little detail. A lasting impression.', description: 'Discover rings for your everyday style and your most meaningful moments.' },
    { path: 'pendants', category: 'pendant', title: 'Keep your story close.', description: 'Explore expressive pendants and personal details that make a piece your own.' },
    { path: 'earrings', category: 'earring', title: 'Your everyday radiance.', description: 'Discover earrings that bring a considered finishing touch to your style.' }
  ].map(collection => ({ path: collection.path, data: { collectionCategory: collection.category, collectionTitle: collection.title, collectionEyebrow: 'The ' + collection.path + ' collection', collectionDescription: collection.description }, loadComponent: () => import('./catalog/catalog.component').then(m => m.CatalogComponent) })),
  { path: 'profile', canActivate: [customerGuard], loadComponent: () => import('./profile/customer-profile.component').then(m => m.CustomerProfileComponent) },
  {
    path: '',
    loadComponent: () => import('./home/home.component').then(({ HomeComponent }) => HomeComponent)
  },
  {
    path: 'shop', redirectTo: '', pathMatch: 'full'
  },
  {
    path: 'chains',
    data: {
      collectionCategory: 'chain',
      collectionTitle: 'Chains built around your style.',
      collectionEyebrow: 'K Square chains',
      collectionDescription: 'Choose your chain size, width and diamond detail. Every available combination shows its exact price.',
      collectionOptionTypes: ['ChainSize', 'ChainWidth', 'ChainDiamondSize']
    },
    loadComponent: () => import('./catalog/catalog.component').then(({ CatalogComponent }) => CatalogComponent)
  },
  {
    path: 'bracelets',
    data: {
      collectionCategory: 'brace',
      collectionTitle: 'Bracelets made for your perfect fit.',
      collectionEyebrow: 'K Square bracelets',
      collectionDescription: 'Choose your stone size, bracelet length and color. Each available combination shows its exact price.',
      collectionOptionTypes: ['BraceletStoneSize', 'BraceletSize', 'Color']
    },
    loadComponent: () => import('./catalog/catalog.component').then(({ CatalogComponent }) => CatalogComponent)
  },
  {
    path: 'custom/requests',
    loadComponent: () => import('./custom-requests/customer-custom-requests.component').then(({ CustomerCustomRequestsComponent }) => CustomerCustomRequestsComponent)
  },
  {
    path: 'custom',
    loadComponent: () => import('./custom-jewelry/custom-jewelry.component').then(({ CustomJewelryComponent }) => CustomJewelryComponent)
  },
  {
    path: 'products/:id',
    loadComponent: () => import('./product-detail/storefront-product-detail.component').then(({ StorefrontProductDetailComponent }) => StorefrontProductDetailComponent)
  }
];
