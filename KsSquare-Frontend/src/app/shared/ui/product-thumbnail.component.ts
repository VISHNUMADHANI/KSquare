import { Component, Input, OnChanges, OnDestroy, inject, signal } from '@angular/core';
import { Subscription } from 'rxjs';
import { StorefrontCatalogService } from '../../core/catalog/storefront-catalog.service';
@Component({selector:'ks-product-thumbnail',standalone:true,template:`@if(url()){<img [src]="url()" [alt]="name" (error)="url.set('')"/>}@else{<span aria-hidden="true">?</span>}`,styles:[`:host{display:grid;place-items:center;width:100%;height:100%;overflow:hidden;background:#eee8dc;border-radius:inherit}img{width:100%;height:100%;object-fit:contain;display:block}span{font-size:1.8rem;color:#80602c}`]})
export class ProductThumbnailComponent implements OnChanges,OnDestroy {
 @Input() productId:string|null=null; @Input() src:string|null|undefined; @Input() name='Product';
 protected readonly url=signal('');private readonly catalog=inject(StorefrontCatalogService);private request?:Subscription;
 ngOnChanges():void{this.request?.unsubscribe();this.url.set(this.src??'');if(!this.src&&this.productId)this.request=this.catalog.getProduct(this.productId).subscribe({next:p=>this.url.set(p.media[0]?.url??''),error:()=>this.url.set('')});}
 ngOnDestroy():void{this.request?.unsubscribe();}
}
