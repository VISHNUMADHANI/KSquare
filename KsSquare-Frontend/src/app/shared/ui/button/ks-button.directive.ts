import { Directive, HostBinding, Input } from '@angular/core';

@Directive({ selector: 'button[ksButton], a[ksButton]', standalone: true })
export class KsButtonDirective {
  @Input() variant: 'gold' | 'outline' | 'ghost' = 'gold';
  @Input() size: 'small' | 'medium' = 'medium';
  @HostBinding('class') get classes(): string { return `ks-button ks-button--${this.variant} ks-button--${this.size}`; }
}
