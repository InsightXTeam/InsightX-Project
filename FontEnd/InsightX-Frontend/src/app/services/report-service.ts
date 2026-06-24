import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Report } from '../models/report';

@Injectable({
  providedIn: 'root'
})
export class ReportService {

  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:7131/api/Reports';

  // POST /api/Reports/upload
  upload(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http.post(
      `${this.apiUrl}/upload`,
      formData
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

  // GET /api/Reports/{id}/text  ✅ NEW
  getText(id: number): Observable<any> {
    return this.http.get(`${this.apiUrl}/${id}/text`);
  }

  // POST /api/Reports/{id}/confirm
  confirm(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/confirm`, {});
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