import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, of, switchMap } from 'rxjs';
import { AdminCatalogService } from '../../../core/catalog/admin-catalog.service';
import { Category } from '../../../core/catalog/catalog.models';
import { KsButtonDirective, StatePanelComponent, StatusBadgeComponent } from '../../../shared/ui';

@Component({ selector: 'app-category-admin', standalone: true, imports: [DialogFocusDirective, ReactiveFormsModule, KsButtonDirective, StatePanelComponent, StatusBadgeComponent], changeDetection: ChangeDetectionStrategy.OnPush, templateUrl: './category-admin.component.html', styleUrl: './category-admin.component.scss' })
export class CategoryAdminComponent implements OnInit {
  private readonly catalog = inject(AdminCatalogService);
  protected readonly categories = signal<Category[]>([]); protected readonly selectedCategoryId = signal<string | null>(null);
  protected readonly loading = signal(true); protected readonly saving = signal(false); protected readonly error = signal(''); protected readonly dialogOpen = signal(false);
  protected readonly editingId = signal<string | null>(null); protected readonly mode = signal<'category' | 'subcategory'>('category'); protected readonly selectedImage = signal<File | null>(null);
  protected readonly topCategories = computed(() => this.categories().filter(item => !item.parentId));
  protected readonly selectedCategory = computed(() => this.topCategories().find(item => item.id === this.selectedCategoryId()) ?? null);
  protected readonly visibleSubcategories = computed(() => this.categories().filter(item => item.parentId === this.selectedCategoryId()));
  protected readonly editingCategory = computed(() => this.categories().find(item => item.id === this.editingId()) ?? null);
  protected readonly form = new FormGroup({ name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(120)] }), parentId: new FormControl<string | null>(null), isActive: new FormControl(true, { nonNullable: true }), showInCustom: new FormControl(false, { nonNullable: true }) });

  ngOnInit(): void { this.load(); }
  protected load(selectId?: string): void { this.loading.set(true); this.error.set(''); this.catalog.getCategories().pipe(finalize(() => this.loading.set(false))).subscribe({ next: value => { this.categories.set(value); const valid = value.some(item => !item.parentId && item.id === (selectId ?? this.selectedCategoryId())); this.selectedCategoryId.set(valid ? (selectId ?? this.selectedCategoryId())! : value.find(item => !item.parentId)?.id ?? null); }, error: error => this.error.set(this.message(error)) }); }
  protected selectCategory(category: Category): void { this.selectedCategoryId.set(category.id); }
  protected create(mode: 'category' | 'subcategory'): void { this.editingId.set(null); this.mode.set(mode); this.selectedImage.set(null); this.form.reset({ name: '', parentId: mode === 'subcategory' ? this.selectedCategoryId() : null, isActive: true, showInCustom: false }); this.dialogOpen.set(true); }
  protected edit(category: Category): void { this.editingId.set(category.id); this.mode.set(category.parentId ? 'subcategory' : 'category'); this.selectedImage.set(null); this.form.reset({ name: category.name, parentId: category.parentId, isActive: category.isActive, showInCustom: category.showInCustom }); this.dialogOpen.set(true); }
  protected closeDialog(): void { if (this.saving()) return; this.dialogOpen.set(false); this.editingId.set(null); this.selectedImage.set(null); }
  protected chooseImage(event: Event): void { const file = (event.target as HTMLInputElement).files?.[0] ?? null; this.error.set(''); if (file && (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 10 * 1024 * 1024)) { this.error.set('Choose a JPEG, PNG or WebP image up to 10 MB.'); this.selectedImage.set(null); return; } this.selectedImage.set(file); }
  protected save(): void {
    if (this.form.invalid || (this.mode() === 'subcategory' && !this.form.controls.parentId.value)) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.error.set(''); const value = this.form.getRawValue(); const request = { ...value, parentId: this.mode() === 'category' ? null : value.parentId, showInCustom: this.mode() === 'category' && value.showInCustom };
    const operation = this.editingId() ? this.catalog.updateCategory(this.editingId()!, request) : this.catalog.createCategory(request);
    operation.pipe(switchMap(category => this.selectedImage() ? this.catalog.uploadCategoryImage(category.id, this.selectedImage()!) : of(category)), finalize(() => this.saving.set(false))).subscribe({ next: category => { const parentSelection = category.parentId ?? category.id; this.dialogOpen.set(false); this.editingId.set(null); this.selectedImage.set(null); this.load(parentSelection); }, error: error => this.error.set(this.message(error)) });
  }
  protected removeImage(category: Category): void { if (!confirm('Remove this category image?')) return; this.catalog.deleteCategoryImage(category.id).subscribe({ next: () => this.load(category.id), error: error => this.error.set(this.message(error)) }); }
  protected remove(category: Category): void { if (!confirm(`Delete "${category.name}"?`)) return; this.catalog.deleteCategory(category.id).subscribe({ next: () => this.load(), error: error => this.error.set(this.message(error)) }); }
  private message(error: unknown): string { return error instanceof HttpErrorResponse ? error.error?.error ?? 'The request could not be completed.' : 'The request could not be completed.'; }
}
