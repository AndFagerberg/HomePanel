import { Component, inject } from '@angular/core';
import { ThemeId, ThemeService } from '../../core/services/theme.service';

@Component({
  selector: 'app-settings-view',
  standalone: true,
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css',
})
export class SettingsComponent {
  private readonly themeService = inject(ThemeService);

  readonly themes = this.themeService.themes;
  readonly activeTheme = this.themeService.activeTheme;

  selectTheme(theme: ThemeId): void {
    this.themeService.setTheme(theme);
  }
}
