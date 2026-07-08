import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { CompanyService } from '../../../../core/services/company.service';
import { catchError, of } from 'rxjs';

interface DemoDepartment {
  id: string;
  name: string;
  icon: string;
  kpis: { label: string; value: string; change: string; isPositive: boolean }[];
  aiInsight: string;
  aiRecommendation: string;
}

interface FaqItem {
  question: string;
  answer: string;
  isOpen: boolean;
}

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.css'
})
export class LandingPageComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly companyService = inject(CompanyService);
  private readonly router = inject(Router);

  // State for interactive demo
  readonly selectedDeptId = signal<string>('sales');

  readonly departments: DemoDepartment[] = [
    {
      id: 'sales',
      name: 'Sales & Revenue',
      icon: '💼',
      kpis: [
        { label: 'Quarterly Revenue', value: '$4,285,000', change: '+14.8%', isPositive: true },
        { label: 'Win Rate', value: '64.2%', change: '+5.1%', isPositive: true },
        { label: 'Sales Cycle Length', value: '18 Days', change: '-12.0%', isPositive: true }
      ],
      aiInsight: 'Revenue velocity is up 14.8% due to higher closing rates in Enterprise accounts. Anomaly detected in North America region with 22% faster deal progression.',
      aiRecommendation: 'Recommend reallocating 15% of SDR outreach budget toward Enterprise healthcare leads for Q4.'
    },
    {
      id: 'marketing',
      name: 'Marketing & Growth',
      icon: '📣',
      kpis: [
        { label: 'Customer Acquisition Cost', value: '$142', change: '-18.4%', isPositive: true },
        { label: 'Marketing Qualified Leads', value: '3,420', change: '+28.5%', isPositive: true },
        { label: 'ROAS (Ad Spend)', value: '4.8x', change: '+0.6x', isPositive: true }
      ],
      aiInsight: 'Organic search traffic increased by 34% following the new content rollout. Paid ad efficiency improved significantly across LinkedIn campaigns.',
      aiRecommendation: 'Scale top-performing AI webinar ad creatives by 25% while CAC remains below the $150 threshold.'
    },
    {
      id: 'logistics',
      name: 'Logistics & Operations',
      icon: '📦',
      kpis: [
        { label: 'On-Time Fulfillment', value: '98.7%', change: '+1.4%', isPositive: true },
        { label: 'Average Transit Time', value: '2.1 Days', change: '-0.4 Days', isPositive: true },
        { label: 'Supply Chain Efficiency', value: '94.5%', change: '+3.2%', isPositive: true }
      ],
      aiInsight: 'Smart alert prevented a potential 12% fulfillment delay by identifying vendor bottleneck in Warehouse B 48 hours in advance.',
      aiRecommendation: 'Automate secondary carrier routing for routes exceeding 85% capacity during peak holiday shipping.'
    },
    {
      id: 'engineering',
      name: 'Engineering & Tech',
      icon: '💻',
      kpis: [
        { label: 'System Uptime', value: '99.99%', change: '+0.04%', isPositive: true },
        { label: 'Sprint Velocity', value: '84 Pts', change: '+12.0%', isPositive: true },
        { label: 'Mean Time to Resolve', value: '14 Mins', change: '-35.0%', isPositive: true }
      ],
      aiInsight: 'Automated code review metrics show a 40% reduction in production bug regression after implementing AI unit test verification.',
      aiRecommendation: 'Maintain current deployment frequency of 4x/day; system load distribution is optimal across all clusters.'
    }
  ];

  // State for FAQ accordion
  readonly faqItems = signal<FaqItem[]>([
    {
      question: 'What makes InsightX AI different from traditional BI tools?',
      answer: 'Traditional BI tools like Tableau or PowerBI require manual data wrangling and complex SQL queries. InsightX AI combines real-time departmental KPI tracking with an autonomous AI engine that proactively explains WHY metrics change, predicts anomalies before they happen, and generates executive reports automatically.',
      isOpen: true
    },
    {
      question: 'How does cascading departmental KPI linking work?',
      answer: 'InsightX allows Company Owners to define top-level strategic targets and link them directly to departmental KPIs. When a Department Manager updates their metrics or integrates data, the parent company KPIs automatically calculate and update in real-time, ensuring complete alignment from leadership to execution.',
      isOpen: false
    },
    {
      question: 'Can I trust the AI reports and automated recommendations?',
      answer: 'Yes! Our AI Report Accuracy engine continuously cross-references raw metric logs against historical trends and statistical variance models. Every AI-generated summary includes a verification score and clickable data citations so you can verify every insight.',
      isOpen: false
    },
    {
      question: 'How fast can our company onboard and start tracking?',
      answer: 'In under 2 minutes. When an Owner registers a company, our intuitive onboarding guide helps set up departments, invite team members, and choose pre-built KPI templates tailored to your industry.',
      isOpen: false
    }
  ]);

  ngOnInit(): void {
    // If the user is already authenticated, redirect them immediately to their app dashboard
    if (this.authService.isAuthenticated()) {
      const user = this.authService.currentUser();
      if (!user) return;

      if (user.role === 'SuperAdmin') {
        this.router.navigate(['/users/owners']);
      } else if (user.role === 'Owner') {
        this.companyService.getCompanyMe().pipe(
          catchError(() => of(null))
        ).subscribe(company => {
          if (company && company.departments && company.departments.length > 0) {
            this.router.navigate(['/dashboard']);
          } else {
            this.router.navigate(['/auth/onboarding']);
          }
        });
      } else {
        this.router.navigate(['/dashboard']);
      }
    }
  }

  selectDepartment(deptId: string): void {
    this.selectedDeptId.set(deptId);
  }

  get currentDepartment(): DemoDepartment {
    return this.departments.find(d => d.id === this.selectedDeptId()) || this.departments[0];
  }

  toggleFaq(index: number): void {
    this.faqItems.update(items =>
      items.map((item, i) => i === index ? { ...item, isOpen: !item.isOpen } : item)
    );
  }
}
