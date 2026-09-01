import { Pipe, PipeTransform } from '@angular/core';

// Maps backend weather symbol keys (see SmhiWeatherService.MapSymbol) to a display emoji.
const ICONS: Record<string, string> = {
  clear: '☀️',
  'mostly-clear': '🌤️',
  'partly-cloudy': '⛅',
  cloudy: '☁️',
  overcast: '☁️',
  fog: '🌫️',
  'rain-showers-light': '🌦️',
  'rain-showers': '🌦️',
  'rain-showers-heavy': '🌧️',
  thunder: '⛈️',
  'sleet-showers-light': '🌨️',
  'sleet-showers': '🌨️',
  'sleet-showers-heavy': '🌨️',
  'snow-showers-light': '🌨️',
  'snow-showers': '🌨️',
  'snow-showers-heavy': '❄️',
  'rain-light': '🌦️',
  rain: '🌧️',
  'rain-heavy': '🌧️',
  'sleet-light': '🌨️',
  sleet: '🌨️',
  'sleet-heavy': '🌨️',
  'snow-light': '🌨️',
  snow: '❄️',
  'snow-heavy': '❄️',
};

@Pipe({ name: 'weatherIcon', standalone: true })
export class WeatherIconPipe implements PipeTransform {
  transform(symbol: string): string {
    return ICONS[symbol] ?? '🌡️';
  }
}
