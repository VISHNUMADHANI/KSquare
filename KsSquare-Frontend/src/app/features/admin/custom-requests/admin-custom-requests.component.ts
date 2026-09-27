import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CustomJewelryRequest } from '../../../core/custom-requests/custom-request.models';
import { CustomRequestService } from '../../../core/custom-requests/custom-request.service';
import { KsButtonDirective, StatusBadgeComponent } from '../../../shared/ui';

@Component({selector:'app-admin-custom-requests',standalone:true,imports:[DialogFocusDirective,CurrencyPipe,DatePipe,FormsModule,KsButtonDirective,StatusBadgeComponent],templateUrl:'./admin-custom-requests.component.html',styleUrls:['./admin-custom-requests.component.scss','./admin-request-dialog.component.scss'],changeDetection:ChangeDetectionStrategy.OnPush})
export class AdminCustomRequestsComponent implements OnInit{
  private readonly route=inject(ActivatedRoute);private readonly service=inject(CustomRequestService);protected readonly requests=signal<CustomJewelryRequest[]>([]);protected readonly selected=signal<CustomJewelryRequest|null>(null);protected readonly loading=signal(true);protected readonly saving=signal(false);protected readonly error=signal('');protected readonly filter=signal('All');protected quoteValue:number|null=null;
  protected readonly filtered=computed(()=>this.filter()==='All'?this.requests():this.requests().filter(item=>item.status===this.filter()));
  ngOnInit():void{this.load();}protected open(item:CustomJewelryRequest):void{this.selected.set(item);this.quoteValue=item.fixedPrice;}protected close():void{if(this.saving())return;this.selected.set(null);this.error.set('');}protected callUrl(phone:string):string{return `tel:${phone}`;}protected emailUrl(item:CustomJewelryRequest):string{return `mailto:${item.email}?subject=${encodeURIComponent(`K Square custom request ${item.requestNumber}`)}`;}
  protected setStatus(status:'Contacted'|'Cancelled'):void{const item=this.selected();if(!item)return;this.saving.set(true);this.service.updateStatus(item.id,status).subscribe({next:value=>{this.replace(value);this.saving.set(false);},error:error=>{this.error.set(this.message(error));this.saving.set(false);}});}
  protected quote():void{const item=this.selected();if(!item||!this.quoteValue||this.quoteValue<=0){this.error.set('Enter a fixed price greater than zero.');return;}this.saving.set(true);this.service.setQuote(item.id,this.quoteValue).subscribe({next:value=>{this.replace(value);this.saving.set(false);},error:error=>{this.error.set(this.message(error));this.saving.set(false);}});}
  protected load():void{this.loading.set(true);this.error.set('');this.service.getAdminAll().subscribe({next:items=>{this.requests.set(items);this.loading.set(false);const id=this.route.snapshot.queryParamMap.get('request');const selected=items.find(item=>item.id===id);if(selected)this.open(selected);},error:()=>{this.error.set('Custom requests could not be loaded.');this.loading.set(false);}});}private replace(value:CustomJewelryRequest):void{this.requests.update(items=>items.map(item=>item.id===value.id?value:item));this.selected.set(value);this.error.set('');}private message(error:unknown):string{return error instanceof HttpErrorResponse?error.error?.error??'The action could not be completed.':'The action could not be completed.';}
}
