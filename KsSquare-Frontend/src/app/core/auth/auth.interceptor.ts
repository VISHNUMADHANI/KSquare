import { HttpInterceptorFn } from '@angular/common/http';

export const authCredentialsInterceptor:HttpInterceptorFn=(request,next)=>next(request.url.startsWith('/api')?request.clone({withCredentials:true}):request);
