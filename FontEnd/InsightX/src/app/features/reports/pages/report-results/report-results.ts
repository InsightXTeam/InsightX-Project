import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ReportService } from '../../../../core/services/report.service';

export interface ExtractedMetric {
  KPIName: string;
  Value: number;
  Month: string;
  Year: number;
}

@Component({
  selector: 'app-report-results',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './report-results.html',
  styleUrls: ['./report-results.css']
})
export class ReportResults implements OnInit {
  private service = inject(ReportService);
  private route = inject(ActivatedRoute);

  reportId!: number;
  
  // Use Signals for state management to guarantee reactivity
  extractedText = signal<string>('');
  kpis = signal<ExtractedMetric[]>([]);
  isLoading = signal<boolean>(true);

  private pendingRequests = 2;

  ngOnInit() {
    this.reportId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.reportId) {
      this.loadResults();
    }
  }

  loadResults() {
    this.isLoading.set(true);
    
    // Load extracted text
    this.service.getText(this.reportId).subscribe({
      next: (res: any) => {
        const textValue = typeof res === 'string' ? res : (res.text || '');
        this.extractedText.set(textValue);
        this.checkLoadingComplete();
      },
      error: (err) => {
        console.error('Failed to load text', err);
        this.checkLoadingComplete();
      }
    });

    // Load KPIs
    this.service.getPreview(this.reportId).subscribe({
      next: (res: any) => {
        this.kpis.set(Array.isArray(res) ? res : []);
        this.checkLoadingComplete();
      },
      error: (err) => {
        console.error('Failed to load KPIs', err);
        this.checkLoadingComplete();
      }
    });
  }

  checkLoadingComplete() {
    this.pendingRequests--;
    if (this.pendingRequests <= 0) {
      this.isLoading.set(false);
    }
  }
}
