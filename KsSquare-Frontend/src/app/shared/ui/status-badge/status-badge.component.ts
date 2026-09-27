import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'ks-status-badge', standalone: true, changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge" [class]="'badge badge--' + tone"><ng-content /></span>`,
  styleUrl: './status-badge.component.scss'
})
export class StatusBadgeComponent { @Input() tone: 'neutral' | 'gold' | 'success' | 'warning' | 'danger' = 'neutral'; }
