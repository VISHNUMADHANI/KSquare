import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AdminCatalogService } from '../../../core/catalog/admin-catalog.service';
import { Product } from '../../../core/catalog/catalog.models';
import { KsButtonDirective, MultiSelectFilterComponent, MultiSelectFilterOption, PriceDisplayComponent, StatePanelComponent, StatusBadgeComponent } from '../../../shared/ui';
import { ProductFormComponent } from './product-form.component';

@Component({ selector: 'app-product-list', standalone: true, imports: [DialogFocusDirective, RouterLink, ProductFormComponent, KsButtonDirective, MultiSelectFilterComponent, PriceDisplayComponent, StatePanelComponent, StatusBadgeComponent], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './product-list.component.html', styleUrl: './product-list.component.scss' })
export class ProductListComponent implements OnInit {
  @ViewChild(ProductFormComponent) private editor?: ProductFormComponent;
  protected dismissEditor(): void { this.editor?.close(); }
  private readonly catalog = inject(AdminCatalogService);
  protected readonly products = signal<Product[]>([]); protected readonly loading = signal(true); protected readonly error = signal('');
  protected readonly search = signal(''); protected readonly categoryFilters = signal<string[]>([]); protected readonly subcategoryFilters = signal<string[]>([]); protected readonly optionFilters = signal<string[]>([]); protected readonly availabilityFilters = signal<string[]>([]); protected readonly statusFilters = signal<string[]>([]);
  protected readonly duplicating = signal(false);
  protected duplicateProduct(event: Event, product: Product): void { event.stopPropagation(); this.openEditor(product.id); this.duplicating.set(true); }
  protected readonly editorOpen = signal(false); protected readonly editingId = signal<string | null>(null);
  protected readonly availabilityChoices: MultiSelectFilterOption[] = [{ id: 'available', name: 'Available' }, { id: 'unavailable', name: 'Unavailable' }];
  protected readonly statusChoices: MultiSelectFilterOption[] = [{ id: 'active', name: 'Active' }, { id: 'draft', name: 'Draft' }];
  protected readonly categoryChoices = computed(() => this.unique(this.products().map(product => ({ id: product.categoryId, name: product.categoryName }))));
  protected readonly subcategoryChoices = computed(() => this.unique(this.products().filter(product => !this.categoryFilters().length || this.categoryFilters().includes(product.categoryId)).flatMap(product => product.subcategories.map(item => ({ id: item.id, name: item.name })))));
  protected readonly optionChoices = computed(() => this.unique(this.products().flatMap(product => product.options.map(item => ({ id: item.id, name: item.name })))));
  protected readonly filteredProducts = computed(() => {
    const query = this.search().trim().toLocaleLowerCase();
    return this.products().filter(product =>
      (!query || product.name.toLocaleLowerCase().includes(query) || product.description.toLocaleLowerCase().includes(query)) &&
      (!this.categoryFilters().length || this.categoryFilters().includes(product.categoryId)) &&
      (!this.subcategoryFilters().length || product.subcategories.some(item => this.subcategoryFilters().includes(item.id))) &&
      (!this.optionFilters().length || product.options.some(item => this.optionFilters().includes(item.id))) &&
      (!this.availabilityFilters().length || this.availabilityFilters().includes(product.isAvailable ? 'available' : 'unavailable')) &&
      (!this.statusFilters().length || this.statusFilters().includes(product.isActive ? 'active' : 'draft')));
  });
  ngOnInit(): void { this.load(); }
  protected load(): void { this.loading.set(true); this.error.set(''); this.catalog.getProducts().pipe(finalize(() => this.loading.set(false))).subscribe({ next: value => this.products.set(value), error: error => this.error.set(error instanceof HttpErrorResponse ? error.error?.error ?? 'Products could not be loaded.' : 'Products could not be loaded.') }); }
  protected selectCategories(values: string[]): void { this.categoryFilters.set(values); const valid = new Set(this.subcategoryChoices().map(item => item.id)); this.subcategoryFilters.update(selected => selected.filter(id => valid.has(id))); }
  protected clearFilters(): void { this.search.set(''); this.categoryFilters.set([]); this.subcategoryFilters.set([]); this.optionFilters.set([]); this.availabilityFilters.set([]); this.statusFilters.set([]); }
  protected addProduct(): void { this.openEditor(null); }
  protected editProduct(product: Product): void { this.openEditor(product.id); }
  protected closeEditor(): void { this.editorOpen.set(false); this.editingId.set(null); this.duplicating.set(false); }
  protected saved(): void { this.load(); this.closeEditor(); }
  canLeaveProducts(): boolean { if (!this.editorOpen()) return true; this.dismissEditor(); return false; }
  protected remove(event: Event, product: Product): void { event.stopPropagation(); if (!confirm(`Delete product “${product.name}”? Its uploaded images will also be removed.`)) return; this.catalog.deleteProduct(product.id).subscribe({ next: () => this.load(), error: () => this.error.set('The product could not be deleted.') }); }
  private unique(items: { id: string; name: string }[]): { id: string; name: string }[] { return [...new Map(items.map(item => [item.id, item])).values()].sort((a, b) => a.name.localeCompare(b.name)); }
  private openEditor(productId: string | null): void { if (this.editorOpen()) return; this.editingId.set(productId); this.editorOpen.set(true); }
}
