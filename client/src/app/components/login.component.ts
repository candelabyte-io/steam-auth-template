import { Component } from '@angular/core';
import { AuthService } from '../services/auth.service';

@Component({
	selector: 'app-login-button',
	template: `
		<button
			(click)="login()"
			class="bg-green-600 hover:bg-green-700 text-white font-semibold py-2 px-4 rounded-lg shadow-md transition duration-200"
		>
			Login with Steam
		</button>
	`
})
export class LoginComponent {
  constructor(private authService: AuthService) {}

  login() {
    this.authService.login(); // Redirects to backend Steam login
  }
}
