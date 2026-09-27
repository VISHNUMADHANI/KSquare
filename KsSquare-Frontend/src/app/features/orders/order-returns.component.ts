import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnDestroy, OnChanges, Input, Output, EventEmitter, inject, signal, isDevMode } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { CustomerOrder, OrderService, ReturnRequest, ReturnSelection } from '../../core/orders/order.service';
@Component({selector:'ks-order-returns',standalone:true,imports:[FormsModule,RouterLink,CurrencyPipe,DatePipe],templateUrl:'./order-returns.component.html',styleUrl:'./order-returns.component.scss'})
export class OrderReturnsComponent implements OnDestroy, OnChanges {
 private readonly previewUrls=new Map<File,string>();
 protected previewUrl(file:File){return this.previewUrls.get(file)??'';}
 protected clearFiles(){this.previewUrls.forEach(url=>URL.revokeObjectURL(url));this.previewUrls.clear();this.files=[];}
 ngOnDestroy(){this.clearFiles();}
 ngOnChanges(){if(this.admin){const entry=this.order.returns?.find(r=>r.status==='Requested'||r.status==='Approved');if(entry){this.beginReview(entry,'Accept');this.note=entry.adminNote??'';this.refundAmount=entry.refundAmount??this.returnTotal(entry);}}}
 protected selectionTotal(){return this.selectedItems.reduce((sum,line)=>sum+this.order.items[line.itemIndex].unitPrice*line.quantity,0);}
 protected decide(entry:ReturnRequest,action:'Accept'|'Decline'){this.action.set(action);this.review(entry);}
 @Input({required:true}) order!: CustomerOrder; @Input() admin=false;
 @Output() orderChanged=new EventEmitter<CustomerOrder>(); @Output() busyChange=new EventEmitter<boolean>();
 private readonly service=inject(OrderService); protected readonly busy=signal(false); protected readonly error=signal(''); protected readonly success=signal('');
 protected readonly formOpen=signal(false); protected readonly reviewId=signal(''); protected readonly action=signal(''); protected readonly demo=isDevMode();
 protected selectedItems: ReturnSelection[]=[]; protected reason='Change of mind'; protected description=''; protected condition=false; protected files:File[]=[];
 protected note=''; protected refundAmount:number|null=null; protected inspected=false; protected contactConfirmed=false;
 protected readonly reasons=['Change of mind','Size or fit','Damaged or defective','Incorrect item'];
 protected eligible(){if(this.order.returns?.length)return [];return this.order.items.map((item,index)=>({item,index})).filter(row=>row.item.canRequestReturn);}
 protected start(){this.selectedItems=this.eligible().slice(0,1).map(row=>({itemIndex:row.index,quantity:1}));this.error.set('');this.success.set('');this.formOpen.set(true);}
 protected addFiles(event:Event){const input=event.target as HTMLInputElement;const files=Array.from(input.files??[]);input.value='';if(!files.length)return;const combined=[...this.files];for(const file of files){if(!combined.some(f=>f.name===file.name&&f.size===file.size&&f.lastModified===file.lastModified))combined.push(file);}if(combined.length>4||files.some(f=>!['image/jpeg','image/png','image/webp'].includes(f.type)||f.size<=0||f.size>10*1024*1024)){this.error.set('Choose up to 4 JPEG, PNG or WebP photos, no more than 10 MB each.');return;}for(const file of combined){if(!this.previewUrls.has(file))this.previewUrls.set(file,URL.createObjectURL(file));}this.files=combined;this.error.set('');}
 protected removeFile(index:number){const file=this.files[index];const url=this.previewUrls.get(file);if(url)URL.revokeObjectURL(url);this.previewUrls.delete(file);this.files=this.files.filter((_,i)=>i!==index);this.error.set('');}
 protected submit(){
  if(this.busy())return;this.error.set('');
  if(!this.validSelection(this.eligible()))return;
  if(this.description.trim().length<10||this.description.trim().length>2000){this.error.set('Please describe your reason in at least 10 characters (maximum 2000).');return;}
  if(!this.condition){this.error.set('Please tick the return conditions checkbox before sending your request.');return;}
  if(['Damaged or defective','Incorrect item'].includes(this.reason)&&!this.files.length){this.error.set('Add a clear photo of the issue.');return;}
  const body=new FormData();this.selectedItems.forEach((line,index)=>{body.set('items['+index+'].itemIndex',String(line.itemIndex));body.set('items['+index+'].quantity',String(line.quantity));});body.set('reason',this.reason);body.set('description',this.description.trim());body.set('confirmCondition',String(this.condition));body.set('version',String(this.order.version));this.files.forEach(file=>body.append('files',file));
  this.setBusy(true);this.service.requestReturn(this.order.id,body).pipe(finalize(()=>this.setBusy(false))).subscribe({next:order=>{this.orderChanged.emit(order);this.formOpen.set(false);this.description='';this.condition=false;this.clearFiles();this.success.set('Return request sent. Please wait for approval and return instructions before shipping your item.');},error:e=>this.error.set(e.error?.error??(e.error?.errors?Object.values(e.error.errors).flat().join(' '):e.status===0?'Unable to connect. Your details and photos are kept; please try again.':'Your request could not be sent. Your details and photos are kept; please try again.'))});
 }
 protected beginReview(entry:ReturnRequest,action:string){this.reviewId.set(entry.id);this.action.set(action);this.selectedItems=this.returnLines(entry).map(line=>({...line}));this.note='';this.refundAmount=entry.refundAmount;this.inspected=false;this.contactConfirmed=false;this.error.set('');this.success.set('');}
 protected review(entry:ReturnRequest){
  if(this.busy())return;this.error.set('');const max=this.selectionTotal();if(this.action()==='Accept'&&!this.validSelection(this.adminEligible()))return;
  if(this.note.trim().length<5||this.note.trim().length>2000){this.error.set('Enter a customer-facing note of 5 to 2000 characters.');return;}
  
  if(this.action()==='Accept'&&(this.refundAmount===null||!Number.isFinite(this.refundAmount)||this.refundAmount<0||this.refundAmount>max||Math.abs(this.refundAmount*100-Math.round(this.refundAmount*100))>0.000001)){this.error.set('Enter a refund between zero and the selected items total, with at most two decimals.');return;}
  this.setBusy(true);this.service.reviewReturn(this.order.id,entry.id,{action:this.action(),note:this.note.trim(),refundAmount:this.action()==='Accept'?this.refundAmount:null,inspected:this.inspected,contactConfirmed:this.contactConfirmed,items:this.action()==='Accept'?this.selectedItems:undefined,version:this.order.version}).pipe(finalize(()=>this.setBusy(false))).subscribe({next:order=>{this.orderChanged.emit(order);this.reviewId.set('');this.success.set(this.action()==='Accept'?'Return accepted. Instructions and refund amount are visible to the customer.':'Return declined. The reason is visible to the customer.');},error:e=>this.error.set(e.error?.error??'The decision could not be saved. Please try again.')});
 }
 protected contactText(){return 'Hello '+this.order.customerName+', regarding return for order '+this.order.orderNumber+'. Let us discuss the return and refund amount, including any shipping deductions.';}
 protected whatsapp(){const phone=this.order.customerPhone.replace(/[^0-9]/g,'');return phone?'https://wa.me/'+phone+'?text='+encodeURIComponent(this.contactText()):null;}
 protected email(){return 'mailto:'+encodeURIComponent(this.order.customerEmail)+'?subject='+encodeURIComponent('Return request '+this.order.orderNumber)+'&body='+encodeURIComponent(this.contactText());}
 protected returnLines(entry:ReturnRequest):ReturnSelection[]{return entry.items??(entry.itemIndices??[entry.itemIndex]).map(itemIndex=>({itemIndex,quantity:this.order.items[itemIndex].quantity}));}
 protected returnTotal(entry:ReturnRequest){return this.returnLines(entry).reduce((total,line)=>total+this.order.items[line.itemIndex].unitPrice*line.quantity,0);}
 protected selectionQuantity(index:number){return this.selectedItems.find(line=>line.itemIndex===index)?.quantity??0;}
 protected toggleItem(index:number,checked:boolean){this.selectedItems=checked?[...this.selectedItems.filter(line=>line.itemIndex!==index),{itemIndex:index,quantity:1}]:this.selectedItems.filter(line=>line.itemIndex!==index);}
 protected setQuantity(index:number,value:string){this.selectedItems=this.selectedItems.map(line=>line.itemIndex===index?{...line,quantity:Number(value)}:line);}
 protected adminEligible(){return this.order.items.map((item,index)=>({item,index})).filter(row=>row.item.canIncludeInReturn);}
 private validSelection(rows:{index:number}[]){if(!this.selectedItems.length||this.selectedItems.some(line=>!rows.some(row=>row.index===line.itemIndex)||!Number.isInteger(line.quantity)||line.quantity<1||line.quantity>this.order.items[line.itemIndex].quantity)){this.error.set('Select at least one eligible item and a quantity between 1 and the quantity purchased.');return false;}return true;}
 private setBusy(value:boolean){this.busy.set(value);this.busyChange.emit(value);}
}
