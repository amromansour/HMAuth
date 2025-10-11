import { HttpClient } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Auth } from '../../Services/Auth/auth';
import { ApiResponse } from '../../Models/Api-response.model';
import { LoginResponse } from '../../Models/Login-response.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  standalone: true,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  constructor(private authService: Auth) { }
  errorMessage: string = '';
  loading: boolean = false;
  loginobj = {
    username: '',
    password: ''
  }
  router = inject(Router);
  login() {
    this.loading = true;
    this.errorMessage = '';

    this.authService.login({ UserName: this.loginobj.username, Password: this.loginobj.password })
      .subscribe({
        next: (response: ApiResponse<LoginResponse>) => {
          this.loading = false;

          if (response._ResponseCode === 200 && response.data?.accessToken) {
            const { accessToken, refreshToken, userName } = response.data;

            this.authService.saveToken(accessToken);
            this.authService.saveRefreshToken(refreshToken);

            alert(`Welcome, ${userName}!`);
            // أو تقدر تستخدم:
            this.router.navigate(['/']);
          } else {
            this.errorMessage = response.message || 'Login failed';
          }
        },
        error: (err) => {
          this.loading = false;
          console.error('Login error:', err);

          if (err.status === 0) {
            this.errorMessage = 'Cannot connect to server. Please try again later.';
          } else {
            this.errorMessage = err.error?.message || 'Invalid credentials';
          }
        }
      });
  }

}


