import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BrandLogoComponent } from '../../ui';
import { STOREFRONT_NAVIGATION } from '../storefront-navigation';

@Component({
  selector: 'ks-storefront-footer', standalone: true, imports: [BrandLogoComponent, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './storefront-footer.component.html', styleUrl: './storefront-footer.component.scss'
})
export class StorefrontFooterComponent {
  protected readonly navigation = STOREFRONT_NAVIGATION;
  protected readonly year = new Date().getFullYear();
}
