import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { SettingsDto } from '../../core/models/api.models';

@Injectable()
export class SettingsFacade {
  private readonly api = inject(HomeManagerApiService);

  getSettings(): Observable<SettingsDto> {
    return this.api.getSettings();
  }

  saveSettings(settings: SettingsDto): Observable<void> {
    return this.api.updateSettings(settings);
  }
}
