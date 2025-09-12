import { Routes } from '@angular/router';
import { WelcomeComponent } from './components/welcome.component';
import { LoginComponent } from './components/login.component';
import { AuthGuard } from './guards/auth.guard';

export const routes: Routes = [
    { path: 'login', component: LoginComponent},
    { path: 'welcome', component: WelcomeComponent, canActivate: [AuthGuard] },
    { path: 'auth/callback', loadComponent: () => import('./components/auth-callback.component').then(m => m.AuthCallbackComponent) },
    { path: '**', redirectTo: 'welcome' }
];
