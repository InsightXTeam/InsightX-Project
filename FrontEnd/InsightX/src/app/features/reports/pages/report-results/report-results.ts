import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { ReportService } from '../../../../core/services/report.service';
import { ExtractedMetric } from '../../../../core/models/extracted-metric';
import { forkJoin } from 'rxjs'; // Import it at the top

@Component({
  selector: 'app-report-results',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './report-results.html',
  styleUrls: ['./report-results.css'],
})
export class ReportResults implements OnInit {
  private service = inject(ReportService);
  private route = inject(ActivatedRoute);

  reportId!: number;

  // Use Signals for state management to guarantee reactivity
  extractedText = signal<string>('');
  kpis = signal<ExtractedMetric[]>([]);
  isLoading = signal<boolean>(true);

  ngOnInit() {
    this.reportId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.reportId) {
      this.loadResults();
    }
  }

  loadResults() {
    this.isLoading.set(true);

    forkJoin({
      textResult: this.service.getText(this.reportId),
      kpiResult: this.service.getPreview(this.reportId),
    }).subscribe({
      next: (results: { textResult: any; kpiResult: any }) => {
        const textValue =
          typeof results.textResult === 'string'
            ? results.textResult
            : results.textResult?.text || '';

        this.extractedText.set(textValue);
        this.kpis.set(Array.isArray(results.kpiResult) ? results.kpiResult : []);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      },
    });
  }
}








