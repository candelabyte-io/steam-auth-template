import { Injectable } from "@angular/core";
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private backendUrl = 'http://localhost:5027/auth/steam';

  constructor(private http: HttpClient) {}

  login() {
    window.location.href = `${this.backendUrl}/login`;
  }

  handleCallback(token: string) {
    localStorage.setItem('jwt', token);
  }

  getToken(): string | null {
    return localStorage.getItem('jwt');
  }

  logout() {
    localStorage.removeItem('jwt');
  }

  isLoggedIn(): boolean {
    const token = this.getToken();

    if (!token) return false;

    const payload = JSON.parse(atob(token.split('.')[1]));
    const exp = payload.exp;
    return Date.now() < exp * 1000;
  }

  getSteamId(): Observable<any> {
    return this.http.get(`${this.backendUrl}/me`);
  }
}
