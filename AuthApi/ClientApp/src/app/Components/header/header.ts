import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Auth } from '../../Services/Auth/auth';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-header',
  imports: [RouterLink, CommonModule],
  templateUrl: './header.html',
  styleUrl: './header.css'
})
export class Header {
  constructor(public auth: Auth) { }
}
