import { CommonModule } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { ReportService } from "../../services/report-service";
import { Report } from "../../models/report";

@Component({
  selector: 'app-report-list',
  imports: [CommonModule],
  templateUrl: './report-list.html',
  styleUrl: './report-list.css',
})
export class ReportList {

  private service = inject(ReportService);

  reports = signal<Report[]>([]);
  previewData = signal<any>(null);
  textData = signal<any>(null);

  ngOnInit() {
    this.loadReports();
  }

  loadReports() {
    this.service.getReports().subscribe(res => {
      this.reports.set(res);
    });
  }

  preview(id: number) {
    this.service.getPreview(id).subscribe(res => {
      this.previewData.set(res);
    });
  }

  getText(id: number) {
    this.service.getText(id).subscribe(res => {
      this.textData.set(res);
    });
  }

  closeText() {
    this.textData.set(null);
  }

  process(id: number) {
    this.service.process(id).subscribe(() => this.loadReports());
  }

  confirm(id: number) {
    this.service.confirm(id).subscribe(() => this.loadReports());
  }

  delete(id: number) {
    this.service.delete(id).subscribe(() => this.loadReports());
  }
}