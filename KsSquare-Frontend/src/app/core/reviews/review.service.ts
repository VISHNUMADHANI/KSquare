import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
export interface ReviewMedia { type: 'photo'|'video'; url: string; }
export interface Review { id:string; productId:string|null; isImported?:boolean; categoryId?:string|null; productName?:string; displayName:string; rating:number; title:string; body:string; verifiedPurchase:boolean; createdAt:string; media:ReviewMedia[]; status:string|null; moderationNote:string|null; version:number; }
export interface ReviewPage { categoryId:string; categoryName:string; total:number; average:number; breakdown:{rating:number;count:number}[]; count:number; page:number; items:Review[]; }
@Injectable({providedIn:'root'})
export class ReviewService {
 private http=inject(HttpClient);
 list(id:string,page=1,sort='newest',mediaOnly=false){return this.http.get<ReviewPage>('/api/reviews/product/'+id,{params:{page,sort,mediaOnly}});}
 eligibility(id:string,orderId:string){return this.http.get<{eligible:boolean;message:string;note:string;review?:Review}>('/api/reviews/product/'+id+'/eligibility',{params:{orderId}});}
 submitted(orderId:string){return this.http.get<string[]>('/api/reviews/order/'+orderId+'/submitted');}
 create(id:string,body:FormData){return this.http.post('/api/reviews/product/'+id,body,{observe:'events',reportProgress:true});}
 admin(status:string,page:number){return this.http.get<{count:number;items:{review:Review;productName:string}[]}>('/api/reviews/admin',{params:{status,page}});}
 moderate(review:Review,status:string,note:string){return this.http.put('/api/reviews/admin/'+review.id,{status,note,version:review.version});}
}
