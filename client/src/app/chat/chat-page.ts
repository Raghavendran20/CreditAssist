import { Component, ElementRef, effect, inject, signal, viewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CreditApi } from '../core/credit-api';

interface ChatMessage {
  role: 'user' | 'assistant';
  text: string;
  tools?: string[];
  error?: boolean;
}

@Component({
  selector: 'app-chat-page',
  imports: [FormsModule],
  templateUrl: './chat-page.html',
  styleUrl: './chat-page.css'
})
export class ChatPage {
  private readonly api = inject(CreditApi);
  private readonly scroller = viewChild<ElementRef<HTMLDivElement>>('scroller');

  readonly messages = signal<ChatMessage[]>([]);
  readonly question = signal('');
  readonly agentMode = signal(false);
  readonly busy = signal(false);

  readonly examples = [
    'What is FOIR and what is a safe limit?',
    'Explain CIBIL score in simple words',
    'Can the applicant with PAN ABCDE1234F afford a ₹5,00,000 loan for 36 months at 12%?',
    'What is the EMI for ₹10,00,000 at 10.5% for 60 months?'
  ];

  constructor() {
    // Auto-scroll to the newest message whenever messages change
    effect(() => {
      this.messages();
      queueMicrotask(() => {
        const el = this.scroller()?.nativeElement;
        if (el) el.scrollTop = el.scrollHeight;
      });
    });
  }

  useExample(text: string): void {
    this.question.set(text);
    // Questions about PANs or EMI amounts need tools
    this.agentMode.set(/PAN|EMI for/i.test(text));
  }

  clear(): void {
    this.messages.set([]);
  }

  send(): void {
    const q = this.question().trim();
    if (!q || this.busy()) return;

    this.question.set('');
    this.addMessage({ role: 'user', text: q });
    this.busy.set(true);

    if (this.agentMode()) {
      this.sendToAgent(q);
    } else {
      this.streamAnswer(q);
    }
  }

  private sendToAgent(q: string): void {
    this.api.askWithTools(q).subscribe({
      next: res => this.addMessage({ role: 'assistant', text: res.answer, tools: res.toolsUsed }),
      error: () => {
        this.addMessage({ role: 'assistant', text: 'Could not reach the API. Is it running?', error: true });
        this.busy.set(false);
      },
      complete: () => this.busy.set(false)
    });
  }

  private streamAnswer(q: string): void {
    this.addMessage({ role: 'assistant', text: '' });   // empty bubble that fills up

    this.api.streamAnswer(q).subscribe({
      next: chunk => this.updateLast(m => ({ ...m, text: m.text + chunk })),
      complete: () => {
        this.updateLast(m => m.text
          ? m
          : { ...m, text: 'No answer received. Is the API running?', error: true });
        this.busy.set(false);
      }
    });
  }

  private addMessage(message: ChatMessage): void {
    this.messages.update(list => [...list, message]);
  }

  private updateLast(change: (m: ChatMessage) => ChatMessage): void {
    this.messages.update(list => {
      const copy = [...list];
      copy[copy.length - 1] = change(copy[copy.length - 1]);
      return copy;
    });
  }
}