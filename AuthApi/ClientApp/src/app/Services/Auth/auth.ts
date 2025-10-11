import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../Models/Api-response.model';
import { LoginResponse } from '../../Models/Login-response.model';
import { JwtHelperService } from '@auth0/angular-jwt';
@Injectable({
  providedIn: 'root'
})
export class Auth {

  private MainUrl = 'https://localhost:7258/api'; // Main URL of the .NET API
  private AuthUrl = this.MainUrl + '/Auth';
  private UserUrl = this.MainUrl + '/users';
  private LoginUrl = this.AuthUrl + '/Login';
  private RefreshTokenUrl = this.AuthUrl + '/ValidateRefreshToken';
  constructor(private http: HttpClient) {

  }


  private jwtHelper = new JwtHelperService();

  getRole(): string | null {
    const token = this.getToken();
    if (!token) return null;
    const decodedToken = this.jwtHelper.decodeToken(token);
    var role = decodedToken['role'] || decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || null;
    return role;

  }
  getName(): string | null {
    const token = this.getToken();
    if (!token) return null;
    const decodedToken = this.jwtHelper.decodeToken(token);
    var name = decodedToken['name'] || decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || null;
    return name;
  }

  isAuthenticated(): boolean {
    const token = this.getToken();

    return token != null && !this.jwtHelper.isTokenExpired(token);
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }

  isUser(): boolean {
    return this.getRole() === 'User';
  }


  //  Login method
  login(credentials: { UserName: string, Password: string }): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.LoginUrl}`, credentials);
  }


  refreshToken() {
    const token = localStorage.getItem('refreshToken');
    if (!token) return null;
    return this.http.post<ApiResponse<LoginResponse>>(`${this.RefreshTokenUrl}`, {
      token
    });
  }

  // Register method 
  register(data: any): Observable<any> {
    return this.http.post(`${this.UserUrl}/UserRegister`, data);
  }




  //  Logout 
  logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
  }

  //  Save token
  saveToken(token: string) {
    localStorage.setItem('token', token);
  }

  saveRefreshToken(token: string) {
    localStorage.setItem('refreshToken', token);
  }

  //  Get token
  getToken(): string | null {
    return localStorage.getItem('token');
  }

  //  Get refresh token
  getRefreshToken(): string | null {
    return localStorage.getItem('refreshToken');
  }

}
