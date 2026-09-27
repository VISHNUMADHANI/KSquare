import { CurrencyPipe,DatePipe } from '@angular/common';
import { Component,DestroyRef,inject,signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { CustomerOrder,OrderService } from '../../../core/orders/order.service';
import { OrderReturnsComponent } from '../../orders/order-returns.component';
import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
@Component({selector:'app-admin-returns',standalone:true,imports:[CurrencyPipe,DatePipe,RouterLink,OrderReturnsComponent,DialogFocusDirective],templateUrl:'./admin-returns.component.html',styleUrl:'./admin-returns.component.scss'})
export class AdminReturnsComponent{
 private readonly service=inject(OrderService);private readonly destroy=inject(DestroyRef);private sequence=0;
 protected readonly orders=signal<CustomerOrder[]>([]);protected readonly loading=signal(true);protected readonly error=signal('');protected readonly total=signal(0);protected readonly page=signal(1);protected readonly status=signal('Requested');protected readonly selected=signal<CustomerOrder|null>(null);protected readonly busy=signal(false);protected readonly detailError=signal('');protected readonly detailLoading=signal(false);
 protected readonly filters=[{value:'Requested',label:'Needs review'},{value:'Approved',label:'Awaiting return'},{value:'Declined',label:'Declined'},{value:'Refunded',label:'Refunded'},{value:'',label:'All requests'}];
 constructor(){this.load();}
 protected load(){const seq=++this.sequence;this.loading.set(true);this.error.set('');this.service.returns(this.page(),this.status()).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:r=>{if(seq!==this.sequence)return;this.orders.set(r.items);this.total.set(r.totalCount);this.loading.set(false);if(!r.items.length&&this.page()>1){this.page.set(this.page()-1);this.load();}},error:e=>{if(seq!==this.sequence)return;this.error.set(e.error?.error??'Return requests could not be loaded.');this.loading.set(false);}});}
 protected filter(value:string){this.status.set(value);this.page.set(1);this.load();}
 protected visibleReturns(order:CustomerOrder){return (order.returns??[]).filter(r=>!this.status()||r.status===this.status());}
 protected open(order:CustomerOrder){this.selected.set(order);this.detailError.set('');this.detailLoading.set(true);this.service.get(order.id,true).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:updated=>{if(this.selected()?.id!==updated.id)return;this.selected.set(updated);this.detailLoading.set(false);},error:()=>{if(this.selected()?.id!==order.id)return;this.detailLoading.set(false);this.detailError.set('The latest order could not be loaded. Close and reopen it to try again.');}});}
 protected close(){if(this.busy())return;this.selected.set(null);}
 protected updated(order:CustomerOrder){this.selected.set(order);this.load();}
 protected paginate(delta:number){this.page.update(p=>p+delta);this.load();}
}
