import { Component, OnInit, ElementRef, ViewChild, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiChatService } from '../../core/services/ai-chat.service';
import { AiHistoryItem } from '../../core/models/ai-chat.model';

interface ChatMessage {
  sender: 'user' | 'ai';
  text: string;
  timestamp: Date;
}

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  template: `
    <div class="chat-page-container">
      <!-- Legal Disclaimer Banner -->
      <div class="alert alert-info disclaimer-banner">
        <span class="info-icon">⚖️</span>
        <div>
          <strong>Informational Guidance Notice:</strong>
          TaxStream AI answers are for educational & informational assistance only.
          AI is not a certified tax professional and cannot calculate official tax liability.
          Always confirm eligibility with authoritative tax guidelines.
        </div>
      </div>

      <div class="chat-card card">
        <div class="chat-header">
          <div class="header-info">
            <span class="ai-avatar">🤖</span>
            <div>
              <h3>TaxStream AI Assistant</h3>
              <span class="status-indicator">Connected to your tax records</span>
            </div>
          </div>
          <button (click)="loadHistory()" class="btn btn-secondary btn-sm" title="Refresh history">↻ Refresh</button>
        </div>

        <!-- Chat Messages Area -->
        <div class="chat-messages" #scrollContainer>
          <div *ngIf="messages.length === 0 && !loading" class="chat-welcome">
            <div class="welcome-icon">💬</div>
            <h4>How can I help with your taxes today?</h4>
            <p>I have access only to your authorized tax summaries to provide contextual help.</p>

            <div class="suggestions-title">Try asking:</div>
            <div class="suggestions-list">
              <button
                *ngFor="let s of suggestedQuestions"
                (click)="sendSuggestion(s)"
                class="suggestion-chip">
                {{ s }}
              </button>
            </div>
          </div>

          <div
            *ngFor="let msg of messages"
            class="message-row"
            [class.user-row]="msg.sender === 'user'"
            [class.ai-row]="msg.sender === 'ai'">

            <div class="message-bubble" [class.user-bubble]="msg.sender === 'user'" [class.ai-bubble]="msg.sender === 'ai'">
              <div class="message-sender">{{ msg.sender === 'user' ? 'You' : 'TaxStream AI' }}</div>
              <div class="message-text">{{ msg.text }}</div>
              <div class="message-time">{{ msg.timestamp | date:'shortTime' }}</div>
            </div>
          </div>

          <div *ngIf="loading" class="message-row ai-row">
            <div class="message-bubble ai-bubble typing">
              <span class="dot"></span>
              <span class="dot"></span>
              <span class="dot"></span>
            </div>
          </div>
        </div>

        <!-- Input Box -->
        <div class="chat-input-wrapper">
          <form (ngSubmit)="sendMessage()" class="chat-input-form">
            <input
              type="text"
              class="form-control chat-input"
              [(ngModel)]="userPrompt"
              name="prompt"
              placeholder="Ask a question about deductions, tax years, or rules..."
              [disabled]="loading"
              autocomplete="off"
            />
            <button type="submit" class="btn btn-primary" [disabled]="loading || !userPrompt.trim()">
              Send
            </button>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .chat-page-container {
      max-width: 900px;
      margin: 1.5rem auto;
      padding: 0 1rem;
    }
    .disclaimer-banner {
      display: flex;
      align-items: flex-start;
      gap: 0.75rem;
      font-size: 0.85rem;
      margin-bottom: 1.25rem;
    }
    .info-icon {
      font-size: 1.25rem;
    }
    .chat-card {
      height: 75vh;
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }
    .chat-header {
      padding: 1rem 1.5rem;
      border-bottom: 1px solid var(--border);
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: #fafafa;
    }
    .header-info {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }
    .ai-avatar {
      font-size: 1.75rem;
      background: var(--primary-light);
      padding: 0.25rem 0.5rem;
      border-radius: 50%;
    }
    .header-info h3 {
      font-size: 1.1rem;
      color: var(--primary);
    }
    .status-indicator {
      font-size: 0.75rem;
      color: var(--accent-dark);
      font-weight: 500;
    }
    .chat-messages {
      flex: 1;
      overflow-y: auto;
      padding: 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
      background: #fdfdfd;
    }
    .chat-welcome {
      text-align: center;
      padding: 2.5rem 1rem;
      max-width: 500px;
      margin: 0 auto;
    }
    .welcome-icon {
      font-size: 2.5rem;
      margin-bottom: 0.5rem;
    }
    .chat-welcome h4 {
      color: var(--primary);
      margin-bottom: 0.5rem;
    }
    .chat-welcome p {
      font-size: 0.875rem;
      color: var(--text-secondary);
      margin-bottom: 1.5rem;
    }
    .suggestions-title {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--text-muted);
      text-transform: uppercase;
      margin-bottom: 0.75rem;
    }
    .suggestions-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .suggestion-chip {
      background: white;
      border: 1px solid var(--border);
      border-radius: var(--radius-sm);
      padding: 0.5rem 1rem;
      font-size: 0.85rem;
      color: var(--text-secondary);
      cursor: pointer;
      text-align: left;
      transition: all 0.15s ease;
    }
    .suggestion-chip:hover {
      border-color: var(--primary);
      color: var(--primary);
      background: var(--primary-light);
    }
    .message-row {
      display: flex;
      width: 100%;
    }
    .user-row {
      justify-content: flex-end;
    }
    .ai-row {
      justify-content: flex-start;
    }
    .message-bubble {
      max-width: 75%;
      padding: 0.875rem 1.25rem;
      border-radius: var(--radius-md);
      font-size: 0.92rem;
      line-height: 1.5;
    }
    .user-bubble {
      background-color: var(--primary);
      color: white;
      border-bottom-right-radius: 2px;
    }
    .ai-bubble {
      background-color: #f1f5f9;
      color: var(--text-primary);
      border-bottom-left-radius: 2px;
      border: 1px solid #e2e8f0;
    }
    .message-sender {
      font-size: 0.7rem;
      font-weight: 700;
      text-transform: uppercase;
      margin-bottom: 0.25rem;
      opacity: 0.8;
    }
    .message-time {
      font-size: 0.7rem;
      text-align: right;
      margin-top: 0.35rem;
      opacity: 0.7;
    }
    .chat-input-wrapper {
      padding: 1rem 1.5rem;
      border-top: 1px solid var(--border);
      background: white;
    }
    .chat-input-form {
      display: flex;
      gap: 0.75rem;
    }
    .chat-input {
      flex: 1;
    }
    .typing {
      display: flex;
      align-items: center;
      gap: 0.35rem;
      padding: 1rem 1.25rem;
    }
    .dot {
      width: 8px;
      height: 8px;
      background: var(--text-muted);
      border-radius: 50%;
      animation: pulse 1.4s infinite ease-in-out;
    }
    .dot:nth-child(2) { animation-delay: 0.2s; }
    .dot:nth-child(3) { animation-delay: 0.4s; }
    @keyframes pulse {
      0%, 80%, 100% { opacity: 0.3; transform: scale(0.8); }
      40% { opacity: 1; transform: scale(1.1); }
    }
  `]
})
export class AiChatComponent implements OnInit {
  private aiService = inject(AiChatService);

  @ViewChild('scrollContainer') private scrollContainer?: ElementRef;

  messages: ChatMessage[] = [];
  userPrompt = '';
  loading = false;

  suggestedQuestions = [
    'Can I deduct expenses for my home office?',
    'What is the difference between standard and itemized deductions?',
    'How do retirement contributions affect my taxable income?',
    'What records should I keep for charitable donations?'
  ];

  ngOnInit(): void {
    this.loadHistory();
  }

  loadHistory(): void {
    this.aiService.getHistory(1, 20).subscribe({
      next: (res) => {
        if (res.items && res.items.length > 0) {
          // Format chronological
          const loaded: ChatMessage[] = [];
          res.items.reverse().forEach((item: AiHistoryItem) => {
            loaded.push({ sender: 'user', text: item.userPrompt, timestamp: new Date(item.timestamp) });
            loaded.push({ sender: 'ai', text: item.aiResponse, timestamp: new Date(item.timestamp) });
          });
          this.messages = loaded;
          this.scrollToBottom();
        }
      },
      error: () => {
        // Soft fail if history empty
      }
    });
  }

  sendSuggestion(question: string): void {
    this.userPrompt = question;
    this.sendMessage();
  }

  sendMessage(): void {
    const prompt = this.userPrompt.trim();
    if (!prompt || this.loading) return;

    // Add user message to UI immediately
    this.messages.push({
      sender: 'user',
      text: prompt,
      timestamp: new Date()
    });

    this.userPrompt = '';
    this.loading = true;
    this.scrollToBottom();

    this.aiService.ask(prompt).subscribe({
      next: (res) => {
        this.messages.push({
          sender: 'ai',
          text: res.response,
          timestamp: new Date()
        });
        this.loading = false;
        this.scrollToBottom();
      },
      error: (err) => {
        this.messages.push({
          sender: 'ai',
          text: err.error?.detail || 'Unable to contact the AI assistant. Please ensure your Azure OpenAI credentials are configured.',
          timestamp: new Date()
        });
        this.loading = false;
        this.scrollToBottom();
      }
    });
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.scrollContainer) {
        this.scrollContainer.nativeElement.scrollTop = this.scrollContainer.nativeElement.scrollHeight;
      }
    }, 100);
  }
}
