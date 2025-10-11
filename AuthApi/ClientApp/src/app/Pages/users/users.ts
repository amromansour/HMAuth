
import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { UsersService } from '../../Services/Users/users-service';
import { UsersResponse } from '../../Models/Users-response.model';
@Component({
  selector: 'app-users',
  imports: [CommonModule],
  templateUrl: './users.html',
  styleUrl: './users.css'
})
export class Users {

  constructor(public usersService: UsersService) { }
  Users: UsersResponse[] = [];
  ngOnInit(): void {
    this.usersService.getAllUsers({ PageIndex: '1', PageSize: '10' }).subscribe(response => {
      this.Users = response.data ?? [];
      console.log(this.Users);
    });
  }

}
