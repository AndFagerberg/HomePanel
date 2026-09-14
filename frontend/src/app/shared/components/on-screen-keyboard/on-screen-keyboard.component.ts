import { Component, input, output, signal } from '@angular/core';

export type KeyboardMode = 'text' | 'number';

@Component({
  selector: 'app-on-screen-keyboard',
  standalone: true,
  templateUrl: './on-screen-keyboard.component.html',
  styleUrl: './on-screen-keyboard.component.css',
})
export class OnScreenKeyboardComponent {
  readonly value = input('');
  readonly mode = input<KeyboardMode>('text');
  readonly maxlength = input(80);
  readonly valueChange = output<string>();
  readonly submit = output<void>();
  readonly dismissed = output<void>();

  readonly uppercase = signal(false);
  readonly letterRows = [
    ['q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'å'],
    ['a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'ö', 'ä'],
    ['z', 'x', 'c', 'v', 'b', 'n', 'm'],
  ];
  readonly numberRows = [
    ['1', '2', '3', '4', '5'],
    ['6', '7', '8', '9', '0'],
  ];

  keyLabel(key: string): string {
    return this.uppercase() ? key.toLocaleUpperCase('sv-SE') : key;
  }

  append(key: string): void {
    if (this.value().length >= this.maxlength()) {
      return;
    }

    this.valueChange.emit(this.value() + this.keyLabel(key));
  }

  addSpace(): void {
    if (this.value().length < this.maxlength()) {
      this.valueChange.emit(this.value() + ' ');
    }
  }

  backspace(): void {
    this.valueChange.emit(this.value().slice(0, -1));
  }

  clear(): void {
    this.valueChange.emit('');
  }
}
