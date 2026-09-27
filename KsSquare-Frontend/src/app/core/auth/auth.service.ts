import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { Observable, catchError, finalize, map, of, shareReplay, tap } from 'rxjs';
import { AuthMessage, AuthUser } from './auth.models';

@Injectable({ providedIn:'root' })
export class AuthService {
  private readonly baseUrl='/api/auth';
  private sessionRequest?:Observable<AuthUser|null>;
  readonly user=signal<AuthUser|null>(null);
  readonly initialized=signal(false);
  constructor(private readonly http:HttpClient){}
  ensureSession():Observable<AuthUser|null>{if(this.initialized())return of(this.user());if(!this.sessionRequest){this.sessionRequest=this.http.get<AuthUser>(`${this.baseUrl}/me`).pipe(tap(user=>this.user.set(user)),map(user=>user as AuthUser|null),catchError(()=>{this.user.set(null);return of(null);}),finalize(()=>{this.initialized.set(true);this.sessionRequest=undefined;}),shareReplay(1));}return this.sessionRequest;}
  register(body:{fullName:string;email:string;phone:string;password:string}):Observable<AuthMessage>{return this.http.post<AuthMessage>(`${this.baseUrl}/register`,body);}
  verifyEmail(email:string,code:string):Observable<AuthUser>{return this.http.post<AuthUser>(`${this.baseUrl}/verify-email`,{email,code});}
  resendVerification(email:string):Observable<AuthMessage>{return this.http.post<AuthMessage>(`${this.baseUrl}/resend-verification`,{email});}
  login(email:string,password:string):Observable<AuthUser>{return this.http.post<AuthUser>(`${this.baseUrl}/login`,{email,password}).pipe(tap(user=>{this.user.set(user);this.initialized.set(true);}));}
  forgotPassword(email:string):Observable<AuthMessage>{return this.http.post<AuthMessage>(`${this.baseUrl}/forgot-password`,{email});}
  resetPassword(email:string,code:string,newPassword:string):Observable<AuthMessage>{return this.http.post<AuthMessage>(`${this.baseUrl}/reset-password`,{email,code,newPassword});}
  updateProfile(fullName:string,phone:string):Observable<AuthUser>{return this.http.put<AuthUser>(`${this.baseUrl}/profile`,{fullName,phone}).pipe(tap(user=>this.user.set(user)));}
  changePassword(currentPassword:string,newPassword:string):Observable<void>{return this.http.put<void>(`${this.baseUrl}/change-password`,{currentPassword,newPassword});}
  logout():Observable<void>{return this.http.post<void>(`${this.baseUrl}/logout`,{}).pipe(finalize(()=>{this.user.set(null);this.initialized.set(true);}));}
}
