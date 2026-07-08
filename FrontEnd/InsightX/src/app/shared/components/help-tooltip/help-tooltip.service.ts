import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class HelpTooltipService {
  readonly activeTooltipId = signal<string | null>(null);

  open(id: string): void {
    this.activeTooltipId.set(id);
  }

  close(): void {
    this.activeTooltipId.set(null);
  }

  toggle(id: string): void {
    if (this.activeTooltipId() === id) {
      this.close();
    } else {
      this.open(id);
    }
  }
}
