import { Component, Input, Output, EventEmitter, OnChanges, OnDestroy, inject, signal, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { ReviewService, ReviewPage, ReviewMedia } from '../../../core/reviews/review.service';
import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
@Component({selector:'ks-product-reviews',standalone:true,imports:[DatePipe,FormsModule,DialogFocusDirective],templateUrl:'./product-reviews.component.html',styleUrls:['./product-reviews.component.scss','./product-reviews.component-dialog.scss']})
export class ProductReviewsComponent implements OnChanges, OnDestroy {
 @Input({required:true}) productId=''; @Output() summary=new EventEmitter<{total:number;average:number}>();
 private api=inject(ReviewService);private request?:Subscription;
 protected data=signal<ReviewPage|null>(null);protected loading=signal(false);protected error=signal('');protected viewer=signal<ReviewMedia|null>(null);
 protected media=computed(()=>this.data()?.items.flatMap(r=>r.media.map(m=>({...m,name:r.displayName})))??[]);
 protected round=Math.round;protected page=1;protected sort='newest';
 ngOnChanges(){this.page=1;this.data.set(null);this.viewer.set(null);this.load();}
 ngOnDestroy(){this.request?.unsubscribe();}
 protected load(){this.request?.unsubscribe();this.loading.set(true);this.error.set('');this.request=this.api.list(this.productId,this.page,this.sort,false).subscribe({next:d=>{this.data.set(d);this.summary.emit(d);this.loading.set(false);},error:()=>{this.error.set('Reviews could not be loaded. Please try again.');this.loading.set(false);}});}
 protected filter(){this.page=1;this.load();}
 protected changePage(delta:number){this.page+=delta;this.load();}
}
