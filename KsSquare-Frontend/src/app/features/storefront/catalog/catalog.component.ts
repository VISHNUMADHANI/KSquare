import { EtsyReviewsComponent } from '../../../shared/ui/etsy-reviews.component';
import { matchesCollectionCategory } from '../../../core/catalog/category-names';
import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { CatalogPage, StorefrontCatalogService } from '../../../core/catalog/storefront-catalog.service';
import { Category, ProductOption } from '../../../core/catalog/catalog.models';
import { MultiSelectFilterComponent, MultiSelectFilterOption, ProductCardComponent, StatePanelComponent } from '../../../shared/ui';

@Component({ selector: 'app-catalog', standalone: true, imports: [EtsyReviewsComponent, DialogFocusDirective, MultiSelectFilterComponent, ProductCardComponent, StatePanelComponent], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './catalog.component.html', styleUrls: ['./catalog.component.scss', './chain-collection.component.scss'] })
export class CatalogComponent implements OnInit {
  private readonly catalog = inject(StorefrontCatalogService); private readonly route = inject(ActivatedRoute); private readonly router = inject(Router);
  protected readonly categories = signal<Category[]>([]); protected readonly options = signal<ProductOption[]>([]); protected readonly result = signal<CatalogPage>({ items: [], totalCount: 0, page: 1, pageSize: 12, totalPages: 0 });
  protected readonly loading = signal(true); protected readonly error = signal(''); protected readonly filtersOpen = signal(false); protected readonly search = signal(''); protected readonly categoryIds = signal<string[]>([]); protected readonly subcategoryIds = signal<string[]>([]); protected readonly optionIds = signal<string[]>([]); protected readonly availability = signal<string[]>([]); protected readonly minimumPrice = signal<number | null>(null); protected readonly maximumPrice = signal<number | null>(null); protected readonly minimumDiscount = signal<number | null>(null); protected readonly sort = signal('newest');
  protected readonly collectionTitle = signal('Shop all jewellery'); protected readonly collectionEyebrow = signal('K Square collection'); protected readonly collectionDescription = signal('Explore our latest chains, rings, pendants and more.'); protected readonly lockedCollection = signal(false); private readonly lockedCategoryIds = signal<string[]>([]); private readonly allowedCollectionOptionTypes = signal<string[]>([]);
  protected readonly priceError = signal('');
  protected readonly collectionHasOptions = computed(() => this.allowedCollectionOptionTypes().length > 0);
  protected readonly availabilityChoices: MultiSelectFilterOption[] = [{ id: 'available', name: 'Available' }, { id: 'unavailable', name: 'Unavailable' }];
  protected readonly categoryChoices = computed(() => this.categories().filter(item => !item.parentId).map(item => ({ id: item.id, name: item.name })));
  protected readonly subcategoryChoices = computed(() => this.categories().filter(item => !!item.parentId && (!this.categoryIds().length || this.categoryIds().includes(item.parentId!))).map(item => ({ id: item.id, name: item.name })));
  protected readonly optionChoices = computed(() => this.options().filter(item => !this.allowedCollectionOptionTypes().length || this.allowedCollectionOptionTypes().includes(item.type)).map(item => ({ id: item.id, name: `${this.optionLabel(item.type)} · ${item.name}` })));
  protected readonly activeFilterCount = computed(() => (this.lockedCollection() ? 0 : this.categoryIds().length) + this.subcategoryIds().length + this.optionIds().length + this.availability().length + (this.minimumPrice() !== null ? 1 : 0) + (this.maximumPrice() !== null ? 1 : 0) + (this.minimumDiscount() !== null ? 1 : 0));

  ngOnInit(): void {
    const data = this.route.snapshot.data; const collectionCategory = data['collectionCategory'] as string | undefined;
    this.lockedCollection.set(!!collectionCategory); this.collectionTitle.set((data['collectionTitle'] as string | undefined) ?? 'Shop all jewellery'); this.collectionEyebrow.set((data['collectionEyebrow'] as string | undefined) ?? 'K Square collection'); this.collectionDescription.set((data['collectionDescription'] as string | undefined) ?? 'Explore our latest chains, rings, pendants and more.'); this.allowedCollectionOptionTypes.set((data['collectionOptionTypes'] as string[] | undefined) ?? []);
    const params = this.route.snapshot.queryParamMap; this.minimumPrice.set(this.priceParam(params.get('minPrice'))); this.maximumPrice.set(this.priceParam(params.get('maxPrice'))); this.minimumDiscount.set(this.priceParam(params.get('discount'))); this.search.set(params.get('search') ?? ''); if (!collectionCategory) this.categoryIds.set(params.getAll('category')); this.subcategoryIds.set(params.getAll('subcategory')); this.optionIds.set(params.getAll('option')); this.availability.set(params.getAll('availability')); this.sort.set(params.get('sort') ?? 'newest');
    forkJoin({ categories: this.catalog.getCategories(), options: this.catalog.getOptions() }).subscribe({ next: response => { this.categories.set(response.categories); this.options.set(response.options); if (collectionCategory) { const matches = response.categories.filter(item => !item.parentId && matchesCollectionCategory(item.name, collectionCategory)); if (!matches.length) { this.loading.set(false); this.result.set({ items: [], totalCount: 0, page: 1, pageSize: 12, totalPages: 0 }); return; } this.lockedCategoryIds.set(matches.map(item=>item.id)); this.categoryIds.set(this.lockedCategoryIds()); } this.load(Number(params.get('page') ?? 1), false); }, error: error => { this.loading.set(false); this.error.set(this.message(error)); } });
  }
  protected retry(): void { if (!this.categories().length || (this.lockedCollection() && !this.lockedCategoryIds().length)) this.ngOnInit(); else this.load(this.result().page, false); }
  protected submitSearch(event: Event): void { event.preventDefault(); this.load(1); }
  protected selectCategories(values: string[]): void { if (this.lockedCollection()) return; this.categoryIds.set(values); const valid = new Set(this.subcategoryChoices().map(item => item.id)); this.subcategoryIds.update(ids => ids.filter(id => valid.has(id))); }
  protected setSubcategories(values: string[]): void { this.subcategoryIds.set(values); }
  protected setOptions(values: string[]): void { this.optionIds.set(values); }
  protected setAvailability(values: string[]): void { this.availability.set(values); }
  protected setPrice(target: 'minimum' | 'maximum', value: string): void { const parsed = value === '' ? null : Number(value); target === 'minimum' ? this.minimumPrice.set(parsed) : this.maximumPrice.set(parsed); }
  protected applyFilters(): void { this.priceError.set(''); const min = this.minimumPrice(), max = this.maximumPrice(); if ((min !== null && (!Number.isFinite(min) || min < 0)) || (max !== null && (!Number.isFinite(max) || max < 0)) || (min !== null && max !== null && min > max)) { this.priceError.set('Enter a valid range. Minimum price must not exceed maximum price.'); return; } this.filtersOpen.set(false); this.filterSnapshot = undefined; this.load(1); }
  protected setDiscount(value: string): void { this.minimumDiscount.set(value === '' ? null : Number(value)); }
  protected setSort(value: string): void { this.sort.set(value); this.load(1); }
  protected goToPage(page: number): void { if (page >= 1 && page <= this.result().totalPages && page !== this.result().page) { this.load(page); window.scrollTo({ top: 0, behavior: 'smooth' }); } }
  protected clearFilters(): void { this.priceError.set(''); if (!this.filtersOpen()) this.search.set(''); this.categoryIds.set(this.lockedCollection() ? this.lockedCategoryIds() : []); this.subcategoryIds.set([]); this.optionIds.set([]); this.availability.set([]); this.minimumPrice.set(null); this.maximumPrice.set(null); this.minimumDiscount.set(null); if (!this.filtersOpen()) { this.sort.set('newest'); this.load(1); } }
  private filterSnapshot?: ReturnType<CatalogComponent['captureFilters']>;
  private captureFilters() { return { categories: [...this.categoryIds()], subcategories: [...this.subcategoryIds()], options: [...this.optionIds()], availability: [...this.availability()], minimum: this.minimumPrice(), maximum: this.maximumPrice(), discount: this.minimumDiscount() }; }
  protected openFilters(): void { this.filterSnapshot = this.captureFilters(); this.priceError.set(''); this.filtersOpen.set(true); }
  protected closeFilters(): void { const saved = this.filterSnapshot; if (saved) { this.categoryIds.set(saved.categories); this.subcategoryIds.set(saved.subcategories); this.optionIds.set(saved.options); this.availability.set(saved.availability); this.minimumPrice.set(saved.minimum); this.maximumPrice.set(saved.maximum); this.minimumDiscount.set(saved.discount); } this.filterSnapshot = undefined; this.priceError.set(''); this.filtersOpen.set(false); }
  private load(page: number, updateUrl = true): void {
    if (this.lockedCollection() && !this.lockedCategoryIds().length) { this.loading.set(false); return; } this.loading.set(true); this.error.set(''); const query = { search: this.search().trim(), categoryIds: this.categoryIds(), subcategoryIds: this.subcategoryIds(), optionIds: this.optionIds(), availability: this.availability(), minimumPrice: this.minimumPrice(), maximumPrice: this.maximumPrice(), minimumDiscount: this.minimumDiscount(), sort: this.sort(), page, pageSize: 12 };
    if (updateUrl) void this.router.navigate([], { relativeTo: this.route, queryParams: { minPrice: query.minimumPrice, maxPrice: query.maximumPrice, discount: query.minimumDiscount, search: query.search || null, category: query.categoryIds.length ? query.categoryIds : null, subcategory: query.subcategoryIds.length ? query.subcategoryIds : null, option: query.optionIds.length ? query.optionIds : null, availability: query.availability.length ? query.availability : null, sort: query.sort === 'newest' ? null : query.sort, page: page > 1 ? page : null }, replaceUrl: true });
    this.catalog.getProducts(query).pipe(finalize(() => this.loading.set(false))).subscribe({ next: result => this.result.set(result), error: error => this.error.set(this.message(error)) });
  }
  private priceParam(value: string | null): number | null { return value !== null && value !== '' && Number.isFinite(Number(value)) && Number(value) >= 0 ? Number(value) : null; }
  private optionLabel(type: string): string { return ({ PendantSize: 'Pendant', ChainSize: 'Chain size', ChainWidth: 'Chain width', ChainDiamondSize: 'Diamond size', BraceletStoneSize: 'Stone size', BraceletSize: 'Bracelet size', RingSize: 'Ring', Color: 'Color' } as Record<string, string>)[type] ?? type; }
  private message(error: unknown): string { return error instanceof HttpErrorResponse ? error.error?.error ?? 'Products could not be loaded.' : 'Products could not be loaded.'; }
}
