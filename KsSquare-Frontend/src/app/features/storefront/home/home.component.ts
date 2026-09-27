import { SectionRevealDirective } from './section-reveal.directive';
import { CollectionHeroComponent } from './collection-hero.component';
﻿import { EtsyReviewsComponent } from '../../../shared/ui/etsy-reviews.component';
import { ChangeDetectionStrategy, Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Product } from '../../../core/catalog/catalog.models';
import { StorefrontCatalogService } from '../../../core/catalog/storefront-catalog.service';
import { BrandLogoComponent, KsButtonDirective, ProductCardComponent } from '../../../shared/ui';
@Component({ selector: 'app-home', standalone: true, imports: [SectionRevealDirective, CollectionHeroComponent, EtsyReviewsComponent, RouterLink, BrandLogoComponent, KsButtonDirective, ProductCardComponent], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './home.component.html', styleUrls: ['./home.component.scss', './home-story.component.scss', './home-hero.component.scss', './home-refresh.component.scss', './home-pendant.component.scss', './home-editorial.component.scss'] })
export class HomeComponent implements OnInit {
  private readonly catalog = inject(StorefrontCatalogService);
  protected readonly products = signal<Product[]>([]);
  protected readonly hasMore = signal(false);
  protected readonly loadingMore = signal(false);
  protected readonly search = signal('');
  protected readonly moreError = signal(false);
  private page = 1;
  protected searchProducts(value:string): void {if(this.loadingMore())return;this.search.set(value.trim());this.loadProducts();}
  protected readonly loading = signal(true);
  protected readonly error = signal(false);
  protected readonly collections = [
    {name:'Pendants',path:'/pendants',copy:'Make it personal.',image:'custom-pendant'},
    {name:'Chains',path:'/chains',copy:'Bold in every link.',image:'cuban-chain'},
    {name:'Watch designs',path:'/watches',copy:'The details make it.',image:'watch-detail'},
    {name:'Bracelets',path:'/bracelets',copy:'Your everyday shine.',image:'tennis-bracelet'},
    {name:'Rings',path:'/rings',copy:'Made to mean more.',image:'moissanite-ring'},
    {name:'Earrings',path:'/earrings',copy:'A brilliant finishing touch.',image:'stud-earrings'}
  ];
  ngOnInit(): void { this.loadProducts(); }
  protected loadProducts(): void {
    this.page = 1; this.loading.set(true); this.error.set(false);
    this.catalog.getProducts({ search: this.search(), categoryIds: [], subcategoryIds: [], optionIds: [], availability: ['available'], minimumPrice: null, maximumPrice: null, minimumDiscount: null, sort: 'newest', page: 1, pageSize: 8 })
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({ next: page => {this.products.set(page.items);this.hasMore.set(page.totalCount > page.items.length);}, error: () => this.error.set(true) });
  }
  protected loadMore(): void {
    if(this.loadingMore()) return;
    this.moreError.set(false);this.loadingMore.set(true);
    this.catalog.getProducts({search:this.search(),categoryIds:[],subcategoryIds:[],optionIds:[],availability:['available'],minimumPrice:null,maximumPrice:null,minimumDiscount:null,sort:'newest',page:this.page+1,pageSize:8}).pipe(finalize(()=>this.loadingMore.set(false))).subscribe({next:result=>{this.page++;this.products.update(items=>[...items,...result.items]);this.hasMore.set(this.products().length<result.totalCount);},error:()=>this.moreError.set(true)});
  }
}
