import { ChangeDetectionStrategy, Component } from '@angular/core';
import { NgFor } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [NgFor, RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppComponent {
  readonly tabs = [
    { path: '/dashboard', label: 'Dashboard' },
    { path: '/energy-manager', label: 'Energy Manager' },
    { path: '/rooms', label: 'Rooms' },
    { path: '/schedules', label: 'Schedules' },
    { path: '/overrides', label: 'Overrides' },
    { path: '/ha-entities', label: 'HA Entities' },
    { path: '/logs', label: 'Logs' },
    { path: '/settings', label: 'Settings' }
  ];
}