import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HttpClientModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('ClientApp');

  _httpclint = inject(HttpClient);

  ngOnInit(): void {
    this._httpclint.get('/api/Roles/GetAllRoles').subscribe(data => {
      console.log(data);
    });
  }

}
