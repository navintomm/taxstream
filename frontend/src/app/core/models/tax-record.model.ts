export interface Deduction {
  id?: string;
  category: string;
  description?: string;
  amount: number;
}

export interface TaxRecord {
  id: string;
  taxYear: number;
  grossIncome: number;
  totalDeductions: number;
  taxableIncome: number;
  taxLiability: number;
  filingStatus: string;
  createdAt: string;
  updatedAt: string;
  deductions: Deduction[];
}

export interface CreateTaxRecordRequest {
  taxYear: number;
  grossIncome: number;
  filingStatus: string;
  deductions: Deduction[];
}

export interface UpdateTaxRecordRequest {
  taxYear: number;
  grossIncome: number;
  filingStatus: string;
  deductions: Deduction[];
}
