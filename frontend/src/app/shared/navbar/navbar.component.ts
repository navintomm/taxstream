import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <header class="navbar">
      <div class="navbar-container">
        <a routerLink="/" class="navbar-brand">
          <span class="brand-icon">📈</span>
          <span class="brand-name">TaxStream<strong>AI</strong></span>
        </a>

        <nav class="navbar-nav" *ngIf="auth.isAuthenticated()">
          <a routerLink="/dashboard" routerLinkActive="active" class="nav-link">Dashboard</a>
          <a routerLink="/ai-chat" routerLinkActive="active" class="nav-link">AI Tax Assistant</a>
        </nav>

        <div class="navbar-actions">
          <ng-container *ngIf="auth.isAuthenticated(); else guestBlock">
            <span class="user-greeting">Welcome, <strong>{{ auth.currentUser()?.fullName || 'User' }}</strong></span>
            <button (click)="auth.logout()" class="btn btn-secondary btn-sm">Sign Out</button>
          </ng-container>
          <ng-template #guestBlock>
            <a routerLink="/login" class="btn btn-secondary btn-sm">Sign In</a>
            <a routerLink="/register" class="btn btn-primary btn-sm">Register</a>
          </ng-template>
        </div>
      </div>
    </header>
  `,
  styles: [`
    .navbar {
      background: #ffffff;
      border-bottom: 1px solid var(--border);
      position: sticky;
      top: 0;
      z-index: 100;
      box-shadow: var(--shadow-sm);
    }
    .navbar-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 0.75rem 1.5rem;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    .navbar-brand {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--primary);
      text-decoration: none;
    }
    .brand-icon {
      font-size: 1.5rem;
    }
    .brand-name strong {
      color: var(--accent);
    }
    .navbar-nav {
      display: flex;
      gap: 1.5rem;
    }
    .nav-link {
      font-size: 0.95rem;
      font-weight: 500;
      color: var(--text-secondary);
      padding: 0.5rem 0;
      position: relative;
      text-decoration: none;
      transition: color 0.15s ease;
    }
    .nav-link:hover, .nav-link.active {
      color: var(--primary);
      font-weight: 600;
    }
    .nav-link.active::after {
      content: '';
      position: absolute;
      bottom: -0.75rem;
      left: 0;
      right: 0;
      height: 2px;
      background: var(--primary);
    }
    .navbar-actions {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .user-greeting {
      font-size: 0.875rem;
      color: var(--text-secondary);
    }
  `]
})
export class NavbarComponent {
  auth = inject(AuthService);
}
