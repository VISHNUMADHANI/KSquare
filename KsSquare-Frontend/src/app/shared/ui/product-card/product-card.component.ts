import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { Product } from '../../../core/catalog/catalog.models';
import { RouterLink } from '@angular/router';
import { PriceDisplayComponent } from '../price-display/price-display.component';

@Component({ selector: 'ks-product-card', standalone: true, imports: [PriceDisplayComponent, RouterLink], changeDetection: ChangeDetectionStrategy.OnPush, template: `
  <a class="card" [class.card--unavailable]="!product.isAvailable" [routerLink]="['/products', product.id]">
    <div class="media">@if (product.media[0]; as image) { <img [src]="image.url" [alt]="image.altText || product.name" loading="lazy" /> } @else { <div class="placeholder"><img src="assets/brand/ksquare-symbol.png" alt="" /><small>Image coming soon</small></div> }@if (product.discountPercentage > 0) { <span class="discount">-{{ product.discountPercentage }}%</span> }@if (!product.isAvailable) { <span class="unavailable">Unavailable</span> }</div>
    <div class="body"><p>{{ product.categoryName }}</p><h2>{{ product.name }}</h2>@if (product.subcategories.length) { <div class="tags">@for (item of product.subcategories.slice(0, 2); track item.id) { <span>{{ item.name }}</span> }</div> }@if(product.pendantVariants?.length){<small>From</small>}<ks-price-display [price]="product.originalPrice" [discountedPrice]="product.discountPercentage ? product.finalPrice : null" currency="USD" /></div>
  </a>`, styleUrl: './product-card.component.scss' })
export class ProductCardComponent { @Input({ required: true }) product!: Product; }
