import {FormsModule} from '@angular/forms';
import {HttpClient} from '@angular/common/http';
import { CurrencyPipe, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { catchError, forkJoin, of } from 'rxjs';
import { AdminCatalogService } from '../../../core/catalog/admin-catalog.service';
import { Category, Product } from '../../../core/catalog/catalog.models';
import { CustomRequestService } from '../../../core/custom-requests/custom-request.service';
import { CustomJewelryRequest } from '../../../core/custom-requests/custom-request.models';
import { KsButtonDirective, StatePanelComponent, StatusBadgeComponent } from '../../../shared/ui';
@Component({ selector: 'app-admin-overview', standalone: true, imports: [FormsModule, RouterLink, CurrencyPipe, DatePipe, KsButtonDirective, StatePanelComponent, StatusBadgeComponent], templateUrl: './admin-overview.component.html', styleUrls: ['./admin-overview.component.scss', './overview-actions.component.scss'], changeDetection: ChangeDetectionStrategy.OnPush })
export class AdminOverviewComponent implements OnInit {
  private readonly http=inject(HttpClient);
  protected readonly sales=signal<{revenue:{currency:string;amount:number;paidOrders:number}[];totalOrders:number;testOrders:number;customers:number;pendingPayments:number;cancelledPayments:number;pendingReviews:number;statuses:{status:string;count:number}[];recentOrders:{id:string;orderNumber:string;customerName:string;total:number;currency:string;status:string;isDemo:boolean;createdAt:string}[]}|null>(null);
  private readonly catalog = inject(AdminCatalogService);
  private readonly customRequests = inject(CustomRequestService);
  protected readonly loading = signal(true);
  protected readonly products = signal<Product[] | null>(null);
  protected readonly categories = signal<Category[] | null>(null);
  protected readonly requests = signal<CustomJewelryRequest[] | null>(null);
  protected readonly updated = signal<Date | null>(null);
  protected readonly unavailable = computed(() => !this.loading() && (!this.sales() || !this.products() || !this.categories() || !this.requests()));
  protected readonly activeCount = computed(() => this.products()?.filter(p => p.isActive).length ?? null);
  protected readonly drafts = computed(() => this.products()?.filter(p => !p.isActive).length ?? null);
  protected readonly pending = computed(() => this.requests()?.filter(r => r.status === 'Submitted').length ?? null);
  protected readonly topCategories = computed(() => this.categories()?.filter(c => !c.parentId) ?? []);
  protected readonly distribution = computed(() => this.topCategories().map(c => ({ name: c.name, count: this.products()?.filter(p => p.categoryId === c.id).length ?? 0 })).sort((a,b) => b.count-a.count));
  protected readonly recent = computed(() => [...(this.requests() ?? [])].sort((a,b) => Date.parse(b.createdAt)-Date.parse(a.createdAt)).slice(0,4));
  protected readonly stats = computed(() => [
    { label:'Total products', value:this.products()?.length, hint:'Across your catalog', icon:'\u25c7', path:'/admin/products' },
    { label:'Active products', value:this.activeCount(), hint:'Published in your catalog', icon:'\u2713', path:'/admin/products' },
    { label:'Categories', value:this.categories() ? this.topCategories().length : null, hint:'Your collection structure', icon:'\u25a6', path:'/admin/categories' },
    { label:'New requests', value:this.pending(), hint:'Awaiting your first response', icon:'\u2197', path:'/admin/custom-requests' }
  ]);
  protected period='all';protected from='';protected to='';protected rangeLabel='All time';protected rangeError='';private rangeParams:Record<string,string>={};private sequence=0;
  protected choosePeriod(value:string){this.period=value;this.rangeError='';if(value==='custom')return;
   const now=new Date(),end=new Date(Date.UTC(now.getUTCFullYear(),now.getUTCMonth(),now.getUTCDate())),start=new Date(end);
   if(value==='week')start.setUTCDate(start.getUTCDate()-(start.getUTCDay()+6)%7);
   if(value==='month')start.setUTCDate(1);
   if(value==='year'){start.setUTCMonth(0,1);}
   this.from=start.toISOString().slice(0,10);this.to=end.toISOString().slice(0,10);
   this.rangeParams=value==='all'?{}:{from:this.from,to:this.to};this.rangeLabel=({all:'All time',today:'Today',week:'This week',month:'This month',year:'This year'} as Record<string,string>)[value];this.load();
  }
  protected applyRange(){if(!this.from||!this.to||this.from>this.to){this.rangeError='Choose both dates, with the end on or after the start.';return;}this.rangeError='';this.rangeParams={from:this.from,to:this.to};this.rangeLabel=this.from+' to '+this.to;this.load();}
  ngOnInit(): void { this.load(); }
  protected load(): void {
    const sequence=++this.sequence;this.loading.set(true);
    forkJoin({
      sales:this.http.get<NonNullable<ReturnType<typeof this.sales>>>('/api/admin/overview',{params:this.rangeParams}).pipe(catchError(()=>of(null))),
      products: this.catalog.getProducts().pipe(catchError(() => of(null))),
      categories: this.catalog.getCategories().pipe(catchError(() => of(null))),
      requests: this.customRequests.getAdminAll().pipe(catchError(() => of(null)))
    }).subscribe(({sales,products,categories,requests}) => { if(sequence!==this.sequence)return;this.sales.set(sales); this.products.set(products); this.categories.set(categories); this.requests.set(requests); this.updated.set(new Date()); this.loading.set(false); });
  }
}
