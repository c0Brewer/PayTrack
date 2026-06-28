import { expect, test } from '@playwright/test';

test('shows the PayTrack login page', async ({ page }) => {
  await page.goto('/login');

  await expect(page).toHaveTitle(/TUR PayTrack/);
  await expect(page.getByRole('heading', { name: 'Welcome to PayTrack' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Login Via Google' })).toBeVisible();
});
