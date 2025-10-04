import { Routes } from '@angular/router';

export const routes: Routes = [


    {
        path: '',
        pathMatch: 'full',
        loadComponent() {
            return import('./home/home').then(c => c.Home);
        }

    },
    {
        path: 'Login',
        // component:NewsDetailes
        loadComponent() {
            return import('./Pages/login/login').then(c => c.Login);
        }
    },
    {
        path: 'Register',
        loadComponent() {
            return import('./Pages/registration/registration').then(c => c.Registration);
        }
    }
];
