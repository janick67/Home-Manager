import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.page').then((m) => m.DashboardPageComponent)
  },
  {
    path: 'energy-manager',
    loadComponent: () => import('./features/energy-manager/energy-manager.page').then((m) => m.EnergyManagerPageComponent)
  },
  {
    path: 'rooms',
    loadComponent: () => import('./features/rooms/rooms.page').then((m) => m.RoomsPageComponent)
  },
  {
    path: 'schedules',
    loadComponent: () => import('./features/schedules/schedules.page').then((m) => m.SchedulesPageComponent)
  },
  {
    path: 'overrides',
    loadComponent: () => import('./features/overrides/overrides.page').then((m) => m.OverridesPageComponent)
  },
  {
    path: 'ha-entities',
    loadComponent: () => import('./features/ha-entities/ha-entities.page').then((m) => m.HaEntitiesPageComponent)
  },
  {
    path: 'logs',
    loadComponent: () => import('./features/logs/logs.page').then((m) => m.LogsPageComponent)
  },
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings.page').then((m) => m.SettingsPageComponent)
  }
];
