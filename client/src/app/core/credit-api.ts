import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface AgentAnswer {
  answer: string;
  toolsUsed: string[];
}

@Injectable({ providedIn: 'root' })
export class CreditApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5226/api/ai';

  /** Agent mode: AI can call C# tools. Returns the answer + which tools were used. */
  askWithTools(question: string): Observable<AgentAnswer> {
    return this.http.post<AgentAnswer>(`${this.baseUrl}/agent`, { question });
  }

  /** Streaming mode: emits the answer piece by piece using Server-Sent Events. */
  streamAnswer(question: string): Observable<string> {
    return new Observable<string>(subscriber => {
      const url = `${this.baseUrl}/ask/stream?question=${encodeURIComponent(question)}`;
      const source = new EventSource(url);

      source.onmessage = event => subscriber.next(event.data);

      // The server closes the connection when the answer is complete.
      // EventSource reports that as an "error" and would reconnect, so we close it.
      source.onerror = () => {
        source.close();
        subscriber.complete();
      };

      return () => source.close();   // runs if the component unsubscribes
    });
  }
}