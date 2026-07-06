import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: string;
  message: string;
  type: 'success' | 'error' | 'info';
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  readonly toasts = signal<Toast[]>([]);

  show(message: string, type: 'success' | 'error' | 'info' = 'success', duration = 3000): void {
    const id = Math.random().toString(36).substring(2, 9);
    const newToast: Toast = { id, message, type };
    this.toasts.update(current => [...current, newToast]);

    setTimeout(() => {
      this.remove(id);
    }, duration);
  }

  remove(id: string): void {
    this.toasts.update(current => current.filter(t => t.id !== id));
  }
}
