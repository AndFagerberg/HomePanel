import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// Fixed set of hand-drawn line icons. Avoids relying on emoji glyphs/fonts that
// render inconsistently (missing Å/Ä/Ö-adjacent glyph coverage, color-emoji fallback, etc).
export type IconName =
  | 'menu'
  | 'close'
  | 'back'
  | 'home'
  | 'cabin'
  | 'bus'
  | 'calendar'
  | 'timer'
  | 'music'
  | 'news'
  | 'settings'
  | 'stop'
  | 'droplet'
  | 'wind'
  | 'weather-clear'
  | 'weather-partly-cloudy'
  | 'weather-cloudy'
  | 'weather-fog'
  | 'weather-rain'
  | 'weather-rain-heavy'
  | 'weather-thunder'
  | 'weather-snow'
  | 'weather-sleet';

@Component({
  selector: 'app-icon',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <svg
      class="icon"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="1.8"
      stroke-linecap="round"
      stroke-linejoin="round"
      aria-hidden="true"
    >
      @switch (name()) {
        @case ('menu') {
          <line x1="3" y1="6" x2="21" y2="6" />
          <line x1="3" y1="12" x2="21" y2="12" />
          <line x1="3" y1="18" x2="21" y2="18" />
        }
        @case ('close') {
          <line x1="5" y1="5" x2="19" y2="19" />
          <line x1="19" y1="5" x2="5" y2="19" />
        }
        @case ('back') {
          <polyline points="14 4 6 12 14 20" />
        }
        @case ('home') {
          <path d="M4 11 12 4l8 7" />
          <path d="M6 10v9h12v-9" />
        }
        @case ('cabin') {
          <path d="M4 11 12 4l8 7" />
          <path d="M6 10v9h12v-9" />
          <line x1="10" y1="19" x2="10" y2="14" />
          <line x1="14" y1="19" x2="14" y2="14" />
        }
        @case ('bus') {
          <rect x="3.5" y="5" width="17" height="12" rx="2" />
          <line x1="3.5" y1="12" x2="20.5" y2="12" />
          <circle cx="7.5" cy="19.5" r="1.4" />
          <circle cx="16.5" cy="19.5" r="1.4" />
        }
        @case ('calendar') {
          <rect x="3.5" y="5" width="17" height="15" rx="2" />
          <line x1="3.5" y1="9.5" x2="20.5" y2="9.5" />
          <line x1="7.5" y1="3" x2="7.5" y2="7" />
          <line x1="16.5" y1="3" x2="16.5" y2="7" />
        }
        @case ('timer') {
          <circle cx="12" cy="13" r="8" />
          <line x1="12" y1="13" x2="12" y2="8.5" />
          <line x1="12" y1="13" x2="15" y2="15" />
          <line x1="9.5" y1="2" x2="14.5" y2="2" />
        }
        @case ('music') {
          <path d="M9 17V4.5l10-2V15" />
          <circle cx="6.5" cy="17.5" r="2.5" />
          <circle cx="16.5" cy="15" r="2.5" />
        }
        @case ('news') {
          <rect x="3.5" y="4.5" width="14" height="15" rx="1.5" />
          <path d="M17.5 8H20a.5.5 0 0 1 .5.5v8a2.5 2.5 0 0 1-2.5 2.5" />
          <line x1="6.5" y1="8" x2="14" y2="8" />
          <line x1="6.5" y1="11.5" x2="14" y2="11.5" />
          <line x1="6.5" y1="15" x2="11" y2="15" />
        }
        @case ('settings') {
          <circle cx="12" cy="12" r="3" />
          <path d="M19.4 13.5a1.7 1.7 0 0 0 .34 1.87l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.7 1.7 0 0 0-1.87-.34 1.7 1.7 0 0 0-1.04 1.56V19.6a2 2 0 1 1-4 0v-.09a1.7 1.7 0 0 0-1.04-1.56 1.7 1.7 0 0 0-1.87.34l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.7 1.7 0 0 0 .34-1.87 1.7 1.7 0 0 0-1.56-1.04H2.4a2 2 0 1 1 0-4h.09a1.7 1.7 0 0 0 1.56-1.04 1.7 1.7 0 0 0-.34-1.87l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.7 1.7 0 0 0 1.87.34H8.5a1.7 1.7 0 0 0 1.04-1.56V2.4a2 2 0 1 1 4 0v.09a1.7 1.7 0 0 0 1.04 1.56 1.7 1.7 0 0 0 1.87-.34l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.7 1.7 0 0 0-.34 1.87V8.5a1.7 1.7 0 0 0 1.56 1.04h.09a2 2 0 1 1 0 4h-.09a1.7 1.7 0 0 0-1.56 1.04Z" />
        }
        @case ('stop') {
          <rect x="6" y="6" width="12" height="12" rx="1.5" fill="currentColor" stroke="none" />
        }
        @case ('droplet') {
          <path d="M12 3.5s6 6.7 6 11a6 6 0 0 1-12 0c0-4.3 6-11 6-11Z" />
        }
        @case ('wind') {
          <path d="M3 8h11a2.5 2.5 0 1 0-2.4-3.2" />
          <path d="M3 13h14a2.5 2.5 0 1 1-2.4 3.2" />
          <path d="M3 17.5h8a2 2 0 1 1-1.9 2.6" />
        }
        @case ('weather-clear') {
          <circle cx="12" cy="12" r="4.5" />
          <line x1="12" y1="2.5" x2="12" y2="5" />
          <line x1="12" y1="19" x2="12" y2="21.5" />
          <line x1="2.5" y1="12" x2="5" y2="12" />
          <line x1="19" y1="12" x2="21.5" y2="12" />
          <line x1="5.1" y1="5.1" x2="6.8" y2="6.8" />
          <line x1="17.2" y1="17.2" x2="18.9" y2="18.9" />
          <line x1="5.1" y1="18.9" x2="6.8" y2="17.2" />
          <line x1="17.2" y1="6.8" x2="18.9" y2="5.1" />
        }
        @case ('weather-partly-cloudy') {
          <circle cx="8" cy="8.5" r="3.5" />
          <path d="M7 17.5h9.5a3.5 3.5 0 0 0 .5-6.96A5 5 0 0 0 8 12" />
        }
        @case ('weather-cloudy') {
          <path d="M6.5 18h11a3.7 3.7 0 0 0 .5-7.36A5.5 5.5 0 0 0 7.6 9.9 4 4 0 0 0 6.5 18Z" />
        }
        @case ('weather-fog') {
          <path d="M6.5 14h11a3.7 3.7 0 0 0 .5-7.36A5.5 5.5 0 0 0 7.6 5.9 4 4 0 0 0 6.5 14Z" />
          <line x1="4" y1="18" x2="20" y2="18" />
          <line x1="6" y1="21" x2="18" y2="21" />
        }
        @case ('weather-rain') {
          <path d="M6.5 12.5h11a3.7 3.7 0 0 0 .5-7.36A5.5 5.5 0 0 0 7.6 4.4 4 4 0 0 0 6.5 12.5Z" />
          <line x1="8.5" y1="16" x2="7.5" y2="19.5" />
          <line x1="12.5" y1="16" x2="11.5" y2="19.5" />
          <line x1="16.5" y1="16" x2="15.5" y2="19.5" />
        }
        @case ('weather-rain-heavy') {
          <path d="M6.5 11.5h11a3.7 3.7 0 0 0 .5-7.36A5.5 5.5 0 0 0 7.6 3.4 4 4 0 0 0 6.5 11.5Z" />
          <line x1="7.5" y1="15" x2="6.2" y2="19.5" />
          <line x1="11.5" y1="15" x2="10.2" y2="19.5" />
          <line x1="15.5" y1="15" x2="14.2" y2="19.5" />
          <line x1="19" y1="15" x2="17.7" y2="19.5" />
        }
        @case ('weather-thunder') {
          <path d="M6.5 11.5h11a3.7 3.7 0 0 0 .5-7.36A5.5 5.5 0 0 0 7.6 3.4 4 4 0 0 0 6.5 11.5Z" />
          <polyline points="13 15 10 19.5 13 19.5 11 22.5" />
        }
        @case ('weather-snow') {
          <path d="M6.5 12.5h11a3.7 3.7 0 0 0 .5-7.36A5.5 5.5 0 0 0 7.6 4.4 4 4 0 0 0 6.5 12.5Z" />
          <line x1="8" y1="16" x2="8" y2="20" />
          <line x1="6.2" y1="17" x2="9.8" y2="19" />
          <line x1="9.8" y1="17" x2="6.2" y2="19" />
          <line x1="16" y1="16" x2="16" y2="20" />
          <line x1="14.2" y1="17" x2="17.8" y2="19" />
          <line x1="17.8" y1="17" x2="14.2" y2="19" />
        }
        @case ('weather-sleet') {
          <path d="M6.5 11.5h11a3.7 3.7 0 0 0 .5-7.36A5.5 5.5 0 0 0 7.6 3.4 4 4 0 0 0 6.5 11.5Z" />
          <line x1="8.5" y1="15" x2="7.5" y2="18.5" />
          <line x1="15" y1="16" x2="15" y2="20" />
          <line x1="13.4" y1="17" x2="16.6" y2="19" />
          <line x1="16.6" y1="17" x2="13.4" y2="19" />
        }
      }
    </svg>
  `,
  styles: `
    :host {
      display: inline-flex;
      line-height: 0;
    }

    .icon {
      width: 1em;
      height: 1em;
    }
  `,
})
export class IconComponent {
  readonly name = input.required<IconName>();
}
