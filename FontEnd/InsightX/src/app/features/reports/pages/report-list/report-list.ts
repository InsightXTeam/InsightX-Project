import { CommonModule } from "@angular/common";
import { Component, inject, signal, OnInit } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ReportService } from "../../../../core/services/report.service";
import { Report } from "../../../../core/models/report.model";

@Component({
  selector: 'app-report-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './report-list.html',
  styleUrl: './report-list.css',
})
export class ReportList implements OnInit {

  private service = inject(ReportService);
  private router = inject(Router);

  reports = signal<Report[]>([]);

  ngOnInit() {
    this.loadReports();
  }

  loadReports() {
    this.service.getReports().subscribe({
      next: (res) => this.reports.set(res),
      error: (err) => console.error('Failed to load reports', err)
    });
  }

  processingIds = signal<Set<number>>(new Set());

  getBadgeClass(status: string): string {
    const s = status?.toLowerCase() || '';
    if (s === 'done' || s === 'processed') return 'status-done';
    if (s === 'pending confirmation') return 'status-processing'; // changed color logic slightly
    if (s === 'pending' || s === 'processing') return 'status-processing';
    if (s === 'failed' || s === 'error') return 'status-failed';
    return '';
  }

  processReport(id: number) {
    this.processingIds.update(set => {
      const newSet = new Set(set);
      newSet.add(id);
      return newSet;
    });

    this.service.process(id).subscribe({
      next: () => {
        this.processingIds.update(set => {
          const newSet = new Set(set);
          newSet.delete(id);
          return newSet;
        });
        this.loadReports();
      },
      error: (err) => {
        console.error('Failed to process', err);
        this.processingIds.update(set => {
          const newSet = new Set(set);
          newSet.delete(id);
          return newSet;
        });
        alert('Failed to process the document.');
      }
    });
  }

  reviewReport(id: number) {
    // Navigate to the preview/handoff page
    this.router.navigate(['/reports', id, 'preview']);
  }

  viewResults(id: number) {
    this.router.navigate(['/reports', id, 'results']);
  }

  delete(id: number) {
    if(confirm('Are you sure you want to delete this report?')) {
      this.service.delete(id).subscribe(() => this.loadReports());
    }
  }
}