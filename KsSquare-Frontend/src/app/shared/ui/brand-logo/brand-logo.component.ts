import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
@Component({
  selector: 'ks-brand-logo', standalone: true, changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="brand-frame" [class.brand-frame--storefront]="presentation === 'storefront'" [class.brand-frame--symbol]="variant === 'symbol'"><img [src]="source" [alt]="decorative ? '' : 'K Square Gems & Jewellery'" [attr.aria-hidden]="decorative || null" /></span>`,
  styleUrl: './brand-logo.component.scss'
})
export class BrandLogoComponent {
  @Input() variant: 'symbol' | 'wordmark' = 'wordmark';
  @Input() presentation: 'default' | 'storefront' = 'default';
  @Input() decorative = false;
  get source(): string { return this.variant === 'symbol' ? 'assets/brand/ksquare-symbol.png' : 'assets/brand/ksquare-wordmark.png'; }
}
