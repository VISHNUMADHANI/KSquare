import {HttpClient} from '@angular/common/http';
import {Component,OnInit,inject,signal,DestroyRef} from '@angular/core';
import {DatePipe} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {RouterLink} from '@angular/router';
import {takeUntilDestroyed} from '@angular/core/rxjs-interop';
import {Review,ReviewService} from '../../../core/reviews/review.service';
import {DialogFocusDirective} from '../../../shared/ui/dialog-focus.directive';
@Component({selector:'app-admin-reviews',standalone:true,imports:[DatePipe,FormsModule,RouterLink,DialogFocusDirective],templateUrl:'./admin-reviews.component.html',styleUrl:'./admin-reviews.component.scss'})
export class AdminReviewsComponent implements OnInit {
 private http=inject(HttpClient);
 protected adding=signal(false);protected importing=signal(false);protected importError=signal('');protected categories=signal<{id:string;name:string}[]>([]);
 protected draft={categoryId:'',displayName:'',rating:5,body:'',consent:false};protected files:File[]=[];protected previews:string[]=[];
 protected startImport(){this.draft={categoryId:'',displayName:'',rating:5,body:'',consent:false};this.clearFiles();this.importError.set('');this.adding.set(true);this.http.get<{id:string;name:string;parentId?:string|null}[]>('/api/admin/categories').subscribe({next:r=>this.categories.set(r.filter(c=>!c.parentId)),error:()=>this.importError.set('Categories could not be loaded. Close and try again.')});}
 protected clearFiles(){this.previews.forEach(url=>URL.revokeObjectURL(url));this.previews=[];this.files=[];}
 protected closeImport(){if(this.importing())return;this.adding.set(false);this.clearFiles();}
 protected choose(event:Event){const input=event.target as HTMLInputElement;const files=Array.from(input.files??[]);input.value='';if(this.files.length+files.length>4||files.some(f=>!['image/jpeg','image/png','image/webp'].includes(f.type)||f.size>5*1024*1024)){this.importError.set('Choose up to 4 JPG, PNG or WebP photos, 5 MB each.');return;}this.importError.set('');this.files.push(...files);this.previews.push(...files.map(f=>URL.createObjectURL(f)));}
 protected removePhoto(index:number){URL.revokeObjectURL(this.previews[index]);this.previews.splice(index,1);this.files.splice(index,1);}
 protected saveImport(){if(this.importing())return;const d=this.draft;if(!d.categoryId||!d.displayName.trim()||d.body.trim().length<10||!d.consent){this.importError.set('Choose a category, enter the customer name and at least 10 characters, and confirm permission.');return;}const body=new FormData();Object.entries(d).forEach(([k,v])=>body.append(k,String(v)));this.files.forEach(f=>body.append('files',f));this.importing.set(true);this.importError.set('');this.http.post('/api/reviews/admin',body).subscribe({next:()=>{this.importing.set(false);this.closeImport();this.status='Pending';this.page=1;this.message.set('Customer review saved. Open it to check and publish.');this.load();},error:e=>{this.importing.set(false);this.importError.set(e.error?.error??'Unable to save review. Please try again.');}});}
 private api=inject(ReviewService);private destroy=inject(DestroyRef);protected items=signal<{review:Review;productName:string}[]>([]);protected count=signal(0);protected loading=signal(false);protected busy=signal('');protected error=signal('');protected message=signal('');protected status='Pending';protected page=1;protected notes:Partial<Record<string,string>>={};
 protected selected=signal<{review:Review;productName:string}|null>(null);protected dialogError=signal('');
 protected open(entry:{review:Review;productName:string}){this.dialogError.set('');this.selected.set(entry);}
 protected close(){if(this.busy())return;this.selected.set(null);this.dialogError.set('');}
 ngOnInit(){this.destroy.onDestroy(()=>this.clearFiles());this.load();}
 protected filter(){this.page=1;this.load();}
 protected load(){this.loading.set(true);this.error.set('');this.api.admin(this.status,this.page).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:d=>{if(!d.items.length&&this.page>1){this.page--;this.load();return;}this.items.set(d.items);this.count.set(d.count);this.loading.set(false);},error:()=>{this.loading.set(false);this.error.set('Reviews could not be loaded. Please try again.');}});}
 protected paginate(delta:number){this.page+=delta;this.load();}
 protected decide(r:Review,status:string){if(this.busy())return;const note=this.notes[r.id]??r.moderationNote??'';if(status==='Rejected'&&note.trim().length<5){this.dialogError.set('Add a reason of at least 5 characters before rejecting.');return;}this.busy.set(r.id);this.dialogError.set('');this.api.moderate(r,status,note).pipe(takeUntilDestroyed(this.destroy)).subscribe({next:()=>{this.busy.set('');this.close();this.message.set(status==='Published'?'Review published.':'Review rejected.');this.load();},error:e=>{this.busy.set('');this.dialogError.set(e.error?.error||'Unable to save. Please try again.');}});}
}
