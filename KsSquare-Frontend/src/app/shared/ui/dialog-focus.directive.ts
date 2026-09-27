import { Directive, ElementRef, EventEmitter, HostListener, Input, OnChanges, OnDestroy, Output, inject } from '@angular/core';

/** Keyboard focus and scroll containment for customer dialogs. */
@Directive({ selector: '[ksDialogFocus]', standalone: true })
export class DialogFocusDirective implements OnChanges, OnDestroy {
  @Input() ksDialogFocus = true;
  @Output() ksDialogDismiss = new EventEmitter<void>();
  private readonly element = inject<ElementRef<HTMLElement>>(ElementRef);
  private previous: HTMLElement | null = null;
  private active = false;
  private frame = 0;
  private originalOverflow = '';
  ngOnChanges(): void {
    if (this.ksDialogFocus && !this.active) {
      this.previous = document.activeElement as HTMLElement;
      this.active = true;
      this.originalOverflow = document.body.style.overflow;
      document.body.style.overflow = 'hidden';
      this.frame = requestAnimationFrame(() => this.items()[0]?.focus());
    } else if (!this.ksDialogFocus) this.restore();
  }
  @HostListener('keydown', ['$event']) onKey(event: KeyboardEvent): void {
    if (!this.active) return;
    if (event.key === 'Escape') { event.stopPropagation(); this.ksDialogDismiss.emit(); }
    if (event.key !== 'Tab') return;
    const items = this.items(), first = items[0], last = items[items.length - 1];
    if (!first) { event.preventDefault(); return; }
    if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
    else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
  }
  ngOnDestroy(): void { this.restore(); }
  private items(): HTMLElement[] {
    return Array.from(this.element.nativeElement.querySelectorAll<HTMLElement>('a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex="0"]')).filter(item => item.getClientRects().length > 0);
  }
  private restore(): void {
    if (!this.active) return;
    cancelAnimationFrame(this.frame);
    document.body.style.overflow = this.originalOverflow;
    this.active = false;
    if (this.previous?.isConnected) this.previous.focus();
  }
}
