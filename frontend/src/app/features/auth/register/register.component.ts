import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="auth-wrapper">
      <div class="auth-card card">
        <div class="auth-header">
          <h2>Create an Account</h2>
          <p>Start organizing your tax records and getting AI insights</p>
        </div>

        <div *ngIf="errorMessage" class="alert alert-danger">
          {{ errorMessage }}
        </div>

        <div *ngIf="successMessage" class="alert alert-success">
          {{ successMessage }}
        </div>

        <form (ngSubmit)="onSubmit()" #registerForm="ngForm">
          <div class="form-group">
            <label class="form-label" for="fullName">Full Name</label>
            <input
              type="text"
              id="fullName"
              name="fullName"
              class="form-control"
              [(ngModel)]="fullName"
              required
              minlength="2"
              placeholder="e.g. Asha Nair"
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="email">Email Address</label>
            <input
              type="email"
              id="email"
              name="email"
              class="form-control"
              [(ngModel)]="email"
              required
              email
              placeholder="asha@example.com"
            />
          </div>

          <div class="form-group">
            <label class="form-label" for="password">Password (min 8 characters)</label>
            <input
              type="password"
              id="password"
              name="password"
              class="form-control"
              [(ngModel)]="password"
              required
              minlength="8"
              placeholder="••••••••"
            />
          </div>

          <button type="submit" class="btn btn-primary btn-block" [disabled]="loading || !registerForm.form.valid">
            <span *ngIf="loading">Creating account...</span>
            <span *ngIf="!loading">Create Account</span>
          </button>
        </form>

        <div class="auth-footer">
          Already have an account? <a routerLink="/login">Sign In</a>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-wrapper {
      min-height: calc(100vh - 70px);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem 1rem;
    }
    .auth-card {
      width: 100%;
      max-width: 440px;
      padding: 2.5rem;
    }
    .auth-header {
      text-align: center;
      margin-bottom: 2rem;
    }
    .auth-header h2 {
      font-size: 1.75rem;
      font-weight: 700;
      color: var(--primary);
      margin-bottom: 0.5rem;
    }
    .auth-header p {
      color: var(--text-secondary);
      font-size: 0.9rem;
    }
    .btn-block {
      width: 100%;
      padding: 0.75rem;
      font-size: 1rem;
      margin-top: 1rem;
    }
    .auth-footer {
      text-align: center;
      margin-top: 1.5rem;
      font-size: 0.875rem;
      color: var(--text-secondary);
    }
  `]
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  fullName = '';
  email = '';
  password = '';
  errorMessage = '';
  successMessage = '';
  loading = false;

  onSubmit(): void {
    if (!this.fullName || !this.email || !this.password) return;
    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.auth.register({
      fullName: this.fullName,
      email: this.email,
      password: this.password
    }).subscribe({
      next: () => {
        this.loading = false;
        this.successMessage = 'Account created successfully! Logging you in...';
        // Auto-login after registration
        setTimeout(() => {
          this.auth.login({ email: this.email, password: this.password }).subscribe({
            next: () => this.router.navigate(['/dashboard']),
            error: () => this.router.navigate(['/login'])
          });
        }, 1200);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error?.detail || err.error?.title || 'Registration failed. Email might already be taken.';
      }
    });
  }
}
