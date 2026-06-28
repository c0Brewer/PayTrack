import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

import { Role, UserDto } from '../../../types/exporter';

import { SettingsComponent } from './settings-component';

describe('SettingsComponent', () => {
  let component: SettingsComponent;
  let fixture: ComponentFixture<SettingsComponent>;
  let fragment$: BehaviorSubject<string | null>;

  beforeEach(async () => {
    fragment$ = new BehaviorSubject<string | null>(null);

    await TestBed.configureTestingModule({
      imports: [SettingsComponent],
      providers: [
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { fragment: fragment$ } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should default to the profile tab', () => {
    expect(component['activeTab']()).toBe('profile');
  });

  // ── isActiveTab ────────────────────────────────────────────────────────────

  describe('isActiveTab', () => {
    it('should return true for the currently active tab', () => {
      expect(component['isActiveTab']('profile')).toBe(true);
    });

    it('should return false for a tab that is not active', () => {
      expect(component['isActiveTab']('notifications')).toBe(false);
    });
  });

  // ── filteredTabs ───────────────────────────────────────────────────────────

  describe('filteredTabs', () => {
    it('should return all 4 tabs for an admin user', () => {
      const adminUser = { role: Role.ADMIN } as UserDto;
      expect(component['filteredTabs'](adminUser)).toHaveLength(4);
    });

    it('should exclude the admin-only tab for a regular user', () => {
      const regularUser = { role: Role.REGULAR_USER } as UserDto;
      const tabs = component['filteredTabs'](regularUser);
      expect(tabs).toHaveLength(3);
      expect(tabs.every((t) => !t['adminOnly'])).toBe(true);
    });

    it('should exclude the admin-only tab for a null user', () => {
      const tabs = component['filteredTabs'](null);
      expect(tabs).toHaveLength(3);
    });
  });

  // ── hasNoBankAccounts ──────────────────────────────────────────────────────

  describe('hasNoBankAccounts', () => {
    it('should return false for a null user', () => {
      expect(component['hasNoBankAccounts'](null)).toBe(false);
    });

    it('should return true when the user has no bank accounts', () => {
      const user = { bankAccounts: [] } as unknown as UserDto;
      expect(component['hasNoBankAccounts'](user)).toBe(true);
    });

    it('should return false when the user has at least one bank account', () => {
      const user = { bankAccounts: [{ id: 1 }] } as unknown as UserDto;
      expect(component['hasNoBankAccounts'](user)).toBe(false);
    });
  });

  // ── fragment navigation (ngOnInit) ─────────────────────────────────────────

  describe('fragment navigation', () => {
    it('should set the active tab to a valid fragment', () => {
      fixture.detectChanges();
      fragment$.next('notifications');
      expect(component['activeTab']()).toBe('notifications');
    });

    it('should fall back to the profile tab for an unknown fragment', () => {
      fixture.detectChanges();
      fragment$.next('nonexistent-tab');
      expect(component['activeTab']()).toBe('profile');
    });
  });

  // ── ngOnDestroy ────────────────────────────────────────────────────────────

  describe('ngOnDestroy', () => {
    it('should stop reacting to fragment changes after destruction', () => {
      fixture.detectChanges();
      fragment$.next('notifications');
      expect(component['activeTab']()).toBe('notifications');

      component.ngOnDestroy();
      fragment$.next('bank-accounts');

      expect(component['activeTab']()).toBe('notifications');
    });
  });
});
