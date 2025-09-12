import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';

@Component({
selector: 'app-login-button',
template:  `
<div class="min-h-screen bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center p-4">
  <div class="bg-white rounded-lg shadow-xl p-8 max-w-md w-full text-center">
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-800 mb-2">Welcome to Steam Tools</h1>
      <p class="text-gray-600">Login with your Steam account to get started.</p>
    </div>
    <button
      (click)="login()"
      [disabled]="isLoading"
      class="w-full bg-green-600 hover:bg-green-700 disabled:bg-gray-400 text-white font-semibold py-3 px-4 rounded-lg shadow-md transition duration-200 flex items-center justify-center"
    >
      @if (isLoading) {
        <svg class="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" 
            d="M4 12a8 8 0 018-8V0C5.373 0 
               0 5.373 0 12h4zm2 5.291A7.962 
               7.962 0 014 12H0c0 3.042 
               1.135 5.824 3 7.938l3-2.647z">
          </path>
        </svg>
      }
      {{ isLoading ? 'Logging in...' : 'Login with Steam' }}
    </button>
  </div>
</div>
`
})
export class LoginComponent {
  isLoading = false;

  constructor(private authService: AuthService) {}

  login() {
    this.isLoading = true;
    this.authService.login(); // Redirects to backend Steam login
  }
}
