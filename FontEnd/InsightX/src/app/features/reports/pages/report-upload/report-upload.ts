import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReportService } from '../../../../core/services/report.service';
import { HttpEventType } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-report-upload',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './report-upload.html',
  styleUrl: './report-upload.css',
})
export class ReportUpload {
  private service = inject(ReportService);
  private router = inject(Router);

  selectedFile = signal<File | undefined>(undefined);
  isDragging = signal<boolean>(false);
  isUploading = signal<boolean>(false);
  isProcessing = signal<boolean>(false);
  uploadProgress = signal<number>(0);

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
    this.uploadProgress.set(0);
  }

  upload() {
    const file = this.selectedFile();
    if (!file) return;

    this.isUploading.set(true);
    this.uploadProgress.set(0);

    this.service.upload(file).subscribe({
      next: (event: any) => {
        if (event.type === HttpEventType.UploadProgress) {
          if (event.total) {
            this.uploadProgress.set(Math.round(100 * (event.loaded / event.total)));
          }
        } else if (event.type === HttpEventType.Response) {
          // Upload complete, now waiting for AI processing
          this.isUploading.set(false);
          this.isProcessing.set(true);
          
          setTimeout(() => {
            this.isProcessing.set(false);
            this.router.navigate(['/reports']);
          }, 1500);
        }
      },
      error: (err) => {
        console.error('Upload Failed', err);
        this.isUploading.set(false);
        this.isProcessing.set(false);
        alert('Upload failed. Please try again.');
      }
    });
  }
}