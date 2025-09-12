//cREATE Welcome component for logged users
import { Component } from "@angular/core";
import { AuthService } from "../services/auth.service";
@Component({
    selector: 'app-welcome',
    template: `
    <h1>Welcome to Steam Tools!</h1>
    <p>You are successfully logged in.</p>
    <button (click)="logout()">Logout</button>
    `})
export class WelcomeComponent {
    constructor(private authService: AuthService) {}

    logout() {
        this.authService.logout();
        window.location.reload();
    }
}