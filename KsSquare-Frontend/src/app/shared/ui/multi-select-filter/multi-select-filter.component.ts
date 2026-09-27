import { ChangeDetectionStrategy, Component, ElementRef, EventEmitter, HostListener, Input, Output, inject } from '@angular/core';

export interface MultiSelectFilterOption { id: string; name: string; }

@Component({
  selector: 'ks-multi-select-filter',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="filter-label">{{ label }}</span>
    <button type="button" class="trigger" [attr.aria-expanded]="open" (click)="open = !open">
      <span>{{ summary }}</span><svg viewBox="0 0 20 20" aria-hidden="true"><path d="m5 7 5 5 5-5" /></svg>
    </button>
    @if (open) { <div class="menu">
      <div class="menu-head"><span>{{ selected.length }} selected</span>@if (selected.length) { <button type="button" (click)="clear()">Clear</button> }</div>
      <div class="choices">@for (option of options; track option.id) { <label><input type="checkbox" [checked]="isSelected(option.id)" (change)="toggle(option.id, $any($event.target).checked)" /><span>{{ option.name }}</span></label> } @empty { <p>No options available</p> }</div>
    </div> }
  `,
  styleUrl: './multi-select-filter.component.scss'
})
export class MultiSelectFilterComponent {
  private readonly element = inject(ElementRef<HTMLElement>);
  @Input({ required: true }) label = '';
  @Input() placeholder = 'All';
  @Input() options: MultiSelectFilterOption[] = [];
  @Input() selected: string[] = [];
  @Output() readonly selectedChange = new EventEmitter<string[]>();
  protected open = false;
  protected get summary(): string { if (!this.selected.length) return this.placeholder; if (this.selected.length === 1) return this.options.find(option => option.id === this.selected[0])?.name ?? '1 selected'; return `${this.selected.length} selected`; }
  protected isSelected(id: string): boolean { return this.selected.includes(id); }
  protected toggle(id: string, checked: boolean): void { this.selectedChange.emit(checked ? [...this.selected, id] : this.selected.filter(value => value !== id)); }
  protected clear(): void { this.selectedChange.emit([]); }
  @HostListener('document:click', ['$event']) protected closeOutside(event: Event): void { if (!this.element.nativeElement.contains(event.target as Node)) this.open = false; }
}
