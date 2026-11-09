import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TaxRecordService } from '../../core/services/tax-record.service';
import { CreateTaxRecordRequest, Deduction, TaxRecord } from '../../core/models/tax-record.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe],
  template: `
    <div class="dashboard-container">
      <div class="dashboard-header">
        <div>
          <h1>Tax Records Overview</h1>
          <p class="subtitle">Track your income, organize deductions, and review calculated liabilities</p>
        </div>
        <button (click)="openCreateModal()" class="btn btn-primary">
          <span>+</span> Add Tax Record
        </button>
      </div>

      <!-- Stat Cards -->
      <div class="stats-grid">
        <div class="card stat-card">
          <span class="stat-label">Tax Returns On File</span>
          <span class="stat-value">{{ records.length }}</span>
        </div>
        <div class="card stat-card">
          <span class="stat-label">Total Gross Income</span>
          <span class="stat-value">{{ totalGrossIncome | currency:'USD':'symbol':'1.0-0' }}</span>
        </div>
        <div class="card stat-card accent">
          <span class="stat-label">Total Claimed Deductions</span>
          <span class="stat-value">{{ totalDeductions | currency:'USD':'symbol':'1.0-0' }}</span>
        </div>
        <div class="card stat-card highlight">
          <span class="stat-label">Calculated Taxable Income</span>
          <span class="stat-value">{{ totalTaxableIncome | currency:'USD':'symbol':'1.0-0' }}</span>
        </div>
      </div>

      <!-- Table Card -->
      <div class="card table-card">
        <div class="card-header">
          <h3>Your Tax Years</h3>
          <span *ngIf="loading" class="text-muted">Loading records...</span>
        </div>

        <div *ngIf="errorMessage" class="alert alert-danger" style="margin: 1rem;">
          {{ errorMessage }}
        </div>

        <div *ngIf="!loading && records.length === 0" class="empty-state">
          <div class="empty-icon">📁</div>
          <h4>No tax records found</h4>
          <p>Get started by adding your first tax return.</p>
          <button (click)="openCreateModal()" class="btn btn-primary btn-sm">Add First Record</button>
        </div>

        <table *ngIf="records.length > 0" class="tax-table">
          <thead>
            <tr>
              <th>Tax Year</th>
              <th>Filing Status</th>
              <th>Gross Income</th>
              <th>Deductions</th>
              <th>Taxable Income</th>
              <th>Calculated Liability</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let record of records">
              <td><strong>{{ record.taxYear }}</strong></td>
              <td><span class="badge badge-status">{{ record.filingStatus }}</span></td>
              <td>{{ record.grossIncome | currency }}</td>
              <td>
                <span class="badge badge-deductions" (click)="toggleDeductions(record.id)">
                  {{ record.totalDeductions | currency }} ({{ record.deductions?.length || 0 }})
                </span>
                <!-- Expandable deduction list -->
                <div *ngIf="expandedRecordId === record.id && record.deductions?.length" class="deduction-dropdown">
                  <div *ngFor="let d of record.deductions" class="deduction-item">
                    <span>{{ d.category }}:</span>
                    <strong>{{ d.amount | currency }}</strong>
                    <small *ngIf="d.description">({{ d.description }})</small>
                  </div>
                </div>
              </td>
              <td><strong>{{ record.taxableIncome | currency }}</strong></td>
              <td><span class="liability-tag">{{ record.taxLiability | currency }}</span></td>
              <td>
                <button (click)="deleteRecord(record.id)" class="btn-icon danger" title="Delete">🗑️</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Create Tax Record Modal -->
      <div *ngIf="isModalOpen" class="modal-overlay">
        <div class="modal-card card">
          <div class="modal-header">
            <h3>Add New Tax Record</h3>
            <button (click)="closeModal()" class="btn-close">&times;</button>
          </div>

          <form (ngSubmit)="saveRecord()">
            <div class="modal-body">
              <div class="form-row">
                <div class="form-group">
                  <label class="form-label">Tax Year</label>
                  <input type="number" class="form-control" [(ngModel)]="formYear" name="taxYear" min="2000" max="2100" required />
                </div>
                <div class="form-group">
                  <label class="form-label">Filing Status</label>
                  <select class="form-control" [(ngModel)]="formFilingStatus" name="filingStatus">
                    <option value="Single">Single</option>
                    <option value="MarriedFilingJointly">Married Filing Jointly</option>
                    <option value="MarriedFilingSeparately">Married Filing Separately</option>
                    <option value="HeadOfHousehold">Head of Household</option>
                  </select>
                </div>
              </div>

              <div class="form-group">
                <label class="form-label">Gross Income ($)</label>
                <input type="number" class="form-control" [(ngModel)]="formGrossIncome" name="grossIncome" min="0" step="0.01" required />
              </div>

              <!-- Deductions Builder -->
              <div class="deductions-section">
                <div class="section-title-row">
                  <h4>Itemized Deductions</h4>
                  <button type="button" (click)="addDeductionRow()" class="btn btn-secondary btn-sm">+ Add Deduction</button>
                </div>

                <div *ngFor="let d of formDeductions; let i = index" class="deduction-row">
                  <select class="form-control" [(ngModel)]="d.category" name="cat_{{i}}">
                    <option value="Retirement">Retirement (401k/Super)</option>
                    <option value="Healthcare">Medical & Healthcare</option>
                    <option value="HomeOffice">Home Office</option>
                    <option value="Charitable">Charitable Donations</option>
                    <option value="Education">Education / Student Loan</option>
                    <option value="Other">Other Eligible</option>
                  </select>
                  <input type="text" class="form-control" [(ngModel)]="d.description" name="desc_{{i}}" placeholder="Description (optional)" />
                  <input type="number" class="form-control amount-input" [(ngModel)]="d.amount" name="amount_{{i}}" placeholder="Amount ($)" min="0" step="0.01" />
                  <button type="button" (click)="removeDeductionRow(i)" class="btn-icon danger">✕</button>
                </div>

                <p *ngIf="formDeductions.length === 0" class="text-muted small">No deductions added yet. Click "+ Add Deduction" above.</p>
              </div>

              <!-- Live preview summary -->
              <div class="calc-preview">
                <div>Total Deductions: <strong>{{ previewDeductions() | currency }}</strong></div>
                <div>Estimated Taxable Income: <strong>{{ previewTaxableIncome() | currency }}</strong></div>
              </div>
            </div>

            <div class="modal-footer">
              <button type="button" (click)="closeModal()" class="btn btn-secondary">Cancel</button>
              <button type="submit" class="btn btn-primary" [disabled]="saving">
                {{ saving ? 'Saving...' : 'Save Tax Record' }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      max-width: 1200px;
      margin: 2rem auto;
      padding: 0 1.5rem;
    }
    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }
    .dashboard-header h1 {
      font-size: 1.875rem;
      color: var(--primary);
    }
    .subtitle {
      color: var(--text-secondary);
      font-size: 0.95rem;
    }
    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: 1.25rem;
      margin-bottom: 2rem;
    }
    .stat-card {
      padding: 1.25rem 1.5rem;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
    .stat-card.accent {
      border-left: 4px solid var(--accent);
    }
    .stat-card.highlight {
      border-left: 4px solid var(--border-focus);
    }
    .stat-label {
      font-size: 0.8rem;
      text-transform: uppercase;
      font-weight: 600;
      color: var(--text-muted);
      letter-spacing: 0.05em;
    }
    .stat-value {
      font-size: 1.75rem;
      font-weight: 700;
      color: var(--text-primary);
    }
    .table-card {
      overflow: hidden;
    }
    .card-header {
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid var(--border);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .tax-table {
      width: 100%;
      border-collapse: collapse;
      text-align: left;
    }
    .tax-table th {
      background: #f8fafc;
      padding: 0.875rem 1.5rem;
      font-size: 0.8rem;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-muted);
      font-weight: 600;
      border-bottom: 1px solid var(--border);
    }
    .tax-table td {
      padding: 1rem 1.5rem;
      border-bottom: 1px solid var(--border);
      font-size: 0.95rem;
      position: relative;
    }
    .tax-table tr:hover {
      background-color: #fcfdfe;
    }
    .badge {
      display: inline-block;
      padding: 0.25rem 0.625rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 600;
    }
    .badge-status {
      background-color: var(--primary-light);
      color: var(--primary);
    }
    .badge-deductions {
      background-color: var(--accent-light);
      color: var(--accent-dark);
      cursor: pointer;
    }
    .badge-deductions:hover {
      text-decoration: underline;
    }
    .liability-tag {
      font-weight: 600;
      color: var(--text-primary);
    }
    .btn-icon {
      background: none;
      border: none;
      font-size: 1rem;
      cursor: pointer;
      padding: 0.25rem;
      border-radius: var(--radius-sm);
    }
    .btn-icon:hover {
      background: var(--danger-light);
    }
    .deduction-dropdown {
      position: absolute;
      top: 3rem;
      left: 1.5rem;
      background: white;
      border: 1px solid var(--border);
      border-radius: var(--radius-sm);
      box-shadow: var(--shadow-md);
      padding: 0.75rem;
      z-index: 10;
      min-width: 220px;
    }
    .deduction-item {
      font-size: 0.8rem;
      padding: 0.25rem 0;
      display: flex;
      justify-content: space-between;
      gap: 1rem;
    }
    .empty-state {
      padding: 4rem 2rem;
      text-align: center;
    }
    .empty-icon {
      font-size: 3rem;
      margin-bottom: 1rem;
    }
    .empty-state h4 {
      margin-bottom: 0.5rem;
      color: var(--text-primary);
    }
    .empty-state p {
      color: var(--text-secondary);
      margin-bottom: 1.5rem;
    }

    /* Modal */
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(15, 23, 42, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 200;
      padding: 1rem;
    }
    .modal-card {
      width: 100%;
      max-width: 600px;
      max-height: 90vh;
      display: flex;
      flex-direction: column;
    }
    .modal-header {
      padding: 1.25rem 1.5rem;
      border-bottom: 1px solid var(--border);
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .btn-close {
      background: none;
      border: none;
      font-size: 1.5rem;
      color: var(--text-muted);
      cursor: pointer;
    }
    .modal-body {
      padding: 1.5rem;
      overflow-y: auto;
    }
    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }
    .deductions-section {
      margin-top: 1.5rem;
      border-top: 1px solid var(--border);
      padding-top: 1rem;
    }
    .section-title-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1rem;
    }
    .deduction-row {
      display: grid;
      grid-template-columns: 1.5fr 2fr 1fr auto;
      gap: 0.5rem;
      margin-bottom: 0.75rem;
      align-items: center;
    }
    .amount-input {
      text-align: right;
    }
    .calc-preview {
      margin-top: 1.5rem;
      padding: 1rem;
      background: var(--primary-light);
      border-radius: var(--radius-sm);
      display: flex;
      justify-content: space-between;
      font-size: 0.9rem;
    }
    .modal-footer {
      padding: 1rem 1.5rem;
      border-top: 1px solid var(--border);
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
    }
  `]
})
export class DashboardComponent implements OnInit {
  private taxService = inject(TaxRecordService);

  records: TaxRecord[] = [];
  loading = false;
  saving = false;
  errorMessage = '';
  expandedRecordId: string | null = null;

  // Modal State
  isModalOpen = false;
  formYear = 2025;
  formGrossIncome = 85000;
  formFilingStatus = 'Single';
  formDeductions: Deduction[] = [];

  ngOnInit(): void {
    this.loadRecords();
  }

  loadRecords(): void {
    this.loading = true;
    this.taxService.getAll().subscribe({
      next: (res) => {
        this.records = res;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = 'Failed to load tax records.';
      }
    });
  }

  get totalGrossIncome(): number {
    return this.records.reduce((acc, r) => acc + (r.grossIncome || 0), 0);
  }

  get totalDeductions(): number {
    return this.records.reduce((acc, r) => acc + (r.totalDeductions || 0), 0);
  }

  get totalTaxableIncome(): number {
    return this.records.reduce((acc, r) => acc + (r.taxableIncome || 0), 0);
  }

  openCreateModal(): void {
    this.formYear = 2025;
    this.formGrossIncome = 85000;
    this.formFilingStatus = 'Single';
    this.formDeductions = [
      { category: 'Retirement', description: '401k / Superannuation', amount: 6000 }
    ];
    this.isModalOpen = true;
  }

  closeModal(): void {
    this.isModalOpen = false;
  }

  addDeductionRow(): void {
    this.formDeductions.push({
      category: 'HomeOffice',
      description: '',
      amount: 1000
    });
  }

  removeDeductionRow(index: number): void {
    this.formDeductions.splice(index, 1);
  }

  previewDeductions(): number {
    return this.formDeductions.reduce((sum, d) => sum + (Number(d.amount) || 0), 0);
  }

  previewTaxableIncome(): number {
    return Math.max(0, (this.formGrossIncome || 0) - this.previewDeductions());
  }

  saveRecord(): void {
    this.saving = true;
    const payload: CreateTaxRecordRequest = {
      taxYear: this.formYear,
      grossIncome: this.formGrossIncome,
      filingStatus: this.formFilingStatus,
      deductions: this.formDeductions.map(d => ({
        category: d.category,
        description: d.description || '',
        amount: Number(d.amount) || 0
      }))
    };

    this.taxService.create(payload).subscribe({
      next: () => {
        this.saving = false;
        this.closeModal();
        this.loadRecords();
      },
      error: (err) => {
        this.saving = false;
        alert(err.error?.detail || 'Failed to save tax record.');
      }
    });
  }

  deleteRecord(id: string): void {
    if (confirm('Are you sure you want to delete this tax record?')) {
      this.taxService.delete(id).subscribe({
        next: () => this.loadRecords(),
        error: () => alert('Failed to delete record.')
      });
    }
  }

  toggleDeductions(recordId: string): void {
    this.expandedRecordId = this.expandedRecordId === recordId ? null : recordId;
  }
}
