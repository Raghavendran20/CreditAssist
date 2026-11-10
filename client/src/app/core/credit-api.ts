import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

// ---------- Types (match the C# records) ----------

export interface AgentAnswer {
  answer: string;
  toolsUsed: string[];
}

export interface ApplicantProfile {
  name: string;
  monthlyIncome: number;
  existingEmis: number;
  requestedLoanAmount: number;
  tenureMonths: number;
  annualInterestRate: number;
  cibilScore: number;
  employmentType: string;
  yearsInCurrentJob: number;
}

export type RiskLevel = 'Low' | 'Medium' | 'High';

export interface LoanAssessment {
  riskLevel: RiskLevel;
  strengths: string[];
  redFlags: string[];
  summary: string;
}

export interface AssessmentResult {
  newEmi: number;
  foirPercent: number;
  assessment: LoanAssessment;
}

// ---------- Service ----------

@Injectable({ providedIn: 'root' })
export class CreditApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5226/api/ai';

  /** Agent mode: AI can call C# tools. */
  askWithTools(question: string): Observable<AgentAnswer> {
    return this.http.post<AgentAnswer>(`${this.baseUrl}/agent`, { question });
  }

  /** Structured loan assessment (Day 3 endpoint). */
  assess(applicant: ApplicantProfile): Observable<AssessmentResult> {
    return this.http.post<AssessmentResult>(`${this.baseUrl}/assess`, applicant);
  }

  /** Streaming answer using Server-Sent Events. */
  streamAnswer(question: string): Observable<string> {
    return new Observable<string>(subscriber => {
      const url = `${this.baseUrl}/ask/stream?question=${encodeURIComponent(question)}`;
      const source = new EventSource(url);

      source.onmessage = event => subscriber.next(event.data);
      source.onerror = () => {
        source.close();
        subscriber.complete();
      };

      return () => source.close();
    });
  }
}