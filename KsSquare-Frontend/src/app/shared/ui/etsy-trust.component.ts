import { DecimalPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { EtsyReviewsStore } from './etsy-reviews.component';
@Component({selector:'ks-etsy-trust',standalone:true,imports:[DecimalPipe],template: `@if(feed();as data){@if(data.averageRating && data.totalReviews){<span class="trust"><span class="star" aria-hidden="true">★</span><strong title="Etsy shop rating for the past 12 months">{{data.averageRating|number:'1.1-1'}}/5</strong><span class="divider">·</span><span><strong>{{data.totalReviews|number}}</strong> total Etsy reviews</span></span>}}`,styles:[`:host{display:block}.trust{display:flex;align-items:center;justify-content:center;gap:7px;font-size:13px;white-space:nowrap;color:#e9dfca}.star{color:#e2c17a;font-size:18px}.period{font-size:9px;color:#b7ac96}.divider{padding:0 3px}@media(max-width:600px){.trust{font-size:12px;gap:5px}}`]})
export class EtsyTrustComponent {protected readonly feed=toSignal(inject(EtsyReviewsStore).feed);}
