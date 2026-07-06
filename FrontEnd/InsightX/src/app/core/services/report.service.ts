import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Report } from '../models/report.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReportService {

  private http = inject(HttpClient);

  private apiUrl = `${environment.apiBaseUrl}/Reports`;

  // POST /api/Reports/upload
  upload(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(
      `${this.apiUrl}/upload`,
      formData,
      {
        reportProgress: true,
        observe: 'events'
      }
    );
  }

  // GET /api/Reports
  getReports(): Observable<Report[]> {
    return this.http.get<Report[]>(this.apiUrl);
  }

  // GET /api/Reports/{id}/status
  getStatus(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/status`);
  }

  // GET /api/Reports/{id}/preview
  getPreview(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/preview`);
  }

  // GET /api/Reports/{id}/text
  getText(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/text`);
  }

  // POST /api/Reports/{id}/confirm
  // Sends the manager-reviewed metrics back to the backend.
  // The backend then saves the corrected KPIs to DB and marks them confirmed.
  confirm(id: number, payload: { metrics: any[] }): Observable<any> {
    const body = JSON.stringify(payload);
    const headers = new HttpHeaders({ 'Content-Type': 'application/json' });
    return this.http.post(`${this.apiUrl}/${id}/confirm`, body, { headers });
  }

  // DELETE /api/Reports/{id}
  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  // POST /api/Reports/{id}/process
  process(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/process`, {});
  }
}