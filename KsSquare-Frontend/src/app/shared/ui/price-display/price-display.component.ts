import { CurrencyPipe, NgIf } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'ks-price-display', standalone: true, imports: [CurrencyPipe, NgIf], changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<div class="price" aria-label="Product price"><span class="price__current">{{ effectivePrice | currency:currency:'symbol':'1.0-2' }}</span><del *ngIf="hasDiscount" class="price__original">{{ price | currency:currency:'symbol':'1.0-2' }}</del><span *ngIf="hasDiscount" class="price__discount">{{ discountPercent }}% off</span></div>`,
  styleUrl: './price-display.component.scss'
})
export class PriceDisplayComponent {
  @Input({ required: true }) price = 0;
  @Input() discountedPrice: number | null = null;
  @Input() currency = 'INR';
  get hasDiscount(): boolean { return this.discountedPrice !== null && this.discountedPrice >= 0 && this.discountedPrice < this.price; }
  get effectivePrice(): number { return this.hasDiscount ? this.discountedPrice! : this.price; }
  get discountPercent(): number { return this.hasDiscount && this.price > 0 ? Math.round((1 - this.effectivePrice / this.price) * 100) : 0; }
}
