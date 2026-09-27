import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, DestroyRef, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpEventType } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ReviewService, Review } from '../../../core/reviews/review.service';
import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
@Component({selector:'ks-review-composer',standalone:true,imports:[DatePipe,FormsModule,DialogFocusDirective],templateUrl:'./review-composer.component.html',styleUrls:['./product-reviews.component.scss','./product-reviews.component-dialog.scss','./review-composer.component.scss']})
export class ReviewComposerComponent implements OnInit, OnDestroy {
 @Input({required:true}) productId=''; @Input({required:true}) orderId=''; @Input({required:true}) productName=''; @Input() orderNumber='';
 @Output() dismissed=new EventEmitter<void>(); @Output() submitted=new EventEmitter<void>();
 private api=inject(ReviewService);private destroy=inject(DestroyRef);
 protected error=signal('');protected saving=signal(false);protected progress=signal(0);protected eligibility=signal<{eligible:boolean;message:string;note:string;review?:Review}|null>(null);protected eligibilityError=signal('');
 protected rating=0;protected body='';protected consent=false;protected files:{file:File;url:string}[]=[];
 ngOnInit(){this.api.eligibility(this.productId,this.orderId).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:e=>this.eligibility.set(e),error:()=>this.eligibilityError.set('We could not check your order. Close this form and try again.')});}
 ngOnDestroy(){this.clearFiles();}
 protected close(){if(!this.saving())this.dismissed.emit();}
 protected choose(event:Event){const input=event.target as HTMLInputElement;const added=Array.from(input.files??[]);input.value='';const all=[...this.files.map(x=>x.file),...added];const photos=all.filter(f=>f.type.startsWith('image/'));const videos=all.filter(f=>f.type.startsWith('video/'));if(photos.length>4||videos.length>1||all.some(f=>!['image/jpeg','image/png','image/webp','video/mp4','video/webm','video/quicktime'].includes(f.type)||f.size===0||f.size>(f.type.startsWith('video/')?25:5)*1024*1024)){this.error.set('Add up to 4 photos (JPG, PNG or WebP, 5 MB each) and 1 video (MP4, WebM or MOV, 25 MB).');return;}this.error.set('');this.files.push(...added.map(file=>({file,url:URL.createObjectURL(file)})));}
 protected remove(index:number){URL.revokeObjectURL(this.files[index].url);this.files.splice(index,1);}
 private clearFiles(){this.files.forEach(f=>URL.revokeObjectURL(f.url));this.files=[];}
 protected submit(){if(this.saving())return;if(!this.rating||!this.consent||this.body.trim().length<10){this.error.set('Add your rating, at least 10 characters and permission to publish.');return;}const form=new FormData();form.append('orderId',this.orderId);form.append('rating',String(this.rating));form.append('body',this.body.trim());form.append('consent','true');this.files.forEach(f=>form.append('files',f.file));this.saving.set(true);this.progress.set(0);this.error.set('');this.api.create(this.productId,form).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:e=>{if(e.type===HttpEventType.UploadProgress)this.progress.set(Math.round(100*e.loaded/(e.total||e.loaded)));if(e.type===HttpEventType.Response){this.saving.set(false);this.submitted.emit();this.clearFiles();this.body='';this.rating=0;this.consent=false;}},error:e=>{this.saving.set(false);this.error.set(e.error?.error||'Your review could not be sent. Please try again.');}});}
}
