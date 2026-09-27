import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'ks-state-panel', standalone: true, changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<section class="state" role="status"><span class="state__mark" aria-hidden="true">{{ icon }}</span><h2>{{ title }}</h2><p>{{ message }}</p><ng-content /></section>`,
  styleUrl: './state-panel.component.scss'
})
export class StatePanelComponent {
  @Input({ required: true }) title = '';
  @Input() message = '';
  @Input() icon = '◇';
}
