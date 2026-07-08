import { Component, Input, ElementRef, ViewChild, HostListener, inject, signal, computed, effect, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HelpTooltipService } from './help-tooltip.service';

@Component({
  selector: 'app-help-tooltip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './help-tooltip.component.html',
  styleUrl: './help-tooltip.component.css'
})
export class HelpTooltipComponent implements OnDestroy {
  @Input({ required: true }) description!: string;
  @Input({ required: true }) expectedType!: string;
  @Input() example?: string;
  @Input() title?: string;

  @ViewChild('buttonRef') buttonRef!: ElementRef<HTMLButtonElement>;
  @ViewChild('popoverRef') popoverRef?: ElementRef<HTMLDivElement>;

  private readonly tooltipService = inject(HelpTooltipService);
  private readonly elementRef = inject(ElementRef);

  readonly id = 'tooltip-' + Math.random().toString(36).substring(2, 11);
  readonly isOpen = computed(() => this.tooltipService.activeTooltipId() === this.id);
  readonly isPositioned = signal(false);

  readonly popoverTop = signal(-9999);
  readonly popoverLeft = signal(-9999);
  private closeTimer?: any;

  constructor() {
    effect(() => {
      if (this.isOpen()) {
        this.isPositioned.set(false);
        this.popoverTop.set(-9999);
        this.popoverLeft.set(-9999);
        this.schedulePositionUpdate();
      } else {
        this.removePopoverFromBody();
      }
    });
  }

  private removePopoverFromBody(): void {
    if (this.popoverRef && this.popoverRef.nativeElement) {
      if (this.popoverRef.nativeElement.parentElement === document.body) {
        document.body.removeChild(this.popoverRef.nativeElement);
      }
    }
  }

  private schedulePositionUpdate(): void {
    const checkAndPosition = (attempts: number) => {
      if (!this.isOpen()) return;
      if (this.buttonRef && this.popoverRef && this.popoverRef.nativeElement) {
        if (this.popoverRef.nativeElement.parentElement !== document.body) {
          document.body.appendChild(this.popoverRef.nativeElement);
        }
        this.updatePosition();
      } else if (attempts < 15) {
        setTimeout(() => checkAndPosition(attempts + 1), 10);
      }
    };
    setTimeout(() => checkAndPosition(0), 0);
  }

  onMouseEnter(): void {
    if (this.closeTimer) {
      clearTimeout(this.closeTimer);
      this.closeTimer = undefined;
    }
    if (!this.isOpen()) {
      this.tooltipService.open(this.id);
    }
  }

  onMouseLeave(): void {
    this.scheduleClose();
  }

  onPopoverMouseEnter(): void {
    if (this.closeTimer) {
      clearTimeout(this.closeTimer);
      this.closeTimer = undefined;
    }
  }

  onPopoverMouseLeave(): void {
    this.scheduleClose();
  }

  private scheduleClose(): void {
    if (this.closeTimer) {
      clearTimeout(this.closeTimer);
    }
    this.closeTimer = setTimeout(() => {
      if (this.isOpen()) {
        this.tooltipService.close();
        this.isPositioned.set(false);
      }
    }, 150);
  }

  updatePosition(): void {
    if (!this.buttonRef || !this.popoverRef) return;
    const btnRect = this.buttonRef.nativeElement.getBoundingClientRect();
    const popWidth = this.popoverRef.nativeElement.offsetWidth;
    const popHeight = this.popoverRef.nativeElement.offsetHeight;
    const viewportWidth = window.innerWidth;
    const viewportHeight = window.innerHeight;

    // Horizontal calculation: Anchor left edge neatly over the icon (8px to the left of icon left edge)
    let left = btnRect.left - 8;
    if (left + popWidth > viewportWidth - 12) {
      left = viewportWidth - popWidth - 12;
    }
    if (left < 12) {
      left = 12;
    }

    // Vertical calculation: Predictably position directly ABOVE the icon by default
    let top = btnRect.top - popHeight - 6;

    // Only fallback to below if there is not enough space above in the viewport
    if (btnRect.top < popHeight + 10) {
      top = btnRect.bottom + 6;
    }

    this.popoverTop.set(top);
    this.popoverLeft.set(left);
    this.isPositioned.set(true);
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this.isOpen()) return;
    const target = event.target as Node;
    const clickedInsideButton = this.elementRef.nativeElement.contains(target);
    const clickedInsidePopover = this.popoverRef?.nativeElement?.contains(target);

    if (!clickedInsideButton && !clickedInsidePopover) {
      this.tooltipService.close();
      this.isPositioned.set(false);
    }
  }

  @HostListener('document:keydown.escape')
  onEscapePress(): void {
    if (this.isOpen()) {
      this.tooltipService.close();
      this.isPositioned.set(false);
    }
  }

  @HostListener('window:resize')
  onResize(): void {
    if (this.isOpen()) {
      this.updatePosition();
    }
  }

  @HostListener('document:scroll', ['$event'])
  onScroll(event: Event): void {
    if (!this.isOpen()) return;
    const target = event.target as Node;
    if (this.popoverRef && !this.popoverRef.nativeElement.contains(target)) {
      this.tooltipService.close();
      this.isPositioned.set(false);
    }
  }

  ngOnDestroy(): void {
    if (this.closeTimer) {
      clearTimeout(this.closeTimer);
    }
    this.removePopoverFromBody();
    if (this.isOpen()) {
      this.tooltipService.close();
      this.isPositioned.set(false);
    }
  }
}
