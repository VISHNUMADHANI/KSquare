import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, Injectable, inject, signal } from '@angular/core';
import { catchError, map, of, shareReplay } from 'rxjs';
interface EtsyReview {rating:number;text:string;imageUrl:string|null;createdAt:number;listingUrl:string;}
interface EtsyFeed {shopName:string|null;shopUrl:string|null;items:EtsyReview[];updatedAt:string|null;totalReviews?:number|null;averageRating?:number|null;}
function decodeReview(text:string):string {
 return text.replace(/&(#x[0-9a-f]+|#[0-9]+|amp|quot|apos|lt|gt|nbsp);/gi,(original,entity:string)=>{
  if(entity.startsWith('#')){const hex=entity[1].toLowerCase()==='x';const code=parseInt(entity.slice(hex?2:1),hex?16:10);return code>0&&code<=0x10ffff?String.fromCodePoint(code):original;}
  return ({amp:'&',quot:'"',apos:"'",lt:'<',gt:'>',nbsp:' '} as Record<string,string>)[entity.toLowerCase()]??original;
 });
}
@Injectable({providedIn:'root'})
export class EtsyReviewsStore {
 readonly feed=inject(HttpClient).get<EtsyFeed>('/api/etsy/reviews').pipe(map(feed=>({...feed,items:feed.items.map(review=>({...review,text:decodeReview(review.text)}))})),catchError(()=>of({shopName:null,shopUrl:null,items:[],updatedAt:null} as EtsyFeed)),shareReplay({bufferSize:1,refCount:false}));
}
@Component({selector:'ks-etsy-reviews',standalone:true,imports:[DatePipe,DecimalPipe],template:`
@if(feed();as data){@if(data.items.length){
<section id="etsy-customer-reviews" class="etsy-section" aria-label="Reviews from our Etsy shop">
 <header><div><p class="ks-eyebrow">FROM OUR ETSY CUSTOMERS</p><h2>A little sparkle. A lot of love.</h2></div>@if(data.totalReviews && data.averageRating){<div class="review-summary"><span class="summary-star" aria-hidden="true">★</span><strong title="Etsy shop rating for the past 12 months">{{data.averageRating|number:'1.1-1'}}<small> / 5</small></strong><span class="summary-divider"></span><span><b>{{data.totalReviews|number}}</b> total Etsy reviews</span></div>}</header>
 <div class="slider-shell">
 <button class="nav nav-prev" type="button" aria-label="Previous reviews" [disabled]="position()===0" (click)="move(track,-1)"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="m14 6-6 6 6 6"/></svg></button>
 <div #track class="review-track" tabindex="0" role="region" aria-label="Swipe or use arrow keys to browse Etsy reviews" (scroll)="updatePosition(track)" (keydown.arrowright)="move(track,1);$event.preventDefault()" (keydown.arrowleft)="move(track,-1);$event.preventDefault()">
 @for(review of data.items;track $index){<article>
 <div class="stars" role="img" [attr.aria-label]="review.rating+' out of 5 stars'">@for(star of [1,2,3,4,5];track star){<span [class.empty]="star>review.rating" aria-hidden="true">★</span>}</div>
 <div class="review-body"><div class="story">@if(review.text){<p class="review-text" [class.expanded]="expanded().has($index)">“{{review.text}}”</p>@if(review.text.length>180){<button class="read-more" type="button" [attr.aria-expanded]="expanded().has($index)" (click)="toggle($index)">{{expanded().has($index)?'Read less':'Read full review'}}</button>}}@else{<p class="rating-only">A customer shared their rating.</p>}</div>

 @if(review.imageUrl){<div class="photo"><img [src]="review.imageUrl" alt="Customer photo of their jewellery" loading="lazy" (error)="hideImage($event)" /></div>}</div>
 <footer><p>Review shared on Etsy <span>from {{data.shopName}}</span></p>@if(review.createdAt){<time>{{review.createdAt*1000|date:'MMM d, y'}}</time>}</footer>
 </article>}
 </div>
 <button class="nav nav-next" type="button" aria-label="Next reviews" [disabled]="atEnd()||data.items.length===1" (click)="move(track,1)"><svg viewBox="0 0 24 24" aria-hidden="true"><path d="m10 6 6 6-6 6"/></svg></button>
 </div>
 <div class="carousel-footer"><p>Customer experiences from across our Etsy shop.</p></div>
</section>}}`,styleUrl:'./etsy-reviews.component.scss'})
export class EtsyReviewsComponent {
 protected readonly feed=signal<EtsyFeed|null>(null);
 protected readonly expanded=signal(new Set<number>());
 protected readonly position=signal(0);protected readonly atEnd=signal(false);
 constructor(){inject(EtsyReviewsStore).feed.subscribe(data=>this.feed.set(data));}
 protected toggle(index:number){this.expanded.update(current=>{const next=new Set(current);next.has(index)?next.delete(index):next.add(index);return next;});}
 protected move(track:HTMLElement,direction:number){const card=track.querySelector('article');if(!card)return;track.scrollBy({left:direction*(card.getBoundingClientRect().width+16),behavior:matchMedia('(prefers-reduced-motion: reduce)').matches?'instant':'smooth'});}
 protected updatePosition(track:HTMLElement){const width=track.querySelector('article')?.getBoundingClientRect().width??1;this.position.set(Math.round(track.scrollLeft/(width+16)));this.atEnd.set(track.scrollLeft+track.clientWidth>=track.scrollWidth-3);}
 protected hideImage(event:Event){(event.target as HTMLImageElement).hidden=true;}
}
