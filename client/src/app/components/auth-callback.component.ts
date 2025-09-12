import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { AuthService } from "../services/auth.service";

// auth-callback.component.ts
@Component({
  selector: 'app-auth-callback',
  template: '<p>Logging in...</p>'
})
export class AuthCallbackComponent implements OnInit {
  constructor(private route: ActivatedRoute, private authService: AuthService, private router: Router) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      if (token) {
        this.authService.handleCallback(token);
        this.router.navigate(['/welcome']); // redirect to home
      }
    });
  }
}
