import { ReturnPolicyComponent } from '../return-policy/return-policy.component';
import { ProductReviewsComponent } from '../reviews/product-reviews.component';
import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
import { ProductThumbnailComponent } from '../../../shared/ui/product-thumbnail.component';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CartService } from '../../../core/orders/cart.service';
import { switchMap } from 'rxjs';
import { Category, Product, ProductOptionSummary } from '../../../core/catalog/catalog.models';
import { StorefrontCatalogService } from '../../../core/catalog/storefront-catalog.service';
import { PriceDisplayComponent, ProductCardComponent, StatePanelComponent } from '../../../shared/ui';

@Component({ selector: 'app-storefront-product-detail', standalone: true, imports: [ReturnPolicyComponent, ProductReviewsComponent, DialogFocusDirective, ProductThumbnailComponent, CurrencyPipe, RouterLink, PriceDisplayComponent, ProductCardComponent, StatePanelComponent], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './storefront-product-detail.component.html', styleUrls: ['./storefront-product-detail.component.scss', './storefront-chain-options.component.scss'] })
export class StorefrontProductDetailComponent implements OnInit {
 protected readonly infoDialog = signal<'details'|'returns'|null>(null);
 protected scrollReviews(){document.getElementById('customer-reviews')?.scrollIntoView({behavior:'smooth'});}
 protected reviewSummary = signal<{total:number;average:number}|null>(null);
  private readonly route = inject(ActivatedRoute); private readonly catalog = inject(StorefrontCatalogService);
  protected readonly cart = inject(CartService);
  private readonly destroyRef=inject(DestroyRef);
  private readonly customCategories=signal<Category[]>([]);
  protected readonly showCustomDesign=computed(()=>this.customCategories().some(category=>category.id===this.product()?.categoryId&&category.isActive&&category.showInCustom));
  protected readonly personalizationNote = signal('');
  protected readonly addedToCart = signal(false);
  protected readonly canAddToCart = computed(() => {
    const item = this.product();
    return !!item?.isActive && !!item.isAvailable && this.displayedAvailability() && this.combinationSelectionComplete() && this.optionGroups().every(group => !!this.selectedOptions()[group.type])
      && (!item.supportsNamePersonalization || (/^[A-Za-z]{1,8}$/.test(this.pendantName()) && !this.nameError()))
      && this.personalizationNote().length <= 1000 && Number.isFinite(this.purchaseAmount()) && this.purchaseAmount() > 0;
  });
  protected readonly purchaseAmount = computed(() => this.product()?.supportsNamePersonalization ? this.personalizedPrice() : this.displayFinalPrice());
  protected addToCart(): void {
    const item = this.product(); if (!item || !this.canAddToCart()) return;
    const details = this.optionGroups().map(group => group.label + ': ' + (group.options.find(option => option.id === this.selectedOptions()[group.type])?.name ?? ''));
    if (item.supportsNamePersonalization) details.push('Name: ' + this.pendantName());
    const note=this.personalizationNote().trim();
    if(note) details.push("Personalization: " + note);
    this.addedToCart.set(this.cart.add({personalizationNote:note || null,imageUrl:item.media[0]?.url,productId:item.id,optionIds:Object.values(this.selectedOptions()),personalizedName:item.supportsNamePersonalization?this.pendantName():null,name:item.name,unitPrice:this.purchaseAmount(),details}));
  }
  protected readonly product = signal<Product | null>(null); protected readonly related = signal<Product[]>([]); protected readonly selectedImage = signal(0); protected readonly selectedOptions = signal<Record<string, string>>({}); protected readonly pendantName = signal(''); protected readonly nameError = signal(''); protected readonly loading = signal(true); protected readonly error = signal('');
  protected readonly optionGroups = computed(() => { const groups = new Map<string, ProductOptionSummary[]>(); for (const option of this.product()?.options ?? []) groups.set(option.type, [...(groups.get(option.type) ?? []), option]); return [...groups.entries()].map(([type, options]) => ({ type, label: this.label(type), options })); });
  protected readonly isPendantProduct = computed(()=>(this.product()?.pendantVariants?.length??0)>0);
  protected readonly selectedPendantVariant=computed(()=>this.product()?.pendantVariants?.find(v=>v.pendantSizeOptionId===this.selectedOptions()['PendantSize'])??null);
  protected readonly startingPendantVariant=computed(()=>[...(this.product()?.pendantVariants??[])].filter(v=>v.isAvailable).sort((a,b)=>a.finalPrice-b.finalPrice)[0]??null);
  protected readonly isChainProduct = computed(() => (this.product()?.chainVariants.length ?? 0) > 0);
  protected readonly isBraceletProduct = computed(() => (this.product()?.braceletVariants.length ?? 0) > 0);
  protected readonly isCombinationProduct = computed(() => this.isChainProduct() || this.isBraceletProduct() || this.isPendantProduct());
  protected readonly selectedChainVariant = computed(() => { const selected = this.selectedOptions(); if (!selected['ChainSize'] || (this.hasOption('ChainWidth') && !selected['ChainWidth']) || (this.hasOption('ChainDiamondSize') && !selected['ChainDiamondSize'])) return null; return this.product()?.chainVariants.find(variant => variant.chainSizeOptionId === selected['ChainSize'] && variant.chainWidthOptionId === (this.hasOption('ChainWidth') ? selected['ChainWidth'] : null) && variant.chainDiamondSizeOptionId === (this.hasOption('ChainDiamondSize') ? selected['ChainDiamondSize'] : null)) ?? null; });
  protected readonly startingChainVariant = computed(() => [...(this.product()?.chainVariants ?? [])].filter(variant => variant.isAvailable).sort((left, right) => left.finalPrice - right.finalPrice)[0] ?? null);
  protected readonly selectedBraceletVariant = computed(() => { const selected = this.selectedOptions(); if ((this.hasOption('BraceletSize') && !selected['BraceletSize']) || (this.hasOption('BraceletStoneSize') && !selected['BraceletStoneSize'])) return null; return this.product()?.braceletVariants.find(variant => variant.braceletSizeOptionId === (this.hasOption('BraceletSize') ? selected['BraceletSize'] : null) && variant.braceletStoneSizeOptionId === (this.hasOption('BraceletStoneSize') ? selected['BraceletStoneSize'] : null)) ?? null; });
  protected readonly startingBraceletVariant = computed(() => [...(this.product()?.braceletVariants ?? [])].filter(variant => variant.isAvailable).sort((left, right) => left.finalPrice - right.finalPrice)[0] ?? null);
  protected readonly displayOriginalPrice = computed(() => this.selectedPendantVariant()?.originalPrice ?? this.startingPendantVariant()?.originalPrice ?? this.selectedChainVariant()?.originalPrice ?? this.selectedBraceletVariant()?.originalPrice ?? this.startingChainVariant()?.originalPrice ?? this.startingBraceletVariant()?.originalPrice ?? this.product()?.originalPrice ?? 0);
  protected readonly displayFinalPrice = computed(() => this.selectedPendantVariant()?.finalPrice ?? this.startingPendantVariant()?.finalPrice ?? this.selectedChainVariant()?.finalPrice ?? this.selectedBraceletVariant()?.finalPrice ?? this.startingChainVariant()?.finalPrice ?? this.startingBraceletVariant()?.finalPrice ?? this.product()?.finalPrice ?? 0);
  protected readonly chainSelectionComplete = computed(() => !!this.selectedOptions()['ChainSize'] && (!this.hasOption('ChainWidth') || !!this.selectedOptions()['ChainWidth']) && (!this.hasOption('ChainDiamondSize') || !!this.selectedOptions()['ChainDiamondSize']));
  protected readonly chainSelectionPrompt = computed(() => { const missing = []; if (!this.selectedOptions()['ChainSize']) missing.push('size'); if (this.hasOption('ChainWidth') && !this.selectedOptions()['ChainWidth']) missing.push('width'); if (this.hasOption('ChainDiamondSize') && !this.selectedOptions()['ChainDiamondSize']) missing.push('diamond size'); return `Select ${missing.join(', ')}`; });
  protected readonly braceletSelectionComplete = computed(() => (!this.hasOption('BraceletSize') || !!this.selectedOptions()['BraceletSize']) && (!this.hasOption('BraceletStoneSize') || !!this.selectedOptions()['BraceletStoneSize']));
  protected readonly combinationSelectionComplete = computed(() => this.isPendantProduct()?!!this.selectedOptions()['PendantSize']:this.isChainProduct() ? this.chainSelectionComplete() : this.isBraceletProduct() ? this.braceletSelectionComplete() : true);
  protected readonly combinationSelectionPrompt = computed(() => { if(this.isPendantProduct())return 'Select pendant size'; if (this.isChainProduct()) return this.chainSelectionPrompt(); const missing = []; if (this.hasOption('BraceletStoneSize') && !this.selectedOptions()['BraceletStoneSize']) missing.push('stone size'); if (this.hasOption('BraceletSize') && !this.selectedOptions()['BraceletSize']) missing.push('bracelet size'); return `Select ${missing.join(', ')}`; });
  protected readonly displayedAvailability = computed(() => this.isPendantProduct()&&!!this.selectedOptions()['PendantSize']?!!this.selectedPendantVariant()?.isAvailable:this.isChainProduct() && this.chainSelectionComplete() ? !!this.selectedChainVariant()?.isAvailable : this.isBraceletProduct() && this.braceletSelectionComplete() ? !!this.selectedBraceletVariant()?.isAvailable : !!this.product()?.isAvailable);
  protected readonly personalizedOriginalPrice = computed(() => { const p=this.product(); return p ? p.nameFixedPrice + Math.max(0,this.pendantName().length-(p.includedNameLetters ?? 1))*p.namePricePerLetter : 0; });
  protected readonly personalizedPrice = computed(() => { const product = this.product(); const nameLength = this.pendantName().length; return product?.supportsNamePersonalization ? Math.round(this.personalizedOriginalPrice()*(100-product.discountPercentage))/100 : product?.finalPrice ?? 0; });
  ngOnInit(): void { this.catalog.getCategories().pipe(takeUntilDestroyed(this.destroyRef)).subscribe({next:categories=>this.customCategories.set(categories),error:()=>this.customCategories.set([])}); this.route.paramMap.pipe(switchMap(params => this.catalog.getProduct(params.get('id')!)), switchMap(product => { this.product.set(product); this.personalizationNote.set(''); this.pendantName.set(''); this.nameError.set(''); this.selectedImage.set(0); this.selectDefaultOptions(product); return this.catalog.getProducts({ search: '', categoryIds: [product.categoryId], subcategoryIds: [], optionIds: [], availability: [], minimumPrice: null, maximumPrice: null, minimumDiscount: null, sort: 'newest', page: 1, pageSize: 5 }); })).subscribe({ next: result => { this.related.set(result.items.filter(item => item.id !== this.product()?.id).slice(0, 4)); this.loading.set(false); }, error: () => { this.error.set('This product could not be found.'); this.loading.set(false); } }); }
  protected selectOption(type: string, id: string): void { this.selectedOptions.update(current => ({ ...current, [type]: id })); }
  protected optionSelected(type: string, id: string): boolean { return this.selectedOptions()[type] === id; }
  protected optionDisabled(type: string, id: string): boolean { const selected = { ...this.selectedOptions(), [type]: id }; if(this.isPendantProduct()&&type==='PendantSize')return !this.product()?.pendantVariants?.some(v=>v.pendantSizeOptionId===id&&v.isAvailable); if (this.isChainProduct()) return !(this.product()?.chainVariants.some(variant => variant.isAvailable && (!selected['ChainSize'] || variant.chainSizeOptionId === selected['ChainSize']) && (!selected['ChainWidth'] || variant.chainWidthOptionId === selected['ChainWidth']) && (!selected['ChainDiamondSize'] || variant.chainDiamondSizeOptionId === selected['ChainDiamondSize'])) ?? false); if (this.isBraceletProduct() && type !== 'Color') return !(this.product()?.braceletVariants.some(variant => variant.isAvailable && (!selected['BraceletSize'] || variant.braceletSizeOptionId === selected['BraceletSize']) && (!selected['BraceletStoneSize'] || variant.braceletStoneSizeOptionId === selected['BraceletStoneSize'])) ?? false); return false; }
  protected setName(value: string): void { if (!/^[A-Za-z]*$/.test(value)) { this.nameError.set('Use letters only. Spaces and symbols are not allowed.'); return; } this.nameError.set(''); this.pendantName.set(value.slice(0, 8)); }
  private selectDefaultOptions(product: Product): void {
    const pendant=[...(product.pendantVariants??[])].filter(v=>v.isAvailable).sort((a,b)=>a.finalPrice-b.finalPrice)[0];
    if(pendant){const selected:Record<string,string>={PendantSize:pendant.pendantSizeOptionId};for(const option of product.options)if(!selected[option.type])selected[option.type]=option.id;this.selectedOptions.set(selected);return;}
    const availableBracelets = product.braceletVariants.filter(variant => variant.isAvailable);
    const defaultBracelet = [...(availableBracelets.length ? availableBracelets : product.braceletVariants)].sort((left, right) => left.finalPrice - right.finalPrice)[0];
    if (defaultBracelet) {
      const selected: Record<string, string> = {};
      if (defaultBracelet.braceletSizeOptionId) selected['BraceletSize'] = defaultBracelet.braceletSizeOptionId;
      if (defaultBracelet.braceletStoneSizeOptionId) selected['BraceletStoneSize'] = defaultBracelet.braceletStoneSizeOptionId;
      const firstColor = product.options.find(option => option.type === 'Color'); if (firstColor) selected['Color'] = firstColor.id;
      for (const option of product.options) if (!selected[option.type]) selected[option.type] = option.id;
      this.selectedOptions.set(selected);
      return;
    }
    const availableVariants = product.chainVariants.filter(variant => variant.isAvailable);
    const defaultVariant = [...(availableVariants.length ? availableVariants : product.chainVariants)].sort((left, right) => left.finalPrice - right.finalPrice)[0];
    if (defaultVariant) {
      const selected: Record<string, string> = { ChainSize: defaultVariant.chainSizeOptionId };
      if (defaultVariant.chainWidthOptionId) selected['ChainWidth'] = defaultVariant.chainWidthOptionId;
      if (defaultVariant.chainDiamondSizeOptionId) selected['ChainDiamondSize'] = defaultVariant.chainDiamondSizeOptionId;
      for (const option of product.options) if (!selected[option.type]) selected[option.type] = option.id;
      this.selectedOptions.set(selected);
      return;
    }

    const selected: Record<string, string> = {};
    for (const option of product.options) if (!selected[option.type]) selected[option.type] = option.id;
    this.selectedOptions.set(selected);
  }
  private hasOption(type: string): boolean { return this.product()?.options.some(option => option.type === type) ?? false; }
  private label(type: string): string { return ({ PendantSize: 'Pendant size', ChainSize: 'Chain size', ChainWidth: 'Chain width', ChainDiamondSize: 'Diamond size', BraceletStoneSize: 'Stone size', BraceletSize: 'Bracelet size', RingSize: 'Ring size', Color: 'Color' } as Record<string, string>)[type] ?? type; }
}
