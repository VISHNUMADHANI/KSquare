import { ReviewService } from '../../core/reviews/review.service';
import { ReviewComposerComponent } from '../storefront/reviews/review-composer.component';
import { OrderReturnsComponent } from './order-returns.component';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CustomerOrder, OrderService } from '../../core/orders/order.service';
import { DialogFocusDirective } from '../../shared/ui/dialog-focus.directive';
@Component({selector:'app-orders',standalone:true,imports:[ReviewComposerComponent,FormsModule,OrderReturnsComponent,CurrencyPipe,DatePipe,RouterLink,DialogFocusDirective],templateUrl:'./orders.component.html',styleUrls:['./order-dialog.component.scss','./orders.component.scss','./shipment.component.scss','./order-review.component.scss'],changeDetection:ChangeDetectionStrategy.OnPush})
export class OrdersComponent implements OnInit {
 private readonly reviews=inject(ReviewService);protected readonly reviewLookup=signal<'loading'|'ready'|'error'>('loading');
 private readonly route=inject(ActivatedRoute);private readonly service=inject(OrderService);private readonly destroy=inject(DestroyRef);
 protected readonly admin=this.route.snapshot.data['admin']===true;
 protected readonly orders=signal<CustomerOrder[]>([]);protected readonly loading=signal(true);protected readonly error=signal('');protected readonly total=signal(0);protected readonly page=signal(1);protected readonly status=signal('');protected readonly selected=signal<CustomerOrder|null>(null);protected readonly saving=signal(false);protected readonly dialogError=signal('');
 protected readonly statuses=['Confirmed','Processing','Shipped','Delivered','Cancelled'];
 protected shipping=false;protected delivering=false;protected courier='';protected trackingNumber='';protected deliveryDate='';protected readonly couriers=['USPS','UPS','DHL','FedEx','Aramex'];
 protected readonly reviewTarget=signal<{productId:string;productName:string;orderId:string;orderNumber:string}|null>(null);
 protected readonly reviewMessage=signal('');protected readonly reviewedProducts=signal<string[]>([]);
 protected writeReview(order:CustomerOrder,productId:string,productName:string):void{if(this.admin||order.status!=='Delivered'||this.saving()||!order.items.some(i=>i.productId===productId))return;this.reviewMessage.set('');this.reviewTarget.set({productId,productName,orderId:order.id,orderNumber:order.orderNumber});}
 protected reviewSubmitted():void{const target=this.reviewTarget();if(target)this.reviewedProducts.update(ids=>[...ids,target.productId]);this.reviewTarget.set(null);this.reviewMessage.set('Thank you! Your review has been submitted and is awaiting moderation.');}
 private loadSequence=0;
 ngOnInit():void{this.load();}
 protected load():void {
  const sequence=++this.loadSequence;this.loading.set(true);this.error.set('');
  this.service.list(this.admin,this.page(),this.status()).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:p=>{if(sequence!==this.loadSequence)return;this.orders.set(p.items);this.total.set(p.totalCount);this.loading.set(false);const id=this.route.snapshot.queryParamMap.get('order');if(id&&!this.selected()){const match=p.items.find(o=>o.id===id);if(match)this.open(match);else this.service.get(id,this.admin).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:order=>this.open(order),error:()=>this.error.set("The linked order could not be loaded.")});}},error:e=>{if(sequence!==this.loadSequence)return;this.error.set(e.error?.error??'Orders could not be loaded. Please retry.');this.loading.set(false);}});
 }
 protected filter(value:string):void{this.status.set(value);this.page.set(1);this.load();}
 protected paginate(delta:number):void{this.page.update(p=>p+delta);this.load();}
 protected open(order:CustomerOrder):void{this.shipping=false;this.delivering=false;this.dialogError.set('');this.reviewMessage.set('');this.selected.set(order);this.loadReviewStatus(order);this.service.get(order.id,this.admin).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:updated=>{if(this.selected()?.id===updated.id&&updated.version>=(this.selected()?.version??0))this.selected.set(updated);},error:()=>{if(this.selected()?.id===order.id)this.dialogError.set('The latest order details could not be loaded. Close and reopen this order to try again.');}});}
 protected loadReviewStatus(order:CustomerOrder):void{if(this.admin||order.status!=='Delivered')return;this.reviewLookup.set('loading');this.reviews.submitted(order.id).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:ids=>{if(this.selected()?.id!==order.id)return;this.reviewedProducts.set(ids);this.reviewLookup.set('ready');},error:()=>{if(this.selected()?.id===order.id)this.reviewLookup.set('error');}});}
 protected returnUpdated(order:CustomerOrder):void{this.selected.set(order);this.orders.update(rows=>rows.map(row=>row.id===order.id?order:row));}
 protected close():void{if(!this.saving())this.selected.set(null);}
 protected nextStatuses(order:CustomerOrder):string[]{return ({Confirmed:['Processing','Cancelled'],Processing:['Shipped','Cancelled'],Shipped:['Delivered']} as Record<string,string[]>)[order.status]??[];}
 protected update(status:string,confirm=false):void {
  if(status==='Shipped'&&!confirm){this.shipping=true;this.delivering=false;this.courier=this.selected()?.courier??'';this.trackingNumber=this.selected()?.trackingNumber??'';return;}
  if(status==='Delivered'&&!confirm){this.delivering=true;this.shipping=false;this.deliveryDate='';return;}
  if(status==='Shipped'&&(!this.courier||!this.trackingNumber.trim())){this.dialogError.set('Choose a courier and enter the tracking number.');return;}
  if(status==='Delivered'&&(!this.deliveryDate||!Number.isFinite(new Date(this.deliveryDate).getTime()))){this.dialogError.set('Enter the actual delivery date and time.');return;}
  const order=this.selected();if(!order||this.saving())return;this.saving.set(true);this.dialogError.set('');
  this.service.update(order.id,status,order.version,status==='Shipped'?{courier:this.courier,trackingNumber:this.trackingNumber.trim()}:status==='Delivered'?{deliveredAt:new Date(this.deliveryDate).toISOString()}:{}).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:updated=>{this.shipping=false;this.delivering=false;this.selected.set(updated);this.saving.set(false);this.load();},error:e=>{this.dialogError.set(e.error?.error??'Unable to update this order. Reopen the orders page before retrying.');this.saving.set(false);}});
 }
}
