import { Pipe, PipeTransform } from '@angular/core';
import { IconName } from '../components/icon/icon.component';

// Maps backend weather symbol keys (see SmhiWeatherService.MapSymbol) to an app-icon name.
const ICONS: Record<string, IconName> = {
  clear: 'weather-clear',
  'mostly-clear': 'weather-clear',
  'partly-cloudy': 'weather-partly-cloudy',
  cloudy: 'weather-cloudy',
  overcast: 'weather-cloudy',
  fog: 'weather-fog',
  'rain-showers-light': 'weather-rain',
  'rain-showers': 'weather-rain',
  'rain-showers-heavy': 'weather-rain-heavy',
  thunder: 'weather-thunder',
  'sleet-showers-light': 'weather-sleet',
  'sleet-showers': 'weather-sleet',
  'sleet-showers-heavy': 'weather-sleet',
  'snow-showers-light': 'weather-snow',
  'snow-showers': 'weather-snow',
  'snow-showers-heavy': 'weather-snow',
  'rain-light': 'weather-rain',
  rain: 'weather-rain',
  'rain-heavy': 'weather-rain-heavy',
  'sleet-light': 'weather-sleet',
  sleet: 'weather-sleet',
  'sleet-heavy': 'weather-sleet',
  'snow-light': 'weather-snow',
  snow: 'weather-snow',
  'snow-heavy': 'weather-snow',
};

@Pipe({ name: 'weatherIcon', standalone: true })
export class WeatherIconPipe implements PipeTransform {
  transform(symbol: string): IconName {
    return ICONS[symbol] ?? 'weather-cloudy';
  }
}
