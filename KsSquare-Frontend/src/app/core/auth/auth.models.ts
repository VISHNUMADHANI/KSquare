export type UserRole = 'Customer' | 'Admin';
export interface AuthUser { id:string; fullName:string; email:string; phone:string; role:UserRole; isEmailVerified:boolean; }
export interface AuthMessage { message:string; }
