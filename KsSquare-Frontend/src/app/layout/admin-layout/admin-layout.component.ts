import { ChangeDetectionStrategy, Component, HostListener, computed, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';
import { BrandLogoComponent } from '../../shared/ui';
import { DialogFocusDirective } from '../../shared/ui/dialog-focus.directive';
import { AuthService } from '../../core/auth/auth.service';
@Component({ selector: 'app-admin-layout', standalone: true, imports: [RouterOutlet, RouterLink, RouterLinkActive, BrandLogoComponent, DialogFocusDirective], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './admin-layout.component.html', styleUrl: './admin-layout.component.scss' })
export class AdminLayoutComponent {
  private readonly auth = inject(AuthService); private readonly router = inject(Router);
  protected readonly navigationOpen = signal(false);
  protected readonly mobile = signal(window.innerWidth <= 800);
  protected readonly user = this.auth.user;
  protected readonly initials = computed(() => (this.user()?.fullName || 'Admin').trim().split(/\s+/).slice(0,2).map(p => p[0]).join('').toUpperCase());
  protected readonly links = [
    { path: '/admin', label: 'Overview', icon: 'M3 3h7v7H3z M14 3h7v7h-7z M3 14h7v7H3z M14 14h7v7h-7z' },
    {path:'/admin/customers',label:'Customers',icon:'M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2 M13 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0 M17 3a4 4 0 0 1 0 8'},
 {path:'/admin/payments',label:'Payments',icon:'M3 5h18v14H3z M3 9h18 M7 15h4'},
 { path: '/admin/orders', label: 'Orders', icon: 'M5 4h14v17H5z M9 2h6v4H9z M8 11h8 M8 15h6' },
    { path: '/admin/reviews', label: 'Reviews', icon: 'm12 3 3 6 7 1-5 5 1 7-6-3-6 3 1-7-5-5 7-1z' },
    { path: '/admin/returns', label: 'Returns', icon: 'M9 4 4 9l5 5 M4 9h10a6 6 0 0 1 0 12h-4' },
    { path: '/admin/products', label: 'Products', icon: 'M3 7l9-4 9 4v10l-9 4-9-4V7z M3 7l9 4 9-4 M12 11v10 M7 5l10 4' },
    { path: '/admin/categories', label: 'Categories', icon: 'M3 6a2 2 0 0 1 2-2h5l2 3h7a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V6z' },
    { path: '/admin/product-options', label: 'Sizes & colors', icon: 'M4 7h16 M4 17h16 M8 4v6 M16 14v6' },
    { path: '/admin/custom-requests', label: 'Custom requests', icon: 'M21 11a8 8 0 0 1-8 8H7l-4 3V5a2 2 0 0 1 2-2h8a8 8 0 0 1 8 8z M8 8h8 M8 12h5' }
  ];
  private readonly url = toSignal(this.router.events.pipe(filter(e => e instanceof NavigationEnd), map(() => this.router.url)), { initialValue: this.router.url });
  protected readonly pageTitle = computed(() => this.links.find(l => l.path === this.url().split('?')[0])?.label || 'Administration');
  protected closeNavigation(): void { const restore = this.mobile() && this.navigationOpen(); this.navigationOpen.set(false); if (restore) requestAnimationFrame(() => document.querySelector<HTMLButtonElement>('.menu-toggle')?.focus()); }
  @HostListener('window:resize') protected resize(): void { this.mobile.set(window.innerWidth <= 800); if (!this.mobile()) this.closeNavigation(); }
  protected logout(): void { this.auth.logout().subscribe({ next: () => this.router.navigate(['/login']), error: () => this.router.navigate(['/login']) }); }
}
