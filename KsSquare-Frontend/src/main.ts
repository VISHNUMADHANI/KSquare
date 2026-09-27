import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { bootstrapApplication } from '@angular/platform-browser';
import { provideRouter, withInMemoryScrolling } from '@angular/router';
import { AppComponent } from './app/app.component';
import { appRoutes } from './app/app.routes';
import { authCredentialsInterceptor } from './app/core/auth/auth.interceptor';

bootstrapApplication(AppComponent, { providers: [provideRouter(appRoutes, withInMemoryScrolling({ scrollPositionRestoration: 'enabled', anchorScrolling: 'enabled' })), provideHttpClient(withInterceptors([authCredentialsInterceptor]))] }).catch(err => console.error(err));
