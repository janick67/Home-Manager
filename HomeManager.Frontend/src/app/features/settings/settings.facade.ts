import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HomeManagerApiService } from '../../core/api/home-manager-api.service';
import { HomeAssistantEntityDto, SettingsDto } from '../../core/models/api.models';

@Injectable()
export class SettingsFacade {
  private readonly api = inject(HomeManagerApiService);

  getSettings(): Observable<SettingsDto> {
    return this.api.getSettings();
  }

  saveSettings(settings: SettingsDto): Observable<void> {
    return this.api.updateSettings(settings);
  }

  getHaEntities(): Observable<HomeAssistantEntityDto[]> {
    return this.api.getHaEntities();
  }

  refreshHaEntities(): Observable<{ refreshed: number }> {
    return this.api.refreshHaEntities();
  }
}
