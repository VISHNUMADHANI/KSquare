import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({selector:'ks-collection-hero',standalone:true,imports:[RouterLink],changeDetection:ChangeDetectionStrategy.OnPush,templateUrl:'./collection-hero.component.html',styleUrl:'./collection-hero.component.scss'})
export class CollectionHeroComponent implements OnInit,OnDestroy {
 protected readonly slides=[
  {eyebrow:'THE CUSTOM PENDANT COLLECTION',title:'Your story.',accent:'Made to shine.',copy:'Names, initials, and ideas turned into a piece that is unmistakably yours.',cta:'Create your custom pendant',path:'/custom',image:'custom-pendant',alt:'Heart-shaped iced-out custom letter pendant from K Square Gems',label:'Custom pendants'},
  {eyebrow:'THE HIP HOP EDIT',title:'Small details.',accent:'Big energy.',copy:'Expressive pendants with a presence all their own. Find the piece that says it for you.',cta:'Explore pendants',path:'/pendants',image:'butterfly-pendant',alt:'Moissanite butterfly pendant from K Square Gems',label:'Hip hop pendants'},
  {eyebrow:'THE ICED WATCH COLLECTION',title:'Every detail.',accent:'A statement.',copy:'Explore bold silhouettes and brilliant details for your next statement.',cta:'Explore watches',path:'/watches',image:'watch-detail',alt:'Three iced-out watches on a clean dark background',label:'Watches'},
  {eyebrow:'THE CUBAN CHAIN COLLECTION',title:'Bold links.',accent:'Lasting impact.',copy:'A signature silhouette. A remarkable shine. Discover your next everyday statement.',cta:'Explore Cuban chains',path:'/chains',image:'cuban-chain',alt:'Iced-out Cuban link chain from K Square Gems',label:'Cuban chains'}
 ];
 protected readonly active=signal(0);protected readonly focused=signal(false);
 private timer?:ReturnType<typeof setInterval>;private startX=0;private startY=0;private reduced=false;
 ngOnInit(){this.reduced=matchMedia('(prefers-reduced-motion: reduce)').matches;this.timer=setInterval(()=>{if(!this.reduced&&!this.focused()&&!document.hidden)this.active.update(i=>(i+1)%this.slides.length);},3000);}
 ngOnDestroy(){if(this.timer)clearInterval(this.timer);}
 protected select(index:number){this.active.set((index+this.slides.length)%this.slides.length);}
 protected touchStart(event:TouchEvent){this.startX=event.changedTouches[0].clientX;this.startY=event.changedTouches[0].clientY;}
 protected touchEnd(event:TouchEvent){const dx=event.changedTouches[0].clientX-this.startX,dy=event.changedTouches[0].clientY-this.startY;if(Math.abs(dx)>50&&Math.abs(dx)>Math.abs(dy))this.select(this.active()+(dx<0?1:-1));}
 protected focusIn(event:FocusEvent){this.focused.set(!!(event.target as HTMLElement).closest('.hero-content'));}
 protected focusOut(event:FocusEvent){if(!(event.currentTarget as HTMLElement).contains(event.relatedTarget as Node))this.focused.set(false);}
}
