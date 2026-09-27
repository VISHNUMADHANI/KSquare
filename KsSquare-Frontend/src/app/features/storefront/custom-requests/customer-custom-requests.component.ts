import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, isDevMode, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CustomJewelryRequest } from '../../../core/custom-requests/custom-request.models';
import { CustomRequestService } from '../../../core/custom-requests/custom-request.service';

@Component({ selector:'app-customer-custom-requests', standalone:true, imports:[RouterLink,CurrencyPipe,DatePipe], templateUrl:'./customer-custom-requests.component.html', styleUrl:'./customer-custom-requests.component.scss', changeDetection:ChangeDetectionStrategy.OnPush })
export class CustomerCustomRequestsComponent implements OnInit {
  protected readonly createdRequest = inject(ActivatedRoute).snapshot.queryParamMap.get('created');
  protected readonly loadError = signal('');
  private readonly router = inject(Router);
  protected readonly demoEnabled = isDevMode();
  protected canDemo(item: CustomJewelryRequest): boolean {
    return (item.status === 'Quoted' || item.status === 'Approved') && item.fixedPrice !== null && Number.isFinite(item.fixedPrice) && item.fixedPrice > 0;
  }
  protected startDemo(item: CustomJewelryRequest): void {
    if (!this.canDemo(item)) return;
    void this.router.navigate(['/checkout'], { queryParams: { quote: item.id } });
  }
  private readonly service=inject(CustomRequestService); protected readonly requests=signal<CustomJewelryRequest[]>([]); protected readonly loading=signal(true);
  ngOnInit(): void {
    this.loading.set(true); this.loadError.set('');
    const access = this.service.accessList();
    if (!access.length) { this.requests.set([]); this.loading.set(false); return; }
    forkJoin(access.map(item => this.service.getCustomer(item).pipe(catchError(() => of(null))))).subscribe(items => {
      const loaded = items.filter((item): item is CustomJewelryRequest => !!item);
      this.requests.set(loaded);
      if (loaded.length !== items.length) this.loadError.set(loaded.length ? 'Some requests could not be loaded. Please retry to see the full list.' : 'Your requests could not be loaded. Please retry in a moment.');
      this.loading.set(false);
    });
  }
}
