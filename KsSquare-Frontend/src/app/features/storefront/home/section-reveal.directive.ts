import { Directive, ElementRef, OnDestroy, afterNextRender, inject } from '@angular/core';
@Directive({selector:'[ksSectionReveal]',standalone:true})
export class SectionRevealDirective implements OnDestroy {
 private readonly element=inject(ElementRef<HTMLElement>).nativeElement;
 private observer?:IntersectionObserver;
 constructor(){afterNextRender(()=>{if(matchMedia('(prefers-reduced-motion: reduce)').matches||!('IntersectionObserver' in window))return;this.observer=new IntersectionObserver(entries=>{if(entries.some(entry=>entry.isIntersecting)){this.element.animate([{opacity:.45,transform:'translateY(18px)'},{opacity:1,transform:'translateY(0)'}],{duration:650,easing:'cubic-bezier(.2,.7,.2,1)'});this.observer?.disconnect();}},{threshold:.08});this.observer.observe(this.element);});}
 ngOnDestroy(){this.observer?.disconnect();}
}
