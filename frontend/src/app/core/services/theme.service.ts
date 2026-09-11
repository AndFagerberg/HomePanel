import { DOCUMENT } from '@angular/common';
import { Injectable, inject, signal } from '@angular/core';

export type ThemeId = 'dark' | 'daylight' | 'warm';

export interface ThemeOption {
  id: ThemeId;
  name: string;
  description: string;
}

const STORAGE_KEY = 'homepanel-theme';

const THEME_OPTIONS: readonly ThemeOption[] = [
  { id: 'dark', name: 'Mörkt', description: 'Panelens nuvarande mörka tema' },
  { id: 'daylight', name: 'Dagsljus', description: 'Ljust och klart med svala toner' },
  { id: 'warm', name: 'Varmt ljus', description: 'Ljust med mjuka, varma toner' },
];

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  readonly themes = THEME_OPTIONS;
  readonly activeTheme = signal<ThemeId>(this.readStoredTheme());

  constructor() {
    this.applyTheme(this.activeTheme());
  }

  setTheme(theme: ThemeId): void {
    this.activeTheme.set(theme);
    this.applyTheme(theme);
  }

  private readStoredTheme(): ThemeId {
    try {
      const storedTheme = this.document.defaultView?.localStorage.getItem(STORAGE_KEY);
      return THEME_OPTIONS.some((theme) => theme.id === storedTheme) ? storedTheme as ThemeId : 'dark';
    } catch {
      return 'dark';
    }
  }

  private applyTheme(theme: ThemeId): void {
    this.document.documentElement.dataset['theme'] = theme;

    try {
      this.document.defaultView?.localStorage.setItem(STORAGE_KEY, theme);
    } catch {
      // Theme selection still works when browser storage is unavailable.
    }
  }
}
