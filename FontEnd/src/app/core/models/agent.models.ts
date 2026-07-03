export interface RagSource {
  text: string;
  month: string;
  year: number;
  reportId: number;
  departmentId: number;
}

export interface ChatResponse {
  answer: string;
  sources: RagSource[];
}

export interface ChatHistoryItem {
  id: string;
  question: string;
  answer: string;
  createdAt: string;
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  sources?: RagSource[];
  createdAt: Date;
}
