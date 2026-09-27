import { CanDeactivateFn } from '@angular/router';

export interface ProductDialogAware { canLeaveProducts(): boolean; }

export const productDialogGuard: CanDeactivateFn<ProductDialogAware> = component => component.canLeaveProducts();
