import { Component, OnInit } from "@angular/core";
import { AuthService } from "../services/auth.service";

@Component({
    selector: 'app-welcome',
    template: `
    <div class="min-h-screen bg-gradient-to-br from-blue-500 to-purple-600 flex items-center justify-center p-4">
      <div class="bg-white rounded-lg shadow-xl p-8 max-w-md w-full text-center">
        <div class="mb-6">
          <h1 class="text-3xl font-bold text-gray-800 mb-2">Welcome to Steam Tools!</h1>
          <p class="text-gray-600">You are successfully logged in.</p>
        </div>
        <div class="mb-6">
          <p class="text-sm text-gray-500 mb-1">Your Steam ID:</p>
          <p class="text-lg font-mono bg-gray-100 rounded px-3 py-2">{{ steamId || 'Loading...' }}</p>
        </div>
        <button
          (click)="logout()"
          class="w-full bg-red-500 hover:bg-red-600 text-white font-semibold py-2 px-4 rounded-lg transition duration-200">
          Logout
        </button>
      </div>
    </div>
    `})
export class WelcomeComponent implements OnInit {
    steamId: string | null = null;

    constructor(private authService: AuthService) {}

    ngOnInit() {
        this.authService.getSteamIdFromServer().subscribe({
            next: (data) => {
                this.steamId = data.steamId;
            },
            error: (err) => {
                console.error('Error fetching Steam ID:', err);
                this.steamId = 'Error loading Steam ID';
            }
        });
    }

    logout() {
        this.authService.logout();
        window.location.reload();
    }
}
