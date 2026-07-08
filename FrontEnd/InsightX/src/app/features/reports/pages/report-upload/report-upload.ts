import { Component, inject, signal, computed, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService } from '../../../../core/services/report.service';
import { AuthService } from '../../../../core/services/auth.service';
import { DepartmentService, DepartmentResponse } from '../../../../core/services/department.service';
import { HttpEventType } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { ToastNotificationComponent } from '../../../../shared/components/toast-notification/toast-notification';

interface MonthOption {
  value: number;
  label: string;
  disabled: boolean;
}

@Component({
  selector: 'app-report-upload',
  standalone: true,
  imports: [CommonModule, RouterModule, ToastNotificationComponent],
  templateUrl: './report-upload.html',
  styleUrl: './report-upload.css',
})
export class ReportUpload implements OnInit, OnDestroy {
  private service = inject(ReportService);
  private authService = inject(AuthService);
  private deptService = inject(DepartmentService);
  private router = inject(Router);

  selectedFile = signal<File | undefined>(undefined);
  reportName = signal<string>('');
  selectedDepartmentId = signal<number | null>(null);
  departments = signal<DepartmentResponse[]>([]);
  isOwner = computed(() => this.authService.currentUser()?.role === 'Owner');
  isManager = computed(() => this.authService.currentUser()?.role === 'Manager');

  // Month/Year selection - Year must be chosen first
  selectedMonth = signal<number | null>(null);
  selectedYear = signal<number | null>(null);
  uploadedMonths = signal<number[]>([]);

  // Department must be selected before year/month dropdowns are usable (for owners)
  departmentReady = computed(() => {
    if (this.isOwner()) {
      return this.selectedDepartmentId() !== null;
    }
    // Managers always have a department from their token
    return true;
  });

  canSelectMonth = computed(() => {
    return this.departmentReady() && this.selectedYear() !== null;
  });

  monthPlaceholder = computed(() => {
    if (!this.departmentReady()) {
      return 'Select a department first...';
    }
    if (this.selectedYear() === null) {
      return 'Select a year first...';
    }
    return 'Select month...';
  });

  private readonly monthNames = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December'
  ];

  availableYears = computed(() => {
    const current = new Date().getFullYear();
    return [current - 1, current, current + 1];
  });

  monthOptions = computed<MonthOption[]>(() => {
    const uploaded = this.uploadedMonths();
    return this.monthNames.map((label, i) => ({
      value: i + 1,
      label,
      disabled: uploaded.includes(i + 1)
    }));
  });

  isDragging = signal<boolean>(false);
  isUploading = signal<boolean>(false);
  isProcessing = signal<boolean>(false);
  uploadProgress = signal<number>(0);
  toastOpen = signal(false);
  toastMessage = signal('');
  toastType = signal<'success' | 'error' | 'info'>('success');
  private processingInterval: any;

  ngOnInit() {
    if (this.isOwner()) {
      this.deptService.getDepartments().subscribe({
        next: (depts) => {
          this.departments.set(depts);
        },
        error: (err) => {
          console.error('Failed to load departments', err);
        }
      });
    }
  }

  ngOnDestroy() {
    if (this.processingInterval) {
      clearInterval(this.processingInterval);
    }
  }

  loadUploadedMonths() {
    const year = this.selectedYear();
    if (year === null || !this.departmentReady()) {
      this.uploadedMonths.set([]);
      return;
    }
    const deptId = this.isOwner() ? this.selectedDepartmentId() : null;
    this.service.getUploadedMonths(year, deptId).subscribe({
      next: (months) => {
        this.uploadedMonths.set(months);
        // If the currently selected month is now disabled, clear it
        if (this.selectedMonth() && months.includes(this.selectedMonth()!)) {
          this.selectedMonth.set(null);
        }
      },
      error: (err) => {
        console.error('Failed to load uploaded months', err);
      }
    });
  }

  onDepartmentChange(event: any) {
    const val = event.target.value;
    if (!val || val === '') {
      this.selectedDepartmentId.set(null);
      this.uploadedMonths.set([]);
      this.selectedMonth.set(null);
    } else {
      this.selectedDepartmentId.set(Number(val));
      this.selectedMonth.set(null);
      if (this.selectedYear() !== null) {
        this.loadUploadedMonths();
      }
    }
  }

  onYearChange(event: any) {
    const val = event.target.value;
    if (!val || val === '') {
      this.selectedYear.set(null);
      this.selectedMonth.set(null);
      this.uploadedMonths.set([]);
    } else {
      this.selectedYear.set(Number(val));
      this.selectedMonth.set(null);
      if (this.departmentReady()) {
        this.loadUploadedMonths();
      }
    }
  }

  onMonthChange(event: any) {
    const val = event.target.value;
    if (!val || val === '') {
      this.selectedMonth.set(null);
    } else {
      this.selectedMonth.set(Number(val));
    }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(false);
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging.set(false);
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.selectedFile.set(event.dataTransfer.files[0]);
    }
  }

  onFileSelected(event: any) {
    if (event.target.files && event.target.files.length > 0) {
      this.selectedFile.set(event.target.files[0]);
    }
  }

  removeFile() {
    this.selectedFile.set(undefined);
    this.reportName.set('');
    this.uploadProgress.set(0);
  }

  upload() {
    const file = this.selectedFile();
    const month = this.selectedMonth();
    const year = this.selectedYear();
    if (!file || !month || !year || !this.departmentReady()) return;

    this.isUploading.set(true);
    this.uploadProgress.set(0);

    this.service.upload(file, this.reportName(), month, year, this.selectedDepartmentId()).subscribe({
      next: (event: any) => {
        if (event.type === HttpEventType.UploadProgress) {
          if (event.total) {
            this.uploadProgress.set(Math.round(50 * (event.loaded / event.total)));
          }
        } else if (event.type === HttpEventType.Response) {
          // Upload complete, now trigger AI processing
          this.isUploading.set(false);
          this.isProcessing.set(true);
          
          this.processingInterval = setInterval(() => {
            if (this.uploadProgress() < 95) {
              this.uploadProgress.update(v => v + 1);
            }
          }, 400);

          const reportId = event.body?.id;
          
          if (reportId) {
            this.service.process(reportId).subscribe({
              next: () => {
                if (this.processingInterval) clearInterval(this.processingInterval);
                this.uploadProgress.set(100);
                this.isProcessing.set(false);
                setTimeout(() => {
                  this.router.navigate(['/reports', reportId, 'preview']);
                }, 400);
              },
              error: () => {
                if (this.processingInterval) clearInterval(this.processingInterval);
                this.isProcessing.set(false);
                this.showToast('Processing failed. You can retry from the reports list.', 'error');
                this.router.navigate(['/reports']);
              }
            });
          } else {
             if (this.processingInterval) clearInterval(this.processingInterval);
             this.isProcessing.set(false);
             this.router.navigate(['/reports']);
          }
        }
      },
      error: (err) => {
        if (this.processingInterval) clearInterval(this.processingInterval);
        this.isUploading.set(false);
        this.isProcessing.set(false);
        const message = err?.error?.message || 'Upload failed. Please try again.';
        this.showToast(message, 'error');
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
