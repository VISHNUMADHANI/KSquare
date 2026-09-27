import { ProductThumbnailComponent } from '../../../shared/ui/product-thumbnail.component';
import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, inject, isDevMode } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartService } from '../../../core/orders/cart.service';
@Component({ selector:'app-cart', standalone:true, imports:[ProductThumbnailComponent,CurrencyPipe,RouterLink], templateUrl:'./cart.component.html', styleUrl:'./cart.component.scss', changeDetection:ChangeDetectionStrategy.OnPush })
export class CartComponent { protected readonly cart=inject(CartService); protected readonly demoEnabled=isDevMode(); }
