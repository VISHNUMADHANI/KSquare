import { EtsyTrustComponent } from '../../ui/etsy-trust.component';
import { CartService } from '../../../core/orders/cart.service';
import { DOCUMENT } from '@angular/common';
import { ChangeDetectionStrategy, Component, ElementRef, HostListener, Injector, OnDestroy, OnInit, ViewChild, afterNextRender, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { BrandLogoComponent } from '../../ui';
import { STOREFRONT_NAVIGATION } from '../storefront-navigation';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'ks-storefront-header', standalone: true, imports: [EtsyTrustComponent, BrandLogoComponent, RouterLink, RouterLinkActive],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './storefront-header.component.html', styleUrl: './storefront-header.component.scss'
})
export class StorefrontHeaderComponent implements OnInit, OnDestroy {
  @ViewChild('menuToggle') private menuToggle?: ElementRef<HTMLButtonElement>;
  @ViewChild('menuClose') private menuClose?: ElementRef<HTMLButtonElement>;
  @ViewChild('mobileNav') private mobileNav?: ElementRef<HTMLElement>;
  protected readonly navigation = STOREFRONT_NAVIGATION;
  protected readonly menuOpen = signal(false);
  private readonly document = inject(DOCUMENT);
  private readonly injector = inject(Injector);
  protected readonly cart = inject(CartService);
  protected readonly auth = inject(AuthService);
  protected readonly accountPath = computed(() => this.auth.user()?.role === 'Admin' ? '/admin/products' : this.auth.user() ? '/profile' : '/login');
  protected readonly accountLabel = computed(() => this.auth.user()?.role === 'Admin' ? 'Admin' : this.auth.user() ? 'My account' : 'Sign in');

  ngOnInit(): void { this.auth.ensureSession().subscribe(); }

  protected toggleMenu(): void { this.setMenuState(!this.menuOpen()); }
  protected closeMenu(): void { this.setMenuState(false); }
  @HostListener('document:keydown.escape') protected onEscape(): void { this.closeMenu(); }
  @HostListener('window:resize') protected onResize(): void { if (this.menuToggle && getComputedStyle(this.menuToggle.nativeElement).display === 'none') this.closeMenu(); }
  ngOnDestroy(): void { this.document.body.classList.remove('ks-menu-open'); }
  protected trapFocus(event: KeyboardEvent): void {
    if (event.key !== 'Tab' || !this.menuOpen()) return;
    const items = this.mobileNav?.nativeElement.querySelectorAll<HTMLElement>('a[href], button:not([disabled])');
    if (!items?.length) return;
    const first = items[0], last = items[items.length - 1];
    if (event.shiftKey && this.document.activeElement === first) { event.preventDefault(); last.focus(); }
    else if (!event.shiftKey && this.document.activeElement === last) { event.preventDefault(); first.focus(); }
  }

  private setMenuState(open: boolean): void {
    if (this.menuOpen() === open) return;
    this.menuOpen.set(open);
    this.document.body.classList.toggle('ks-menu-open', open);
    afterNextRender(() => { if (open && this.menuOpen()) this.menuClose?.nativeElement.focus(); else if (!open) this.menuToggle?.nativeElement.focus(); }, { injector: this.injector });
  }
}
