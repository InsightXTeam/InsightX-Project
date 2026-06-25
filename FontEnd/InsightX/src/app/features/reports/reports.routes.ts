import { Routes } from '@angular/router';
import { ReportUpload } from './pages/report-upload/report-upload';
import { ReportList } from './pages/report-list/report-list';
import { ReportPreview } from './pages/report-preview/report-preview';
import { ReportResults } from './pages/report-results/report-results';

export const routes: Routes = [
  {
    path: '',
    component: ReportList
  },
  {
    path: 'upload',
    component: ReportUpload
  },
  {
    path: ':id/preview',
    component: ReportPreview
  },
  {
    path: ':id/results',
    component: ReportResults
  }
];
