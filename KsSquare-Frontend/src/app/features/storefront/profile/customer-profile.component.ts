import { DialogFocusDirective } from '../../../shared/ui/dialog-focus.directive';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, HostListener, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { KsButtonDirective } from '../../../shared/ui';

@Component({
  selector: 'app-customer-profile',
  standalone: true,
  imports: [RouterLink, DialogFocusDirective, ReactiveFormsModule, KsButtonDirective],
  templateUrl: './customer-profile.component.html',
  styleUrls: ['./customer-profile.component.scss', './customer-profile-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CustomerProfileComponent implements OnInit {
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  protected readonly saving = signal(false);
  protected readonly passwordSaving = signal(false);
  protected readonly passwordDialogOpen = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');

  protected readonly profileForm = new FormGroup({
    fullName: new FormControl('', { nonNullable: true }),
    phone: new FormControl('', { nonNullable: true })
  });

  protected readonly passwordForm = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true }),
    newPassword: new FormControl('', { nonNullable: true }),
    confirmPassword: new FormControl('', { nonNullable: true })
  });

  ngOnInit(): void {
    this.auth.ensureSession().subscribe(user => {
      if (user) this.profileForm.reset({ fullName: user.fullName, phone: user.phone });
    });
  }

  protected get customerInitial(): string {
    return this.auth.user()?.fullName?.trim().charAt(0).toUpperCase() || 'K';
  }

  protected saveProfile(): void {
    const value = this.profileForm.getRawValue();
    if (!value.fullName.trim() || value.phone.trim().length < 7) {
      this.error.set('Enter a valid name and phone number.');
      return;
    }
    this.saving.set(true);
    this.clear();
    this.auth.updateProfile(value.fullName, value.phone).pipe(finalize(() => this.saving.set(false))).subscribe({
      next: () => {
        this.profileForm.markAsPristine();
        this.message.set('Your profile has been updated.');
      },
      error: error => this.error.set(this.apiMessage(error))
    });
  }

  protected openPasswordDialog(): void {
    this.passwordForm.reset();
    this.clear();
    this.passwordDialogOpen.set(true);
  }

  protected closePasswordDialog(): void {
    if (this.passwordSaving()) return;
    this.passwordDialogOpen.set(false);
    this.passwordForm.reset();
    this.error.set('');
  }

  protected changePassword(): void {
    const value = this.passwordForm.getRawValue();
    if (value.newPassword.length < 10 || value.newPassword !== value.confirmPassword) {
      this.error.set(value.newPassword !== value.confirmPassword ? 'Passwords do not match.' : 'New password must contain at least 10 characters.');
      return;
    }
    this.passwordSaving.set(true);
    this.clear();
    this.auth.changePassword(value.currentPassword, value.newPassword).pipe(finalize(() => this.passwordSaving.set(false))).subscribe({
      next: () => {
        this.passwordForm.reset();
        this.passwordDialogOpen.set(false);
        this.message.set('Your password has been changed successfully.');
      },
      error: error => this.error.set(this.apiMessage(error))
    });
  }

  protected logout(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/login']));
  }

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    if (this.passwordDialogOpen()) this.closePasswordDialog();
  }

  private clear(): void {
    this.error.set('');
    this.message.set('');
  }

  private apiMessage(error: unknown): string {
    return error instanceof HttpErrorResponse ? error.error?.error ?? 'The action could not be completed.' : 'The action could not be completed.';
  }
}
