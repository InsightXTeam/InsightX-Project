import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})

export class ReportService {


  private http = inject(HttpClient);


  private apiUrl =
  'https://localhost:7131/api/Reports';



  upload(file: File): Observable<any>{

    const formData = new FormData();

    formData.append(
      'file',
      file
    );


    return this.http.post(
      `${this.apiUrl}/upload`,
      formData
    );

  }



  getReports(): Observable<Report[]>{

    return this.http.get<Report[]>(
      this.apiUrl
    );

  }



  getPreview(id:number){

    return this.http.get(
      `${this.apiUrl}/${id}/preview`
    );

  }


}