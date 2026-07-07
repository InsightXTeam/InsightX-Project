import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin, of, Observable } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { extractErrorMessage } from '../../../../shared/utils/error.utils';
import { UserService } from '../../../../core/services/user.service';
import { DepartmentService } from '../../../../core/services/department.service';
import { CompanyService } from '../../../../core/services/company.service';

interface KpiItem {
  name: string;
  threshold: number;
  unit: string;
  alertPercentageDiff: number;
  trendMonthsCount: number;
  thresholdDirection: number;
}

interface DepartmentItem {
  id?: number;
  name: string;
}

interface ManagerItem {
  name: string;
  email: string;
  password?: string;
  departmentId: number;
  isInvited: boolean;
}

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './onboarding.html',
  styleUrl: './onboarding.css'
})
export class OnboardingComponent {
  private readonly router = inject(Router);
  private readonly userService = inject(UserService);
  private readonly departmentService = inject(DepartmentService);
  private readonly companyService = inject(CompanyService);

  // Stepper state
  readonly currentStep = signal(1);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  // Data signals
  readonly kpis = signal<KpiItem[]>([]);
  readonly departments = signal<DepartmentItem[]>([]);
  readonly managers = signal<ManagerItem[]>([]);

  // Input bindings
  newKpi = { name: '', threshold: null as number | null, unit: '%', alertPercentageDiff: null as number | null, trendMonthsCount: null as number | null, thresholdDirection: 1 };
  newDeptName = '';
  newManager = { name: '', email: '', password: '', departmentId: 0 };

  // Computed departments that have been successfully created in the backend (i.e. possess database IDs)
  readonly createdDepartments = computed(() =>
    this.departments().filter(d => d.id !== undefined && d.id !== null)
  );

  // KPI Actions
  addKpiPreset(name: string, threshold: number, unit: string, alertPercentageDiff: number = 10, trendMonthsCount: number = 3, thresholdDirection: number = 1): void {
    const exists = this.kpis().some(k => k.name.toLowerCase() === name.toLowerCase());
    if (!exists) {
      this.kpis.update(list => [...list, { name, threshold, unit, alertPercentageDiff, trendMonthsCount, thresholdDirection }]);
    }
  }

  addKpi(): void {
    if (!this.newKpi.name.trim() || this.newKpi.threshold === null || this.newKpi.alertPercentageDiff === null || this.newKpi.trendMonthsCount === null) return;

    const exists = this.kpis().some(k => k.name.toLowerCase() === this.newKpi.name.trim().toLowerCase());
    if (!exists) {
      this.kpis.update(list => [...list, {
        name: this.newKpi.name.trim(),
        threshold: this.newKpi.threshold!,
        unit: this.newKpi.unit.trim(),
        alertPercentageDiff: this.newKpi.alertPercentageDiff!,
        trendMonthsCount: this.newKpi.trendMonthsCount!,
        thresholdDirection: Number(this.newKpi.thresholdDirection)
      }]);
    }

    // Clear inputs
    this.newKpi = { name: '', threshold: null, unit: '%', alertPercentageDiff: null, trendMonthsCount: null, thresholdDirection: 1 };
  }

  removeKpi(index: number): void {
    this.kpis.update(list => list.filter((_, i) => i !== index));
  }

  // Department Actions
  addDeptPreset(name: string): void {
    const exists = this.departments().some(d => d.name.toLowerCase() === name.toLowerCase());
    if (!exists) {
      this.departments.update(list => [...list, { name }]);
    }
  }

  addDept(): void {
    const trimmed = this.newDeptName.trim();
    if (!trimmed) return;

    const exists = this.departments().some(d => d.name.toLowerCase() === trimmed.toLowerCase());
    if (!exists) {
      this.departments.update(list => [...list, { name: trimmed }]);
    }

    this.newDeptName = '';
  }

  removeDept(index: number): void {
    this.departments.update(list => list.filter((_, i) => i !== index));
  }

  getDeptNameById(id: number): string {
    const dept = this.departments().find(d => d.id === id);
    return dept ? dept.name : 'Unknown';
  }

  // Manager Actions
  inviteManager(): void {
    const { name, email, password, departmentId } = this.newManager;
    if (!name || !email || !password || !departmentId) return;

    this.isSubmitting.set(true);

    const payload = {
      name,
      email,
      password,
      departmentId: Number(departmentId)
    };

    this.userService.inviteManager(payload).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.managers.update(list => [...list, {
          name,
          email,
          departmentId: Number(departmentId),
          isInvited: true
        }]);

        // Reset manager inputs except department selection for convenience
        this.newManager = { name: '', email: '', password: '', departmentId };
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to invite manager. Make sure the email is unique.'));
      }
    });
  }

  // Stepper Transitions
  prevStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update(s => s - 1);
    }
  }

  nextStep(): void {
    this.errorMessage.set(null); // Clear previous errors on step transition
    const step = this.currentStep();
    if (step === 1) {
      this.submitKpis();
    } else if (step === 2) {
      this.submitDepartments();
    } else if (step === 3) {
      this.finishOnboarding();
    }
  }

  skipOnboarding(): void {
    this.router.navigate(['/dashboard']);
  }

  private submitKpis(): void {
    if (this.kpis().length === 0) {
      // Allow moving next without KPIs (or alert)
      this.currentStep.set(2);
      return;
    }

    this.isSubmitting.set(true);

    const payload = {
      kpis: this.kpis().map(k => ({
        name: k.name,
        threshold: k.threshold,
        unit: k.unit,
        alertPercentageDiff: k.alertPercentageDiff,
        trendMonthsCount: k.trendMonthsCount,
        thresholdDirection: Number(k.thresholdDirection)
      }))
    };

    this.companyService.setupCompany(payload.kpis).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.currentStep.set(2);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(extractErrorMessage(err, 'Failed to configure company KPIs. Please try again.'));
      }
    });
  }

  private submitDepartments(): void {
    const pendingDepts = this.departments().filter(d => !d.id);

    if (pendingDepts.length === 0) {
      if (this.createdDepartments().length > 0) {
        this.currentStep.set(3);
      } else {
        this.errorMessage.set('Please create at least one department to proceed.');
      }
      return;
    }

    this.isSubmitting.set(true);

    // Call POST /departments for each department sequentially or concurrently
    const calls: Observable<any>[] = pendingDepts.map(dept =>
      this.departmentService.createDepartment(dept.name).pipe(
        tap(res => {
          // Update the department in local state with the returned ID
          this.departments.update(list =>
            list.map(d => d.name === dept.name ? { ...d, id: res.id } : d)
          );
        }),
        catchError(err => {
          console.error(`Failed to create department: ${dept.name}`, err);
          return of(null); // Continue other requests on failure
        })
      )
    );

    forkJoin(calls).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        if (this.createdDepartments().length > 0) {
          // Auto-select the first department in the manager setup select dropdown
          this.newManager.departmentId = this.createdDepartments()[0].id!;
          this.currentStep.set(3);
        } else {
          this.errorMessage.set('Failed to create departments. Please verify connection and try again.');
        }
      },
      error: () => {
        this.isSubmitting.set(false);
        this.errorMessage.set('An error occurred while creating departments. Please try again.');
      }
    });
  }

  private finishOnboarding(): void {
    // Navigate to dashboard — the Owner's primary landing page
    this.router.navigate(['/dashboard']);
  }

}

