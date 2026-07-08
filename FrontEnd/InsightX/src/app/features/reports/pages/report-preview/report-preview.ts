import { ChangeDetectorRef, Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReportService } from '../../../../core/services/report.service';
import { ToastNotificationComponent } from '../../../../shared/components/toast-notification/toast-notification';
import { AlertsApiService } from '../../../alerts/services/alerts-api.service';
import { ConfirmDialogComponent } from '../../../../shared/components/dialog/confirm-dialog';

@Component({
  selector: 'app-report-preview',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, ToastNotificationComponent, ConfirmDialogComponent],
  templateUrl: './report-preview.html',
  styleUrls: ['./report-preview.css'],
})
export class ReportPreview implements OnInit {
  private service = inject(ReportService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);
  private alertsApi = inject(AlertsApiService);

  reportId!: number;
  form!: FormGroup;
  isLoading = true;
  isSaving = false;
  toastOpen = signal(false);
  toastMessage = signal('');
  toastType = signal<'success' | 'error' | 'info'>('success');
  isDeleteDialogOpen = signal(false);
  rawText = signal<string>('');

  metrics: any[] = [];

  ngOnInit() {
    this.reportId = Number(this.route.snapshot.paramMap.get('id'));

    // Initialize form empty, we will dynamically add controls based on KPIs
    this.form = this.fb.group({});

    if (this.reportId) {
      this.fetchPreview();
    }
  }

  fetchPreview() {
    this.service.getText(this.reportId).subscribe({
      next: (res: any) => this.rawText.set(res.text || 'No text extracted.'),
      error: () => this.rawText.set('Could not load original text.')
    });

    this.service.getPreview(this.reportId).subscribe({
      next: (res: any[]) => {
        this.metrics = res;
        this.metrics.forEach(metric => {
          this.form.addControl(metric.kpiName, this.fb.control(metric.value, Validators.required));
        });
        this.isLoading = false;
        this.cdr.detectChanges(); // Force update the view
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges(); // Force update the view
        this.showToast('Could not load KPIs for review.', 'error');
      },
    });
  }

  confirmAndSave() {
    if (!this.form || this.form.invalid) return;

    this.isSaving = true;
    
    // Map form values back into metric array
    const formValues = this.form.value;
    const updatedMetrics = this.metrics.map(m => ({
      ...m,
      value: formValues[m.kpiName]
    }));

    const finalData = { metrics: updatedMetrics };

    this.service.confirm(this.reportId, finalData).subscribe({
      next: () => {
        this.alertsApi.fetchUnseenCount();
        this.isSaving = false;
        this.router.navigate(['/reports']);
      },
      error: () => {
        this.isSaving = false;
        this.showToast('Failed to save data. Please try again.', 'error');
      },
    });
  }

  acceptAsIs() {
    this.isSaving = true;
    this.service.confirm(this.reportId, { metrics: [] }).subscribe({
      next: () => {
        this.alertsApi.fetchUnseenCount();
        this.isSaving = false;
        this.router.navigate(['/reports']);
      },
      error: () => {
        this.isSaving = false;
        this.showToast('Failed to accept report. Please try again.', 'error');
      },
    });
  }

  openDeleteDialog() {
    this.isDeleteDialogOpen.set(true);
  }

  cancelDelete() {
    this.isDeleteDialogOpen.set(false);
  }

  confirmDelete() {
    this.isDeleteDialogOpen.set(false);
    this.deleteReport();
  }

  deleteReport() {
    this.isSaving = true;
    this.service.delete(this.reportId).subscribe({
      next: () => {
        this.alertsApi.fetchUnseenCount();
        this.isSaving = false;
        this.router.navigate(['/reports']);
      },
      error: () => {
        this.isSaving = false;
        this.showToast('Failed to remove document. Please try again.', 'error');
      },
    });
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
