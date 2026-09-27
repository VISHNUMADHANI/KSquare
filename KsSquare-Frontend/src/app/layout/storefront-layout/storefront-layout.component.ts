import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { StorefrontFooterComponent } from '../../shared/layout/storefront-footer/storefront-footer.component';
import { StorefrontHeaderComponent } from '../../shared/layout/storefront-header/storefront-header.component';

@Component({
  selector: 'app-storefront-layout', standalone: true,
  imports: [RouterOutlet, StorefrontHeaderComponent, StorefrontFooterComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="storefront-shell"><ks-storefront-header /><main id="main-content" class="storefront-shell__content"><router-outlet /></main><ks-storefront-footer /></div>`,
  styleUrl: './storefront-layout.component.scss'
})
export class StorefrontLayoutComponent {}
