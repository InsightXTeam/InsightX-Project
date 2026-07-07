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

  async *sendMessageStream(request: ChatRequestDto): AsyncIterableIterator<string> {
    const token = localStorage.getItem('insightx_access_token');
    
    const response = await fetch(`${this.apiUrl}/stream`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { 'Authorization': `Bearer ${token}` } : {})
      },
      body: JSON.stringify(request)
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    if (!response.body) {
      throw new Error('ReadableStream not supported in this browser.');
    }

    const reader = response.body.getReader();
    const decoder = new TextDecoder('utf-8');
    let buffer = '';

    try {
      while (true) {
        const { done, value } = await reader.read();
        if (done) break;
        
        buffer += decoder.decode(value, { stream: true });
        
        const lines = buffer.split('\n');
        buffer = lines.pop() || '';
        
        for (const line of lines) {
          if (line.startsWith('data: ')) {
            const data = line.substring(6);
            yield data.replace(/\\n/g, '\n');
          }
        }
      }
    } finally {
      reader.releaseLock();
    }
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
