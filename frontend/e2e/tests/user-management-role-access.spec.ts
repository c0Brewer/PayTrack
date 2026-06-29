import { expect, Locator, Page, test } from '@playwright/test';

import { E2ERole, e2eUsers, E2EUser, getHomeDashboardUser } from '../fixtures/users';
import { authenticatePage } from '../utils/auth';

test('updates a user role team and active state and applies the changed access', async ({
  browser,
  browserName,
  page,
  request,
}) => {
  test.setTimeout(60_000);

  const targetUser = getHomeDashboardUser(browserName);

  await authenticatePage(page, request, e2eUsers.admin);
  await openUserManagementForEmail(page, targetUser.email);

  await editUser(page, {
    email: targetUser.email,
    role: 'Admin',
    team: 'Electronics',
    active: false,
  });
  await expectUserRow(page, targetUser.email, {
    role: 'Admin',
    team: 'Electronics',
    activeState: 'Inactive',
  });

  const inactiveContext = await browser.newContext();
  const inactivePage = await inactiveContext.newPage();
  await authenticatePage(inactivePage, request, {
    ...targetUser,
    role: E2ERole.Admin,
  });
  await inactivePage.goto('/submit');
  await expectUnauthorizedPage(inactivePage);
  await expect(
    inactivePage.getByRole('complementary').getByRole('link', { name: /Submit An Invoice/ }),
  ).toHaveCount(0);
  await inactiveContext.close();

  await openUserManagementForEmail(page, targetUser.email);
  await editUser(page, {
    email: targetUser.email,
    role: 'Admin',
    team: 'Electronics',
    active: true,
  });
  await expectUserRow(page, targetUser.email, {
    role: 'Admin',
    team: 'Electronics',
    activeState: 'Active',
  });

  const promotedContext = await browser.newContext();
  const promotedPage = await promotedContext.newPage();
  await authenticatePage(promotedPage, request, promotedAdminUser(targetUser));
  await promotedPage.goto('/');
  await promotedPage.reload();

  const navigation = promotedPage.getByRole('complementary');
  await expect(navigation.getByRole('button', { name: /Management/ })).toBeVisible();
  await expect(navigation.getByRole('button', { name: /Requests/ })).toBeVisible();

  await promotedPage.goto('/user');
  await expect(promotedPage.getByRole('heading', { name: 'User Management' })).toBeVisible();
  await promotedContext.close();
});

async function openUserManagementForEmail(page: Page, email: string): Promise<void> {
  await page.goto('/user');
  await expect(page.getByRole('heading', { name: 'User Management' })).toBeVisible();
  await page.getByPlaceholder('Filter by email...').fill(email);
  await expect(userRow(page, email)).toBeVisible();
}

async function editUser(
  page: Page,
  options: {
    email: string;
    role: 'Regular User' | 'Team Lead' | 'Admin';
    team: string;
    active: boolean;
  },
): Promise<void> {
  await userRow(page, options.email).getByRole('button', { name: /Edit/ }).click();

  const dialog = page.getByRole('dialog').filter({ hasText: 'Edit User' });
  await expect(dialog).toBeVisible();
  const roleSelect = dialog.locator('select[name="role"]');
  const teamSelect = dialog.locator('select[name="team"]');
  await expect(roleSelect).toBeVisible({ timeout: 10_000 });
  await expect(teamSelect).toBeVisible({ timeout: 10_000 });
  await roleSelect.selectOption({ label: options.role });
  await teamSelect.selectOption({ label: options.team });
  await dialog.getByRole('switch', { name: 'Active' }).setChecked(options.active);
  await dialog.getByRole('button', { name: /Save/ }).click();

  await expect(page.getByText(/Successfully updated user/)).toBeVisible();
  await expect(dialog).toBeHidden();
}

async function expectUserRow(
  page: Page,
  email: string,
  expected: {
    role: string;
    team: string;
    activeState: 'Active' | 'Inactive';
  },
): Promise<void> {
  const row = userRow(page, email);
  await expect(row).toContainText(expected.role);
  await expect(row).toContainText(expected.team);
  await expect(row.getByRole('button', { name: expected.activeState })).toBeVisible();
}

async function expectUnauthorizedPage(page: Page): Promise<void> {
  await expect(page).toHaveURL(/\/unauthorized$/);
  await expect(page.getByRole('heading', { name: 'Access Denied' })).toBeVisible();
  await expect(page.getByText('You do not have permission to access this page.')).toBeVisible();
}

function userRow(page: Page, email: string): Locator {
  return page.locator('tbody tr').filter({ hasText: email });
}

function promotedAdminUser(user: E2EUser): E2EUser {
  return {
    ...user,
    role: E2ERole.Admin,
  };
}
