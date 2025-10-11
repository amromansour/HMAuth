import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../Models/Api-response.model';
import { Injectable } from '@angular/core';
import { UsersResponse } from '../../Models/Users-response.model';

@Injectable({
  providedIn: 'root'
})
export class UsersService {

  private MainUrl = 'https://localhost:7258/api'; // Main URL of the .NET API
  private UserUrl = this.MainUrl + '/users';
  private GetAllUsersUrl = this.UserUrl + '/GetAll';
  constructor(private http: HttpClient) {

  }

  //  Login method
  getAllUsers(PageDto: { PageIndex: string, PageSize: string }): Observable<ApiResponse<UsersResponse[]>> {
    return this.http.get<ApiResponse<UsersResponse[]>>(`${this.GetAllUsersUrl}`, { params: PageDto });
  }
}
