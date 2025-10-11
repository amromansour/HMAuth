import { Routes } from '@angular/router';
import { authGuard } from './guards/auth-guard';
import { roleGuard } from './guards/role-guard';
export const routes: Routes = [
    {
        path: '',
        pathMatch: 'full',
        loadComponent() {
            return import('./Pages/home/home').then(c => c.Home);
        }, canActivate: [authGuard]
    },
    {
        path: 'login',
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
    },
    {
        path: 'MyProfile',
        loadComponent() {
            return import('./Pages/my-profile/my-profile').then(c => c.MyProfile);
        }, canActivate: [authGuard, roleGuard], data: { roles: ['Admin', 'User'] }
    },
    {
        path: 'Users',
        loadComponent() {
            return import('./Pages/users/users').then(c => c.Users);
        }, canActivate: [authGuard, roleGuard], data: { roles: ['Admin'] }
    },
    {
        path: 'NewAdmin',
        loadComponent() {
            return import('./Pages/new-admin/new-admin').then(c => c.NewAdmin);
        }, canActivate: [authGuard, roleGuard], data: { roles: ['Admin'] }
    },
    {
        path: 'UserPage',
        loadComponent() {
            return import('./Pages/user-page/user-page').then(c => c.UserPage);
        }, canActivate: [authGuard, roleGuard], data: { roles: ['User'] }
    },
    {
        path: 'unauthorized',
        loadComponent: () => import('./Pages/unauthorized/unauthorized').then(m => m.Unauthorized)
    }

];
