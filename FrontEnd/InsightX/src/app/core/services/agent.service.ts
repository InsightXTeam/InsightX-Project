import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface ChatRequestDto {
  message: string;
  sessionId: string;
}

export interface ChatResponseDto {
  sessionId: string;
  message: string;
  sender: string;
  createdAt: string;
}

export interface ChatSessionDto {
  sessionId: string;
  title: string;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class AgentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiBaseUrl}/agent`;

  sendMessage(request: ChatRequestDto): Observable<ChatResponseDto> {
    return this.http.post<ChatResponseDto>(`${this.apiUrl}/chat`, request);
  }

  getHistory(sessionId: string): Observable<ChatResponseDto[]> {
    return this.http.get<ChatResponseDto[]>(`${this.apiUrl}/chat/${sessionId}`);
  }

  getSessions(): Observable<ChatSessionDto[]> {
    return this.http.get<ChatSessionDto[]>(`${this.apiUrl}/sessions`);
  }

  deleteSession(sessionId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/chat/${sessionId}`);
  }
}
