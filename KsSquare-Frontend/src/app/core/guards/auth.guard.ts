import { inject } from '@angular/core';
import { CanActivateFn, CanMatchFn, Router } from '@angular/router';
import { map } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { UserRole } from '../auth/auth.models';

function roleAccess(role:UserRole, attemptedUrl:string){const auth=inject(AuthService);const router=inject(Router);return auth.ensureSession().pipe(map(user=>user?.role===role?true:router.createUrlTree(user?[user.role==='Admin'?'/admin':'/profile']:['/login'],{queryParams:user?undefined:{returnUrl:attemptedUrl}})));}
export const customerGuard:CanActivateFn=(_,state)=>roleAccess('Customer',state.url);
export const adminGuard:CanMatchFn=(_,segments)=>roleAccess('Admin',`/admin/${segments.map(segment=>segment.path).join('/')}`);
