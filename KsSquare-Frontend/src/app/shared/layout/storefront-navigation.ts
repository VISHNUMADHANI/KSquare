export interface StorefrontNavigationItem {
  readonly label: string;
  readonly path: string;
}

export const STOREFRONT_NAVIGATION: readonly StorefrontNavigationItem[] = [
  { label: 'Custom orders', path: '/custom' },
  { label: 'Chains', path: '/chains' },
  { label: 'Bracelets', path: '/bracelets' },
  { label: 'Rings', path: '/rings' },
  { label: 'Pendants', path: '/pendants' },
  { label: 'Earrings', path: '/earrings' },
  { label: 'Watches', path: '/watches' },
  { label: 'Grillz', path: '/grillz' }
];
