import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AgentService } from '../../core/services/agent-api.service';
import { ChatMessage, RagSource } from '../../core/models/agent.models';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.css']
})
export class ChatComponent implements OnInit {
  @ViewChild('messagesContainer') messagesContainer?: ElementRef<HTMLDivElement>;

  public messages: ChatMessage[] = [];
  public question = '';
  public isProcessing = false;

  constructor(private agentService: AgentService) {}

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.agentService.getHistory().subscribe(history => {
      this.messages = history.flatMap(item => [
        {
          id: `${item.id}-q`,
          role: 'user' as const,
          content: item.question,
          createdAt: new Date(item.createdAt)
        },
        {
          id: `${item.id}-a`,
          role: 'assistant' as const,
          content: item.answer,
          createdAt: new Date(item.createdAt)
        }
      ]);
      this.scrollToBottom();
    });
  }

  sendMessage(): void {
    const text = this.question.trim();
    if (!text || this.isProcessing) return;

    const userMessage: ChatMessage = {
      id: crypto.randomUUID(),
      role: 'user',
      content: text,
      createdAt: new Date()
    };

    this.messages.push(userMessage);
    this.question = '';
    this.isProcessing = true;
    this.scrollToBottom();

    this.agentService.sendMessage(text).subscribe({
      next: response => {
        this.messages.push({
          id: crypto.randomUUID(),
          role: 'assistant',
          content: response.answer,
          sources: response.sources,
          createdAt: new Date()
        });
        this.isProcessing = false;
        this.scrollToBottom();
      },
      error: () => {
        this.messages.push({
          id: crypto.randomUUID(),
          role: 'assistant',
          content: 'Sorry, I could not process your request. Please try again.',
          createdAt: new Date()
        });
        this.isProcessing = false;
        this.scrollToBottom();
      }
    });
  }

  clearHistory(): void {
    this.agentService.clearHistory().subscribe(() => {
      this.messages = [];
    });
  }

  formatSources(sources: RagSource[]): string {
    const unique = [...new Set(sources.map(s => `${s.month} ${s.year} (Report #${s.reportId})`))];
    return unique.join(', ');
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      const el = this.messagesContainer?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    }, 50);
  }
}
