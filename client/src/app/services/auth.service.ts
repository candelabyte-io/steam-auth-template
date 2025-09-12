import { Injectable } from "@angular/core";
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private backendUrl = 'http://localhost:5027/auth/steam';

  constructor(private http: HttpClient) {}

  login() {
    // Redirect user to backend Steam login
    window.location.href = `${this.backendUrl}/login`;
  }

  handleCallback(token: string) {
    // Save token in localStorage (or sessionStorage)
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
    console.log('Token:', token);

    if (!token) return false;

    const payload = JSON.parse(atob(token.split('.')[1]));
    const exp = payload.exp;
    return Date.now() < exp * 1000;
  }

  getSteamId(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.steamid || null;
    } catch (error) {
      console.error('Error decoding token:', error);
      return null;
    }
  }

  getSteamIdFromServer(): Observable<any> {
    return this.http.get(`${this.backendUrl}/me`);
  }
}
