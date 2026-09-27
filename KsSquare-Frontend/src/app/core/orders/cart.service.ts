import { Injectable, computed, effect, inject, signal, untracked } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { OrderLine } from './order.service';
export interface CartEntry extends OrderLine { imageUrl?: string; key: string; name: string; unitPrice: number; details: string[]; }
@Injectable({ providedIn: 'root' })
export class CartService {
 private readonly auth = inject(AuthService);
 private readonly rows = signal<CartEntry[]>([]);
 readonly items = this.rows.asReadonly();
 readonly count = computed(() => this.rows().reduce((n,i) => n + i.quantity, 0));
 readonly total = computed(() => this.rows().reduce((n,i) => n + Math.round(i.unitPrice * 100) * i.quantity, 0) / 100);
 readonly message = signal('');
 private storageKey = '';
 constructor() {
  this.synchronize();
  effect(() => { this.auth.user(); untracked(() => this.synchronize()); }, { allowSignalWrites: true });
 }
 private synchronize(): void {
  const user=this.auth.user();const key='ksquare_cart_v1_'+(user?.id??'guest');
  if(key===this.storageKey)return;
  const next=this.read(key);
  if(user && (!this.storageKey || this.storageKey==='ksquare_cart_v1_guest')) {
   const guest=this.storageKey?this.rows():this.read('ksquare_cart_v1_guest');
   for(const item of guest){const existing=next.find(i=>i.key===item.key);const remaining=20-next.filter(i=>i.productId===item.productId).reduce((n,i)=>n+i.quantity,0);const quantity=Math.min(remaining,item.quantity);if(quantity<=0)continue;if(existing)existing.quantity+=quantity;else if(next.length<30)next.push({...item,quantity});}
   try{localStorage.removeItem('ksquare_cart_v1_guest');}catch{}
  }
  this.storageKey=key;this.rows.set(next);this.save();
 }
 key(line: OrderLine): string { return JSON.stringify([line.productId,[...line.optionIds].sort(),line.personalizedName ?? '',line.personalizationNote?.trim() ?? '']); }
 add(item: Omit<CartEntry,'key'|'quantity'>): boolean {
  this.message.set('');
  const key = this.key({ ...item, quantity: 1 });
  const next = this.rows().map(i => ({...i}));
  if (next.filter(i => i.productId === item.productId).reduce((n,i)=>n+i.quantity,0) >= 20) { this.message.set('You can add up to 20 of each product.'); return false; }
  const row = next.find(i => i.key === key);
  if(row) row.quantity++; else { if(next.length >= 30) { this.message.set('Your cart can hold up to 30 selections.'); return false; } next.push({...item,key,quantity:1}); }
  this.rows.set(next); this.save(); return true;
 }
 quantity(key: string, quantity: number): void {
  if(!Number.isInteger(quantity) || quantity < 1 || quantity > 20) return;
  const row = this.rows().find(i=>i.key===key); if(!row)return;
  if(this.rows().filter(i=>i.productId===row.productId && i.key!==key).reduce((n,i)=>n+i.quantity,quantity)>20) { this.message.set('Maximum 20 per product.'); return; }
  this.message.set(''); this.rows.update(rows=>rows.map(i=>i.key===key?{...i,quantity}:i)); this.save();
 }
 remove(key: string): void { this.rows.update(rows=>rows.filter(i=>i.key!==key)); this.save(); }
 lines(): OrderLine[] { this.synchronize(); return this.rows().map(({productId,quantity,optionIds,personalizedName,personalizationNote})=>({productId,quantity,optionIds:[...optionIds],personalizedName,personalizationNote})); }
 removePurchased(lines: OrderLine[]): void {
  this.rows.update(rows=>rows.map(item=>({...item,quantity:item.quantity-(lines.find(line=>this.key(line)===item.key)?.quantity ?? 0)})).filter(i=>i.quantity>0)); this.save();
 }
 private save(): void { try { if(this.storageKey)localStorage.setItem(this.storageKey,JSON.stringify(this.rows())); } catch { this.message.set('Browser storage is unavailable. Your cart will last until this page is closed.'); } }
 private read(key: string): CartEntry[] {
  try { const rows: unknown = JSON.parse(localStorage.getItem(key) ?? '[]'); if(!Array.isArray(rows))return [];
   return rows.slice(0,30).filter(i=>i && typeof i.productId==='string' && typeof i.name==='string' && Array.isArray(i.optionIds) && i.optionIds.every((v:unknown)=>typeof v==='string') && Array.isArray(i.details) && i.details.every((v:unknown)=>typeof v==='string') && Number.isFinite(i.unitPrice) && i.unitPrice>0 && Number.isInteger(i.quantity) && i.quantity>=1 && i.quantity<=20 && (i.personalizationNote == null || (typeof i.personalizationNote==='string' && i.personalizationNote.length<=1000)) && (i.personalizedName===null || typeof i.personalizedName==='string')).map(i=>({...i,key:this.key(i)}));
  } catch { return []; }
 }
}
