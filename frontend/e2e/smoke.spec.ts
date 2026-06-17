import { expect, test } from '@playwright/test';

test.describe('PayTrack smoke tests', () => {
  test('redirects anonymous users from the app root to login', async ({ page }) => {
    await page.goto('/');

    await expect(page).toHaveURL(/\/login$/);
    await expect(page.getByRole('heading', { name: 'Welcome to PayTrack' })).toBeVisible();
  });

  test('shows the login screen', async ({ page }) => {
    await page.goto('/login');

    await expect(page).toHaveTitle(/TUR PayTrack/);
    await expect(page.getByRole('heading', { name: 'Welcome to PayTrack' })).toBeVisible();
    await expect(page.getByTestId('login-google-button')).toBeVisible();
  });
});
