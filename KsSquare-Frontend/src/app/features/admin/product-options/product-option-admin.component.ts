import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, of, switchMap, tap } from 'rxjs';
import { AdminCatalogService } from '../../../core/catalog/admin-catalog.service';
import { ProductOption, ProductOptionType } from '../../../core/catalog/catalog.models';
import { KsButtonDirective, StatePanelComponent, StatusBadgeComponent } from '../../../shared/ui';

@Component({ selector: 'app-product-option-admin', standalone: true, imports: [DialogFocusDirective, ReactiveFormsModule, KsButtonDirective, StatePanelComponent, StatusBadgeComponent], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './product-option-admin.component.html', styleUrl: './product-option-admin.component.scss' })
export class ProductOptionAdminComponent implements OnInit {
  private readonly catalog = inject(AdminCatalogService);
  protected readonly options = signal<ProductOption[]>([]); protected readonly loading = signal(true); protected readonly saving = signal(false); protected readonly error = signal(''); protected readonly dialogError = signal(''); protected readonly editingId = signal<string | null>(null); protected readonly selectedPhoto = signal<File | null>(null); protected readonly dialogOpen = signal(false);
  protected readonly selectedGuide = signal<File | null>(null);
  protected readonly types: { value: ProductOptionType; label: string; hint: string }[] = [{ value: 'CustomPendantStyle', label: 'Custom pendant styles', hint: 'Selection photos and size guides for pendants and pendant + chain sets' }, { value: 'RingSize', label: 'Ring sizes', hint: 'Used only for Ring products' }, { value: 'PendantSize', label: 'Pendant sizes', hint: 'Used only for Pendant products' }, { value: 'ChainWidth', label: 'Chain widths', hint: 'Used only for Chain products' }, { value: 'ChainSize', label: 'Chain sizes', hint: 'Used only for Chain products' }, { value: 'ChainDiamondSize', label: 'Chain diamond sizes', hint: 'Used only for Chain products' }, { value: 'BraceletSize', label: 'Bracelet sizes', hint: 'Used only for Bracelet products' }, { value: 'BraceletStoneSize', label: 'Bracelet stone sizes', hint: 'Used only for Bracelet products' }, { value: 'Color', label: 'Colors', hint: 'Used for Ring, Pendant and Bracelet products' }];
  protected readonly groups = computed(() => this.types.map(type => ({ ...type, options: this.options().filter(option => option.type === type.value) })));
  protected readonly editingOption = computed(() => this.options().find(option => option.id === this.editingId()) ?? null);
  protected readonly form = new FormGroup({ type: new FormControl<ProductOptionType>('PendantSize', { nonNullable: true, validators: [Validators.required] }), name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(100)] }), displayOrder: new FormControl(0, { nonNullable: true, validators: [Validators.min(0)] }), isActive: new FormControl(true, { nonNullable: true }) });
  ngOnInit(): void { this.load(); }
  protected load(): void { this.loading.set(true); this.error.set(''); this.catalog.getProductOptions().pipe(finalize(() => this.loading.set(false))).subscribe({ next: options => this.options.set(options), error: error => this.error.set(this.message(error)) }); }
  protected add(type: ProductOptionType): void { if (this.saving()) return; this.dialogError.set(''); this.editingId.set(null); this.selectedPhoto.set(null); this.selectedGuide.set(null); this.form.reset({ type, name: '', displayOrder: this.options().filter(option => option.type === type).length, isActive: true }); this.dialogOpen.set(true); }
  protected edit(option: ProductOption): void { if (this.saving()) return; this.dialogError.set(''); this.editingId.set(option.id); this.selectedPhoto.set(null); this.selectedGuide.set(null); this.form.reset({ type: option.type, name: option.name, displayOrder: option.displayOrder, isActive: option.isActive }); this.dialogOpen.set(true); }
  protected cancel(): void { this.dialogError.set(''); this.dialogOpen.set(false); this.editingId.set(null); this.selectedPhoto.set(null); this.selectedGuide.set(null); this.form.reset({ type: 'PendantSize', name: '', displayOrder: 0, isActive: true }); }
  protected choosePhoto(event: Event, guide = false): void { const file = (event.target as HTMLInputElement).files?.[0] ?? null; this.dialogError.set(''); if (file && (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 10 * 1024 * 1024)) { this.dialogError.set('Choose a JPEG, PNG or WebP photo up to 10 MB.'); (guide ? this.selectedGuide : this.selectedPhoto).set(null); return; } (guide ? this.selectedGuide : this.selectedPhoto).set(file); }
  protected save(): void {
    if(this.saving()) return;
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.dialogError.set('');
    const photo = this.selectedPhoto(); const guide = this.selectedGuide();
    const operation = this.editingId() ? this.catalog.updateProductOption(this.editingId()!, this.form.getRawValue()) : this.catalog.createProductOption(this.form.getRawValue());
    operation.pipe(tap(option => this.editingId.set(option.id)), switchMap(option => ['Color','CustomPendantStyle'].includes(option.type) && photo ? this.catalog.uploadProductOptionImage(option.id, photo) : of(option)), switchMap(option => option.type === 'CustomPendantStyle' && guide ? this.catalog.uploadProductOptionSizeGuide(option.id, guide) : of(option)), finalize(() => this.saving.set(false))).subscribe({ next: () => { this.cancel(); this.load(); }, error: error => this.dialogError.set(this.message(error)) });
  }
  protected removePhoto(option: ProductOption, guide = false): void {
    if(this.saving() || !confirm('Remove this photo?')) return;
    this.saving.set(true); this.dialogError.set('');
    (guide ? this.catalog.deleteProductOptionSizeGuide(option.id) : this.catalog.deleteProductOptionImage(option.id)).pipe(finalize(()=>this.saving.set(false))).subscribe({ next: () => this.load(), error: error => this.dialogError.set(this.message(error)) });
  }
  protected remove(option: ProductOption): void {
    if(this.saving() || !confirm('Delete "'+option.name+'"?')) return;
    this.saving.set(true); this.dialogError.set('');
    this.catalog.deleteProductOption(option.id).pipe(finalize(()=>this.saving.set(false))).subscribe({ next: () => { this.cancel(); this.load(); }, error: error => this.dialogError.set(this.message(error)) });
  }
  protected typeLabel(type: ProductOptionType): string { return this.types.find(item => item.value === type)?.label ?? 'Option'; }
  private message(error: unknown): string { return error instanceof HttpErrorResponse ? error.error?.error ?? 'The request could not be completed.' : 'The request could not be completed.'; }
}
