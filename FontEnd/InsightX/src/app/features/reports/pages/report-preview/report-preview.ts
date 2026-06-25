import { ChangeDetectorRef, Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReportService } from '../../../../core/services/report.service';

@Component({
  selector: 'app-report-preview',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './report-preview.html',
  styleUrls: ['./report-preview.css']
})
export class ReportPreview implements OnInit {
  private service = inject(ReportService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  reportId!: number;
  form!: FormGroup;
  isLoading = true;
  isSaving = false;

  ngOnInit() {
    this.reportId = Number(this.route.snapshot.paramMap.get('id'));
    
    // Initialize form with a single text area control
    this.form = this.fb.group({
      extractedText: ['', Validators.required]
    });

    if (this.reportId) {
      this.fetchExtractedText();
    }
  }

  fetchExtractedText() {
    // We fetch the raw extracted text instead of KPIs
    this.service.getText(this.reportId).subscribe({
      next: (res) => {
        // Backend returns { text: '...' }
        const textValue = typeof res === 'string' ? res : res.text || JSON.stringify(res);
        this.form.patchValue({
          extractedText: textValue
        });
        this.isLoading = false;
        this.cdr.detectChanges(); // Force update the view
      },
      error: (err) => {
        console.error('Failed to load extracted text', err);
        this.isLoading = false;
        this.cdr.detectChanges(); // Force update the view
        alert('Could not load text for review.');
      }
    });
  }

  confirmAndSave() {
    if (!this.form || this.form.invalid) return;

    this.isSaving = true;
    const finalData = this.form.value;

    /*
     * =====================================================================
     * CRITICAL HAND-OFF POINT: 
     * When 'confirmAndSave' succeeds, the Manager has finalized the RAW TEXT.
     * 
     * Backend Controller Implications (as specified by user):
     * 1. The backend saves the corrected extractedText to the DB.
     * 2. The backend THEN triggers the AI to extract KPIs from this text.
     * 3. The extracted KPIs are then stored in the database.
     * 4. This eventually triggers Person 3 (Anomaly Detection) and Person 4 (RAG).
     * =====================================================================
     */
    this.service.confirm(this.reportId, finalData).subscribe({
      next: () => {
        this.isSaving = false;
        this.router.navigate(['/reports']);
      },
      error: (err) => {
        console.error('Failed to confirm data', err);
        this.isSaving = false;
        alert('Failed to save data. Please try again.');
      }
    });
  }
}
