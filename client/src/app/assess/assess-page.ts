import { Component, computed, inject, signal } from '@angular/core';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApplicantProfile, AssessmentResult, CreditApi } from '../core/credit-api';

interface FieldConfig {
  key: keyof ApplicantProfile;
  label: string;
  hint?: string;
  min?: number;
  max?: number;
  step?: number;
}

@Component({
  selector: 'app-assess-page',
  imports: [ReactiveFormsModule, CurrencyPipe, DecimalPipe],
  templateUrl: './assess-page.html',
  styleUrl: './assess-page.css'
})
export class AssessPage {
  private readonly api = inject(CreditApi);
  private readonly fb = inject(NonNullableFormBuilder);

  readonly foirLimit = 50;

  readonly form = this.fb.group({
    name:                ['', Validators.required],
    monthlyIncome:       [0,  [Validators.required, Validators.min(1)]],
    existingEmis:        [0,  [Validators.required, Validators.min(0)]],
    requestedLoanAmount: [0,  [Validators.required, Validators.min(1)]],
    tenureMonths:        [36, [Validators.required, Validators.min(1), Validators.max(120)]],
    annualInterestRate:  [12, [Validators.required, Validators.min(0), Validators.max(40)]],
    cibilScore:          [750, [Validators.required, Validators.min(300), Validators.max(900)]],
    employmentType:      ['Salaried', Validators.required],
    yearsInCurrentJob:   [1,  [Validators.required, Validators.min(0), Validators.max(50)]]
  });

  readonly numberFields: FieldConfig[] = [
    { key: 'monthlyIncome',       label: 'Monthly income (₹)',     min: 1 },
    { key: 'existingEmis',        label: 'Existing EMIs (₹/month)', min: 0 },
    { key: 'requestedLoanAmount', label: 'Loan amount (₹)',        min: 1 },
    { key: 'tenureMonths',        label: 'Tenure (months)',        min: 1, max: 120 },
    { key: 'annualInterestRate',  label: 'Interest rate (% p.a.)', min: 0, max: 40, step: 0.1 },
    { key: 'cibilScore',          label: 'CIBIL score',            min: 300, max: 900, hint: '300–900' },
    { key: 'yearsInCurrentJob',   label: 'Years in current job',   min: 0, max: 50 }
  ];

  readonly loading = signal(false);
  readonly result = signal<AssessmentResult | null>(null);
  readonly error = signal<string | null>(null);

  // computed = derived values that update automatically when result() changes
  readonly riskClass = computed(() => this.result()?.assessment.riskLevel.toLowerCase() ?? '');
  readonly foirBarWidth = computed(() => Math.min(this.result()?.foirPercent ?? 0, 100));
  readonly foirOverLimit = computed(() => (this.result()?.foirPercent ?? 0) > this.foirLimit);

  isInvalid(key: string): boolean {
    const control = this.form.get(key);
    return !!control && control.invalid && (control.touched || control.dirty);
  }

  loadSample(kind: 'strong' | 'risky'): void {
    const samples: Record<'strong' | 'risky', ApplicantProfile> = {
      strong: {
        name: 'Priya Sharma', monthlyIncome: 120000, existingEmis: 10000, requestedLoanAmount: 500000,
        tenureMonths: 36, annualInterestRate: 12, cibilScore: 790, employmentType: 'Salaried', yearsInCurrentJob: 5
      },
      risky: {
        name: 'Arun Kumar', monthlyIncome: 45000, existingEmis: 18000, requestedLoanAmount: 400000,
        tenureMonths: 24, annualInterestRate: 14, cibilScore: 640, employmentType: 'Self-employed', yearsInCurrentJob: 1
      }
    };
    this.form.setValue(samples[kind]);
    this.result.set(null);
    this.error.set(null);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();   // shows all validation messages
      return;
    }

    this.loading.set(true);
    this.error.set(null);
    this.result.set(null);

    this.api.assess(this.form.getRawValue()).subscribe({
      next: res => {
        this.result.set(res);
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        // 400 → { error: "..." }   502 → ProblemDetails { detail: "..." }   0 → API not running
        this.error.set(
          err.status === 0
            ? 'Could not reach the API. Is it running?'
            : err.error?.error ?? err.error?.detail ?? `Request failed (${err.status}).`
        );
        this.loading.set(false);
      }
    });
  }
}