import { isPendantCategory } from '../../../core/catalog/category-names';
import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, EventEmitter, Input, OnInit, Output, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { concatMap, finalize, forkJoin, from, map, Observable, of, switchMap, tap, toArray } from 'rxjs';
import { AdminCatalogService } from '../../../core/catalog/admin-catalog.service';
import { Category, Product, ProductMedia, ProductOption, ProductOptionType, SaveProduct, SaveProductBraceletVariant, SaveProductChainVariant, SaveProductPendantVariant } from '../../../core/catalog/catalog.models';
import { KsButtonDirective } from '../../../shared/ui';

@Component({ selector: 'app-product-form', standalone: true, imports: [ReactiveFormsModule, RouterLink, CurrencyPipe, KsButtonDirective], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './product-form.component.html', styleUrls: ['./product-form.component.scss', './product-chain-pricing.component.scss','./product-editor.component.scss'] })
export class ProductFormComponent implements OnInit {
  @Input() duplicateMode = false;
  private duplicateBaseline: string | null = null;
  protected readonly copiedMedia = signal<ProductMedia[]>([]);
  @Input() dialogMode = false; @Input() dialogProductId: string | null = null; @Output() readonly cancelled = new EventEmitter<void>(); @Output() readonly completed = new EventEmitter<Product>();
  private readonly catalog = inject(AdminCatalogService); private readonly route = inject(ActivatedRoute); private readonly router = inject(Router); private readonly destroyRef = inject(DestroyRef);
  private readonly previewUrls = new Map<File, string>();
  protected readonly productId = signal<string | null>(null); protected readonly product = signal<Product | null>(null); protected readonly categories = signal<Category[]>([]); protected readonly options = signal<ProductOption[]>([]);
  protected readonly selectedCategoryId = signal(''); protected readonly selectedFiles = signal<File[]>([]);
  protected readonly namePersonalization = signal(false);
  protected readonly pendantVariants = signal<SaveProductPendantVariant[]>([]);
  protected readonly isPendantCategory = computed(()=>isPendantCategory(this.categories().find(c=>c.id===this.selectedCategoryId())?.name??''));
  protected readonly hasVariantPricing = computed(()=>this.chainVariants().length>0||this.braceletVariants().length>0||this.pendantVariants().length>0);
  protected readonly chainVariants = signal<SaveProductChainVariant[]>([]);
  protected readonly braceletVariants = signal<SaveProductBraceletVariant[]>([]);
  protected readonly loading = signal(true); protected readonly saving = signal(false); protected readonly error = signal('');
  protected readonly topCategories = computed(() => this.categories().filter(category => !category.parentId && category.isActive));
  protected readonly availableSubcategories = computed(() => this.categories().filter(category => category.parentId === this.selectedCategoryId() && category.isActive));
  protected readonly optionGroups = computed(() => this.allowedOptionGroups(this.selectedCategoryId()).filter(group => !this.namePersonalization() || group.type !== 'PendantSize').map(group => ({ ...group, options: this.options().filter(option => option.type === group.type && option.isActive) })).filter(group => group.options.length));
  protected readonly isChainCategory = computed(() => (this.categories().find(category => category.id === this.selectedCategoryId())?.name.toLowerCase() ?? '').includes('chain'));
  protected readonly isBraceletCategory = computed(() => (this.categories().find(category => category.id === this.selectedCategoryId())?.name.toLowerCase() ?? '').includes('brace'));
  protected readonly hasChainWidths = computed(() => this.chainVariants().some(variant => !!variant.chainWidthOptionId));
  protected readonly hasChainDiamondSizes = computed(() => this.chainVariants().some(variant => !!variant.chainDiamondSizeOptionId));
  protected readonly hasBraceletSizes = computed(() => this.braceletVariants().some(variant => !!variant.braceletSizeOptionId));
  protected readonly hasBraceletStoneSizes = computed(() => this.braceletVariants().some(variant => !!variant.braceletStoneSizeOptionId));
  protected readonly totalImageCount = computed(() => (this.product()?.media.length ?? 0) + this.copiedMedia().length + this.selectedFiles().length);
  protected readonly form = new FormGroup({
    name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(180)] }),
    categoryId: new FormControl('', { nonNullable: true, validators: [Validators.required] }), subcategoryIds: new FormControl<string[]>([], { nonNullable: true }),
    description: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(4000)] }), originalPrice: new FormControl(0, { nonNullable: true, validators: [Validators.required, Validators.min(.01)] }),
    discountPercentage: new FormControl(0, { nonNullable: true, validators: [Validators.min(0), Validators.max(100)] }), isAvailable: new FormControl(true, { nonNullable: true }), isActive: new FormControl(true, { nonNullable: true }), isFinalSale: new FormControl(false, { nonNullable: true }), showInCustom: new FormControl(false, { nonNullable: true }), supportsNamePersonalization: new FormControl(false, { nonNullable: true }), includedNameLetters: new FormControl(1, {nonNullable:true, validators:[Validators.required,Validators.min(1),Validators.max(8),Validators.pattern(/^[1-8]$/)]}), namePricePerLetter: new FormControl(0, { nonNullable: true, validators: [Validators.min(0)] }), optionIds: new FormControl<string[]>([], { nonNullable: true })
  });
  protected readonly previewPrice = signal(0);

  ngOnInit(): void {
    this.destroyRef.onDestroy(() => this.clearPendingFiles());
    this.form.controls.supportsNamePersonalization.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(enabled => { this.namePersonalization.set(enabled); this.rebuildPendantVariants(); });
    this.form.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(value => this.previewPrice.set(Math.round(((value.originalPrice ?? 0) * (100 - (value.discountPercentage ?? 0)) / 100) * 100) / 100));
    this.form.controls.categoryId.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(categoryId => { this.selectedCategoryId.set(categoryId); if(!this.isPendantCategory())this.namePersonalization.set(false); if(!this.isPendantCategory())this.form.patchValue({supportsNamePersonalization:false,namePricePerLetter:0},{emitEvent:false}); const allowed = new Set(this.categories().filter(item => item.parentId === categoryId).map(item => item.id)); this.form.controls.subcategoryIds.setValue(this.form.controls.subcategoryIds.value.filter(id => allowed.has(id)), { emitEvent: false }); const allowedTypes = new Set(this.allowedOptionGroups(categoryId).map(group => group.type)); this.form.controls.optionIds.setValue(this.form.controls.optionIds.value.filter(id => { const option = this.options().find(item => item.id === id); return !!option && allowedTypes.has(option.type); }), { emitEvent: false }); this.rebuildChainVariants(); this.rebuildBraceletVariants(); this.rebuildPendantVariants(); });
    this.productId.set(this.duplicateMode ? null : this.dialogMode ? this.dialogProductId : this.route.snapshot.paramMap.get('id'));
    forkJoin({ categories: this.catalog.getCategories(), options: this.catalog.getProductOptions() }).subscribe({ next: value => { this.categories.set(value.categories); this.options.set(value.options); this.loadProduct(); }, error: error => { this.loading.set(false); this.error.set(this.message(error)); } });
  }

  protected save(): void {
    if (this.saving() || this.loading() || (this.duplicateMode && !this.hasDuplicateChanges())) return;
    if (this.form.invalid) { this.form.markAllAsTouched(); this.error.set('Complete the required details and enter a valid price.'); return; }
    if(this.isPendantCategory()&&this.form.controls.isActive.value&&this.pendantVariants().some(v=>v.originalPrice<=0)){this.error.set('Enter a price for every selected pendant size, or remove all sizes to use one price.');return;}
    if (this.isChainCategory() && this.form.controls.isActive.value && (!this.chainVariants().length || this.chainVariants().some(variant => variant.originalPrice <= 0))) { this.error.set('Select at least one Chain size and enter a price greater than zero for every generated combination before activating the product.'); return; }
    if (this.isBraceletCategory() && this.form.controls.isActive.value && this.braceletVariants().some(variant => variant.originalPrice <= 0)) { this.error.set('Enter a price greater than zero for every generated Bracelet combination before activating the product.'); return; }
    if (this.totalImageCount() > 6) { this.error.set('A product can have a maximum of 6 photos and videos combined.'); return; }
    const value = this.form.getRawValue();
    if (value.isActive && !(this.product()?.media.some(m => m.contentType.startsWith('image/')) || this.copiedMedia().some(m => m.contentType.startsWith('image/')) || this.selectedFiles().some(f => f.type.startsWith('image/')))) { this.error.set('Select at least one image before activating the product, or save it as an inactive draft.'); return; }
    const configuredVariants = this.isChainCategory() ? this.chainVariants().filter(variant => variant.originalPrice > 0) : [];
    const configuredBraceletVariants = this.isBraceletCategory() ? this.braceletVariants().filter(variant => variant.originalPrice > 0) : [];
    const configuredPendantVariants=this.isPendantCategory()?this.pendantVariants().filter(v=>v.originalPrice>0):[];
    const combinationPrices = [...configuredVariants, ...configuredBraceletVariants, ...configuredPendantVariants].map(variant => variant.originalPrice);
    const startingPrice = this.isPendantCategory() && value.supportsNamePersonalization ? value.originalPrice : combinationPrices.length ? Math.min(...combinationPrices) : value.originalPrice;
    this.saving.set(true); this.error.set(''); const request: SaveProduct = { ...value, supportsNamePersonalization:this.isPendantCategory()&&value.supportsNamePersonalization, nameFixedPrice:this.isPendantCategory()&&value.supportsNamePersonalization?value.originalPrice:0, namePricePerLetter:this.isPendantCategory()?value.namePricePerLetter:0, originalPrice: startingPrice, sku: this.product()?.sku ?? null, chainVariants: configuredVariants, braceletVariants: configuredBraceletVariants, pendantVariants: configuredPendantVariants };
    const needsDraftStage = !this.productId() || (value.isActive && (this.product()?.media.length ?? 0) === 0 && this.selectedFiles().length > 0);
    const firstRequest = needsDraftStage ? { ...request, isActive: false } : request;
    const wasNew = !this.productId();
    const initial = this.productId() ? this.catalog.updateProduct(this.productId()!, firstRequest) : this.catalog.createProduct(firstRequest);
    this.prepareCopiedMedia().pipe(
      switchMap(() => initial),
      tap(product => { this.product.set(product); if (wasNew) this.productId.set(product.id); }),
      switchMap(product => this.uploadSelected(product)),
      switchMap(product => request.isActive && !product.isActive ? this.catalog.updateProduct(product.id, request) : this.catalog.getProduct(product.id)),
      finalize(() => this.saving.set(false))
    ).subscribe({ next: product => { this.product.set(product); this.selectedFiles.set([]); this.form.markAsPristine(); if (this.dialogMode) this.completed.emit(product); else if (wasNew) this.router.navigate(['/admin/products', product.id, 'edit'], { replaceUrl: true }); }, error: error => this.error.set(this.message(error)) });
  }

  protected chooseFiles(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []); input.value = ''; this.error.set('');
    if (files.some(file => !['image/jpeg', 'image/png', 'image/webp', 'video/mp4', 'video/webm'].includes(file.type) || file.size <= 0 || file.size > (file.type.startsWith('video/') ? 25 : 10) * 1024 * 1024)) { this.error.set('Use JPG, PNG or WebP photos up to 10 MB, or MP4/WebM videos up to 25 MB.'); return; }
    const existing = this.selectedFiles();
    const existingKeys = new Set(existing.map(file => this.fileKey(file)));
    const newFiles = files.filter(file => !existingKeys.has(this.fileKey(file)));
    if ((this.product()?.media.length ?? 0) + this.copiedMedia().length + existing.length + newFiles.length > 6) { this.error.set(`You can add only ${6 - this.totalImageCount()} more file(s).`); return; }
    newFiles.forEach(file => this.previewUrls.set(file, URL.createObjectURL(file)));
    this.selectedFiles.set([...existing, ...newFiles]);
    if (newFiles.length) this.form.markAsDirty();
  }
  protected pendingPreview(file: File): string { return this.previewUrls.get(file) ?? ''; }
  protected removePending(index: number): void { const file = this.selectedFiles()[index]; if (file) this.revokePreview(file); this.selectedFiles.update(files => files.filter((_, itemIndex) => itemIndex !== index)); this.form.markAsDirty(); }
  close(): void { if (this.saving()) return; if (this.form.dirty && !confirm('Discard your unsaved changes?')) return; this.cancelled.emit(); }
  protected subcategorySelected(id: string): boolean { return this.form.controls.subcategoryIds.value.includes(id); }
  protected toggleSubcategory(id: string, checked: boolean): void { this.toggleArrayControl(this.form.controls.subcategoryIds, id, checked); }
  protected optionSelected(id: string): boolean { return this.form.controls.optionIds.value.includes(id); }
  protected toggleOption(id: string, checked: boolean): void { this.toggleArrayControl(this.form.controls.optionIds, id, checked); this.rebuildChainVariants(); this.rebuildBraceletVariants(); this.rebuildPendantVariants(); }
  protected optionName(id: string): string { return this.options().find(option => option.id === id)?.name ?? 'Unknown'; }
  protected updateChainPrice(index: number, value: string): void { const price = Number(value); this.chainVariants.update(items => items.map((item, itemIndex) => itemIndex === index ? { ...item, originalPrice: Number.isFinite(price) ? price : 0 } : item)); this.syncChainStartingPrice(); this.form.markAsDirty(); }
  protected updateChainAvailability(index: number, isAvailable: boolean): void { this.chainVariants.update(items => items.map((item, itemIndex) => itemIndex === index ? { ...item, isAvailable } : item)); this.form.markAsDirty(); }
  protected chainFinalPrice(price: number): number { return Math.round(price * (100 - this.form.controls.discountPercentage.value) / 100 * 100) / 100; }
  protected updatePendantPrice(index:number,value:string):void{const price=Number(value);this.pendantVariants.update(rows=>rows.map((row,i)=>i===index?{...row,originalPrice:Number.isFinite(price)?price:0}:row));this.syncPendantPrice();this.form.markAsDirty();}
  protected updatePendantAvailability(index:number,value:boolean):void{this.pendantVariants.update(rows=>rows.map((row,i)=>i===index?{...row,isAvailable:value}:row));this.form.markAsDirty();}
  private rebuildPendantVariants():void{if(!this.isPendantCategory()||this.form.controls.supportsNamePersonalization.value){this.form.controls.optionIds.setValue(this.form.controls.optionIds.value.filter(id=>this.options().find(o=>o.id===id)?.type!=='PendantSize'),{emitEvent:false});this.pendantVariants.set([]);return;}const previous=new Map(this.pendantVariants().map(v=>[v.pendantSizeOptionId,v]));this.pendantVariants.set(this.selectedOptionsOfType('PendantSize').map(o=>previous.get(o.id)??{pendantSizeOptionId:o.id,originalPrice:0,isAvailable:true}));this.syncPendantPrice();}
  private syncPendantPrice():void{const prices=this.pendantVariants().map(v=>v.originalPrice).filter(p=>p>0);if(prices.length)this.form.controls.originalPrice.setValue(Math.min(...prices));}
  protected updateBraceletPrice(index: number, value: string): void { const price = Number(value); this.braceletVariants.update(items => items.map((item, itemIndex) => itemIndex === index ? { ...item, originalPrice: Number.isFinite(price) ? price : 0 } : item)); this.syncBraceletStartingPrice(); this.form.markAsDirty(); }
  protected updateBraceletAvailability(index: number, isAvailable: boolean): void { this.braceletVariants.update(items => items.map((item, itemIndex) => itemIndex === index ? { ...item, isAvailable } : item)); this.form.markAsDirty(); }
  protected deleteMedia(mediaId: string): void { const id = this.productId(); if (!id || !confirm('Delete this product photo or video?')) return; this.catalog.deleteMedia(id, mediaId).subscribe({ next: () => { const current = this.product(); if (current) this.product.set({ ...current, media: current.media.filter(media => media.id !== mediaId) }); }, error: error => this.error.set(this.message(error)) }); }

  protected removeCopiedMedia(id: string): void { this.copiedMedia.update(items => items.filter(item => item.id !== id)); this.form.markAsDirty(); }
  private prepareCopiedMedia(): Observable<void> {
    const media = this.copiedMedia();
    if (!media.length || !this.dialogProductId) return of(undefined);
    // Fetch originals only when saving. Opening the form needs only lightweight metadata.
    return from(media).pipe(concatMap(item => this.catalog.downloadMedia(this.dialogProductId!, item.id).pipe(map(blob => new File([blob], item.objectKey.split('/').pop() || item.id, {type: item.contentType, lastModified: 0})))), toArray(), tap(files => {
      files.forEach(file => this.previewUrls.set(file, URL.createObjectURL(file)));
      this.selectedFiles.update(existing => [...files, ...existing]);
      this.copiedMedia.set([]);
    }), map(() => undefined));
  }
  private uploadSelected(product: Product): Observable<Product> {
    const files = [...this.selectedFiles()]; if (!files.length) return of(product);
    return from(files).pipe(concatMap(file => this.catalog.uploadMedia(product.id, file, this.form.controls.name.value).pipe(tap(media => { this.revokePreview(file); this.selectedFiles.update(current => current.filter(item => item !== file)); this.product.update(current => current ? { ...current, media: [...current.media, media] } : current); }))), toArray(), map(media => ({ ...product, media: [...product.media, ...media] })));
  }
  private fileKey(file: File): string { return `${file.name}:${file.size}:${file.lastModified}`; }
  private revokePreview(file: File): void { const url = this.previewUrls.get(file); if (url) URL.revokeObjectURL(url); this.previewUrls.delete(file); }
  private clearPendingFiles(): void { this.selectedFiles().forEach(file => this.revokePreview(file)); }
  private allowedOptionGroups(categoryId: string): { type: ProductOption['type']; label: string }[] { const name = this.categories().find(category => category.id === categoryId)?.name.toLowerCase() ?? ''; if (name.includes('brace')) return [{ type: 'BraceletStoneSize', label: 'Stone sizes' }, { type: 'BraceletSize', label: 'Bracelet sizes' }, { type: 'Color', label: 'Colors' }]; if (name.includes('ring')) return [{ type: 'RingSize', label: 'Ring sizes' }, { type: 'Color', label: 'Colors' }]; if (isPendantCategory(name)) return [{ type: 'PendantSize', label: 'Pendant sizes' }, { type: 'Color', label: 'Colors' }]; if (name.includes('chain')) return [{ type: 'ChainSize', label: 'Chain sizes' }, { type: 'ChainWidth', label: 'Chain widths' }, { type: 'ChainDiamondSize', label: 'Chain diamond sizes' }]; return []; }
  private selectedOptionsOfType(type: ProductOptionType): ProductOption[] { const selected = new Set(this.form.controls.optionIds.value); return this.options().filter(option => option.type === type && selected.has(option.id)); }
  private rebuildChainVariants(): void {
    if (!this.isChainCategory()) { this.chainVariants.set([]); return; }
    const existing = new Map(this.chainVariants().map(variant => [this.chainVariantKey(variant), variant]));
    const variants: SaveProductChainVariant[] = [];
    const widths: (ProductOption | null)[] = this.selectedOptionsOfType('ChainWidth'); if (!widths.length) widths.push(null);
    const diamonds: (ProductOption | null)[] = this.selectedOptionsOfType('ChainDiamondSize'); if (!diamonds.length) diamonds.push(null);
    for (const size of this.selectedOptionsOfType('ChainSize')) for (const width of widths) for (const diamond of diamonds) {
      const draft: SaveProductChainVariant = { chainSizeOptionId: size.id, chainWidthOptionId: width?.id ?? null, chainDiamondSizeOptionId: diamond?.id ?? null, originalPrice: 0, isAvailable: true };
      variants.push(existing.get(this.chainVariantKey(draft)) ?? draft);
    }
    this.chainVariants.set(variants); this.syncChainStartingPrice();
  }
  private chainVariantKey(variant: SaveProductChainVariant): string { return `${variant.chainSizeOptionId}:${variant.chainWidthOptionId}:${variant.chainDiamondSizeOptionId}`; }
  private syncChainStartingPrice(): void { const prices = this.chainVariants().map(variant => variant.originalPrice).filter(price => price > 0); if (prices.length) this.form.controls.originalPrice.setValue(Math.min(...prices), { emitEvent: true }); }
  private rebuildBraceletVariants(): void {
    if (!this.isBraceletCategory()) { this.braceletVariants.set([]); return; }
    const existing = new Map(this.braceletVariants().map(variant => [this.braceletVariantKey(variant), variant]));
    const variants: SaveProductBraceletVariant[] = [];
    const sizes: (ProductOption | null)[] = this.selectedOptionsOfType('BraceletSize');
    const stones: (ProductOption | null)[] = this.selectedOptionsOfType('BraceletStoneSize');
    if (!sizes.length && !stones.length) { this.braceletVariants.set([]); return; }
    if (!sizes.length) sizes.push(null); if (!stones.length) stones.push(null);
    for (const size of sizes) for (const stone of stones) {
      const draft: SaveProductBraceletVariant = { braceletSizeOptionId: size?.id ?? null, braceletStoneSizeOptionId: stone?.id ?? null, originalPrice: 0, isAvailable: true };
      variants.push(existing.get(this.braceletVariantKey(draft)) ?? draft);
    }
    this.braceletVariants.set(variants); this.syncBraceletStartingPrice();
  }
  private braceletVariantKey(variant: SaveProductBraceletVariant): string { return `${variant.braceletSizeOptionId}:${variant.braceletStoneSizeOptionId}`; }
  private syncBraceletStartingPrice(): void { const prices = this.braceletVariants().map(variant => variant.originalPrice).filter(price => price > 0); if (prices.length) this.form.controls.originalPrice.setValue(Math.min(...prices), { emitEvent: true }); }
  private toggleArrayControl(control: FormControl<string[]>, id: string, checked: boolean): void { const current = control.value; control.setValue(checked ? [...current, id] : current.filter(value => value !== id)); control.markAsDirty(); }
  private loadProduct(): void { const id = this.duplicateMode ? this.dialogProductId : this.productId(); if (!id) { this.loading.set(false); return; } this.catalog.getProduct(id).pipe(takeUntilDestroyed(this.destroyRef), finalize(() => this.loading.set(false))).subscribe({ next: product => { this.product.set(this.duplicateMode ? null : product); if(this.duplicateMode) this.copiedMedia.set(product.media); this.selectedCategoryId.set(product.categoryId); this.form.reset({ name: product.name, categoryId: product.categoryId, subcategoryIds: product.subcategories.map(item => item.id), description: product.description, originalPrice: product.originalPrice, discountPercentage: product.discountPercentage, isAvailable: product.isAvailable, isActive: product.isActive, isFinalSale: product.isFinalSale ?? false, showInCustom: product.showInCustom, supportsNamePersonalization: product.supportsNamePersonalization, includedNameLetters: product.includedNameLetters ?? 1, namePricePerLetter: product.namePricePerLetter, optionIds: product.options.map(option => option.id) }); this.chainVariants.set(product.chainVariants.map(variant => ({ chainSizeOptionId: variant.chainSizeOptionId, chainWidthOptionId: variant.chainWidthOptionId, chainDiamondSizeOptionId: variant.chainDiamondSizeOptionId, originalPrice: variant.originalPrice, isAvailable: variant.isAvailable }))); this.braceletVariants.set(product.braceletVariants.map(variant => ({ braceletSizeOptionId: variant.braceletSizeOptionId, braceletStoneSizeOptionId: variant.braceletStoneSizeOptionId, originalPrice: variant.originalPrice, isAvailable: variant.isAvailable }))); this.rebuildChainVariants(); this.rebuildBraceletVariants(); this.rebuildPendantVariants(); this.pendantVariants.set((product.pendantVariants??[]).map(v=>({pendantSizeOptionId:v.pendantSizeOptionId,originalPrice:v.originalPrice,isAvailable:v.isAvailable})));if(!product.pendantVariants?.length&&this.isPendantCategory()){this.pendantVariants.set(this.selectedOptionsOfType('PendantSize').map(o=>({pendantSizeOptionId:o.id,originalPrice:product.supportsNamePersonalization?product.nameFixedPrice:product.originalPrice,isAvailable:product.isAvailable})));}this.rebuildPendantVariants();this.previewPrice.set(product.finalPrice); this.form.markAsPristine(); if(this.duplicateMode) this.duplicateBaseline = this.duplicateSnapshot(); }, error: error => this.error.set(this.message(error)) }); }
  protected hasDuplicateChanges(): boolean { return this.duplicateBaseline !== null && this.duplicateBaseline !== this.duplicateSnapshot(); }
  private duplicateSnapshot(): string {
    const value = this.form.getRawValue();
    const sortRows = (rows: object[]) => rows.map(row => JSON.stringify(row)).sort();
    return JSON.stringify({ ...value, name: value.name.trim(), description: value.description.trim(), optionIds: [...value.optionIds].sort(), subcategoryIds: [...value.subcategoryIds].sort(), chains: sortRows(this.chainVariants()), bracelets: sortRows(this.braceletVariants()), pendants: sortRows(this.pendantVariants()), files: this.selectedFiles().map(file => this.fileKey(file)), copiedMedia: this.copiedMedia().map(media => media.id), media: this.product()?.media.map(media => media.id) ?? [] });
  }
  private message(error: unknown): string { return error instanceof HttpErrorResponse ? error.error?.error ?? 'The request could not be completed.' : 'The request could not be completed.'; }
}
