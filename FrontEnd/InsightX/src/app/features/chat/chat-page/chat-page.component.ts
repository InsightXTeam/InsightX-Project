import { Component, ElementRef, ViewChild, AfterViewChecked, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AgentService, ChatSessionDto } from '../../../core/services/agent.service';

interface ChatMessage {
  id: number;
  text: string;
  sender: 'user' | 'ai';
  timestamp: Date;
}
import { MarkdownPipe } from '../../../shared/pipes/markdown.pipe';

@Component({
  selector: 'app-chat-page',
  standalone: true,
  imports: [CommonModule, FormsModule, MarkdownPipe],
  templateUrl: './chat-page.component.html',
  styleUrl: './chat-page.component.css'
})
export class ChatPageComponent implements OnInit, AfterViewChecked {
  @ViewChild('scrollContainer') private scrollContainer!: ElementRef;

  private agentService = inject(AgentService);
  private cdr = inject(ChangeDetectorRef);
  sessionId: string = crypto.randomUUID(); // Public so HTML can bind to it
  sessions: ChatSessionDto[] = [];

  messages: ChatMessage[] = [];

  newMessage: string = '';
  isTyping: boolean = false;
  
  showDeleteModal: boolean = false;
  sessionToDelete: string | null = null;
  isDeleting: boolean = false;

  ngOnInit() {
    this.loadSessions();
    this.newChat(); // Initialize a new chat by default
  }

  loadSessions() {
    this.agentService.getSessions().subscribe({
      next: (data: any) => {
        const dataArray = Array.isArray(data) ? data : (data.value || data.data || []);

        this.sessions = dataArray.map((s: any) => {
          let dateStr = s.createdAt || s.CreatedAt;
          if (dateStr && typeof dateStr === 'string' && !dateStr.endsWith('Z')) dateStr += 'Z';
          return {
            sessionId: s.sessionId || s.SessionId,
            title: s.title || s.Title,
            createdAt: dateStr
          };
        });

        // Force Angular to update the UI
        this.cdr.detectChanges();
      },
      error: (err) => console.error("Error loading sessions:", err)
    });
  }

  newChat() {
    this.sessionId = crypto.randomUUID();
    this.messages = [
      {
        id: 1,
        text: 'Hello! I am InsightX AI, your enterprise business intelligence assistant. How can I help you analyze your data today?',
        sender: 'ai',
        timestamp: new Date()
      }
    ];
  }

  selectSession(id: string) {
    if (!id) return;
    this.sessionId = id;
    this.messages = [];
    this.agentService.getHistory(id).subscribe({
      next: (data: any[]) => {
        this.messages = data.map((d, index) => {
          const senderString = (d.sender || d.Sender || 'ai').toLowerCase();
          let dateStr = d.createdAt || d.CreatedAt;
          if (dateStr && typeof dateStr === 'string' && !dateStr.endsWith('Z')) dateStr += 'Z';

          return {
            id: index,
            text: d.message || d.Message || '',
            sender: senderString.includes('user') ? 'user' : 'ai',
            timestamp: new Date(dateStr || Date.now())
          };
        });
        this.scrollToBottom();
        this.cdr.detectChanges();
      },
      error: (err) => console.error('Error fetching history:', err)
    });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    try {
      this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
    } catch (err) { }
  }

  sendMessage() {
    if (!this.newMessage.trim()) return;

    const userMessage = this.newMessage.trim();
    this.messages.push({
      id: Date.now(),
      text: userMessage,
      sender: 'user',
      timestamp: new Date()
    });

    this.newMessage = '';
    this.isTyping = true;
    this.scrollToBottom();

    this.agentService.sendMessage({
      sessionId: this.sessionId,
      message: userMessage
    }).subscribe({
      next: (response) => {
        this.isTyping = false;
        this.messages.push({
          id: Date.now(),
          text: response.message,
          sender: 'ai',
          timestamp: new Date(response.createdAt)
        });
        this.loadSessions();
        this.scrollToBottom();
      },
      error: (err) => {
        console.error('Error sending message:', err);
        this.isTyping = false;
        this.messages.push({
          id: Date.now(),
          text: "I'm sorry, I encountered an error communicating with the server.",
          sender: 'ai',
          timestamp: new Date()
        });
        this.scrollToBottom();
      }
    });
  }

  deleteSession(id: string, event: Event) {
    event.stopPropagation();
    this.sessionToDelete = id;
    this.showDeleteModal = true;
  }

  confirmDelete() {
    if (!this.sessionToDelete) return;
    
    this.isDeleting = true;
    this.cdr.detectChanges();
    
    const id = this.sessionToDelete;
    this.agentService.deleteSession(id).subscribe({
      next: () => {
        if (this.sessionId === id) {
          this.newChat();
        }
        this.loadSessions();
        this.closeDeleteModal();
      },
      error: (err) => {
        console.error('Error deleting session:', err);
        this.closeDeleteModal();
      }
    });
  }

  closeDeleteModal() {
    this.showDeleteModal = false;
    this.sessionToDelete = null;
    this.isDeleting = false;
    this.cdr.detectChanges();
  }

  handleKeyDown(event: KeyboardEvent) {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }
}
