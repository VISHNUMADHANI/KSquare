import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AdminCatalogService } from '../../../core/catalog/admin-catalog.service';
import { Product } from '../../../core/catalog/catalog.models';
import { KsButtonDirective, PriceDisplayComponent, StatePanelComponent, StatusBadgeComponent } from '../../../shared/ui';

@Component({ selector: 'app-product-detail', standalone: true, imports: [RouterLink, DatePipe, KsButtonDirective, PriceDisplayComponent, StatePanelComponent, StatusBadgeComponent], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './product-detail.component.html', styleUrl: './product-detail.component.scss' })
export class ProductDetailComponent implements OnInit {
  private readonly catalog = inject(AdminCatalogService); private readonly route = inject(ActivatedRoute);
  protected readonly product = signal<Product | null>(null); protected readonly loading = signal(true); protected readonly error = signal('');
  ngOnInit(): void { const id = this.route.snapshot.paramMap.get('id')!; this.catalog.getProduct(id).pipe(finalize(() => this.loading.set(false))).subscribe({ next: value => this.product.set(value), error: () => this.error.set('Product could not be loaded.') }); }
}
