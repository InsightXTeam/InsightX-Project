import { Component, inject, signal, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService } from '../../../../core/services/report.service';
import { HttpEventType } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { ToastNotificationComponent } from '../../../../shared/components/toast-notification/toast-notification';

@Component({
  selector: 'app-report-upload',
  standalone: true,
  imports: [CommonModule, RouterModule, ToastNotificationComponent],
  templateUrl: './report-upload.html',
  styleUrl: './report-upload.css',
})
export class ReportUpload {
  private service = inject(ReportService);
  private router = inject(Router);

  selectedFile = signal<File | undefined>(undefined);
  reportName = signal<string>('');
  isDragging = signal<boolean>(false);
  isUploading = signal<boolean>(false);
  isProcessing = signal<boolean>(false);
  uploadProgress = signal<number>(0);
  toastOpen = signal(false);
  toastMessage = signal('');
  toastType = signal<'success' | 'error' | 'info'>('success');
  private processingInterval: any;

  ngOnDestroy() {
    if (this.processingInterval) {
      clearInterval(this.processingInterval);
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
    if (!file) return;

    this.isUploading.set(true);
    this.uploadProgress.set(0);

    this.service.upload(file, this.reportName()).subscribe({
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
      error: () => {
        if (this.processingInterval) clearInterval(this.processingInterval);
        this.isUploading.set(false);
        this.isProcessing.set(false);
        this.showToast('Upload failed. Please try again.', 'error');
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
