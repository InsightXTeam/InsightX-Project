import { Routes } from '@angular/router';
import { ReportUpload } from './components/report-upload/report-upload';
import { ReportList } from './components/report-list/report-list';

export const routes: Routes = [
{
path:'',
component:ReportUpload
},


{
path:'reports',
component:ReportList
}];


