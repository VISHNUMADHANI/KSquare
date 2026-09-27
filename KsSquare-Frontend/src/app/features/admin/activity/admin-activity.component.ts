import {Component,inject,signal} from '@angular/core';
import {CommonModule} from '@angular/common';
import {HttpClient} from '@angular/common/http';
import {ActivatedRoute,RouterLink} from '@angular/router';
import {FormsModule} from '@angular/forms';
import {DialogFocusDirective} from '../../../shared/ui/dialog-focus.directive';
interface Row {id:string;fullName:string;email:string;phone:string;isEmailVerified:boolean;createdAt:string;orderCount:number;customerName:string;customerEmail:string;total:number;currency:string;status:string;providerState:string;environment:string;reference:string;orderId:string|null;}
interface Order {id:string;orderNumber:string;createdAt:string;total:number;currency:string;status:string;paymentStatus:string;deliveryAddress:string;}
@Component({standalone:true,imports:[CommonModule,FormsModule,RouterLink,DialogFocusDirective],templateUrl:'./admin-activity.component.html',styleUrl:'./admin-activity.component.scss'})
export class AdminActivityComponent{
 private http=inject(HttpClient);
 readonly payments=inject(ActivatedRoute).snapshot.data['payments']===true;
 readonly rows=signal<Row[]>([]);readonly total=signal(0);readonly page=signal(1);readonly loading=signal(false);readonly error=signal('');
 readonly customer=signal<{customer:Row;orders:Order[]}|null>(null);readonly detailLoading=signal(false);readonly detailError=signal('');readonly detailOpen=signal(false);
 search='';status=inject(ActivatedRoute).snapshot.queryParamMap.get('status')??'';private request=0;private detailRequest=0;
 constructor(){this.load(1);}
 load(page:number){const request=++this.request;this.loading.set(true);this.error.set('');
 this.http.get<{items:Row[];total:number;page:number}>('/api/admin/'+(this.payments?'payments':'customers'),{params:{search:this.search.trim(),status:this.status,page}}).subscribe({
 next:r=>{if(request!==this.request)return;this.rows.set(r.items);this.total.set(r.total);this.page.set(r.page);this.loading.set(false);},
 error:()=>{if(request!==this.request)return;this.rows.set([]);this.error.set('Unable to load records. Please try again.');this.loading.set(false);}});
 }
 open(row:Row){const request=++this.detailRequest;this.customer.set(null);this.detailError.set('');this.detailLoading.set(true);this.detailOpen.set(true);
 this.http.get<{customer:Row;orders:Order[]}>('/api/admin/customers/'+row.id).subscribe({next:r=>{if(request!==this.detailRequest)return;this.customer.set(r);this.detailLoading.set(false);},error:()=>{if(request!==this.detailRequest)return;this.detailError.set('Unable to load customer details. Close and try again.');this.detailLoading.set(false);}});
 }
 close(){++this.detailRequest;this.detailOpen.set(false);this.customer.set(null);}
}
