import { CommonModule } from '@angular/common';
import { Component, inject, signal, OnInit } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { ReportService } from '../../../../core/services/report.service';
import { Report } from '../../../../core/models/report.model';
import { ConfirmDialogComponent } from '../../../../shared/components/dialog/confirm-dialog';
import { ToastNotificationComponent } from '../../../../shared/components/toast-notification/toast-notification';

@Component({
  selector: 'app-report-list',
  standalone: true,
  imports: [CommonModule, RouterModule, ConfirmDialogComponent, ToastNotificationComponent],
  templateUrl: './report-list.html',
  styleUrl: './report-list.css',
})
export class ReportList implements OnInit {
  private service = inject(ReportService);
  private router = inject(Router);

  reports = signal<Report[]>([]);
  processingIds = signal<Set<number>>(new Set());
  showDeleteDialog = signal(false);
  deleteReportId = signal<number | null>(null);
  toastOpen = signal(false);
  toastMessage = signal('');
  toastType = signal<'success' | 'error' | 'info'>('success');

  ngOnInit() {
    this.loadReports();
  }

  loadReports() {
    this.service.getReports().subscribe({
      next: (res) => this.reports.set(res),
      error: (err) => {
        this.showToast('Unable to load reports.', 'error');
      },
    });
  }

  getBadgeClass(status: string): string {
    const s = status?.toLowerCase() || '';
    if (s === 'done' || s === 'processed') return 'status-done';
    if (s === 'pendingconfirmation') return 'status-processing';
    if (s === 'pending' || s === 'processing' || s === 'processingai') return 'status-processing';
    if (s === 'failed' || s === 'error' || s === 'failedai') return 'status-failed';
    return '';
  }

  processReport(id: number) {
    this.processingIds.update((set) => {
      const newSet = new Set(set);
      newSet.add(id);
      return newSet;
    });

    this.service.process(id).subscribe({
      next: () => {
        this.processingIds.update((set) => {
          const newSet = new Set(set);
          newSet.delete(id);
          return newSet;
        });
        this.loadReports();
        this.showToast('Document processing started successfully.', 'success');
      },
      error: (err) => {
        this.processingIds.update((set) => {
          const newSet = new Set(set);
          newSet.delete(id);
          return newSet;
        });
        this.showToast('Failed to process the document.', 'error');
      },
    });
  }

  reviewReport(id: number) {
    this.router.navigate(['/reports', id, 'preview']);
  }

  viewResults(id: number) {
    this.router.navigate(['/reports', id, 'results']);
  }

  delete(id: number) {
    this.deleteReportId.set(id);
    this.showDeleteDialog.set(true);
  }

  confirmDelete() {
    const id = this.deleteReportId();
    if (id === null) {
      this.cancelDelete();
      return;
    }

    this.service.delete(id).subscribe({
      next: () => {
        this.showDeleteDialog.set(false);
        this.deleteReportId.set(null);
        this.loadReports();
        this.showToast('Report deleted successfully.', 'success');
      },
      error: () => {
        this.showDeleteDialog.set(false);
        this.showToast('Failed to delete report.', 'error');
      },
    });
  }

  cancelDelete() {
    this.showDeleteDialog.set(false);
    this.deleteReportId.set(null);
  }

  private showToast(message: string, type: 'success' | 'error' | 'info' = 'success') {
    this.toastMessage.set(message);
    this.toastType.set(type);
    this.toastOpen.set(true);

    setTimeout(() => {
      this.toastOpen.set(false);
    }, 4000);
  }
}
