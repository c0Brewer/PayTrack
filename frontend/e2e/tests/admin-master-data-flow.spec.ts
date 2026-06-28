import { expect, Locator, Page, test } from '@playwright/test';

import { e2eUsers } from '../fixtures/users';
import { authenticatePage } from '../utils/auth';

test('creates season, team, cost centre, and budget', async ({ browserName, page, request }) => {
  await authenticatePage(page, request, e2eUsers.admin);

  const suffix = browserName.toUpperCase();
  const seasonName = `E2E Season ${suffix}`;
  const teamName = `E2E Team ${suffix}`;
  const costCentreName = `E2E Cost Centre ${suffix}`;
  const budgetName = `E2E Budget ${suffix}`;
  const budgetAmount = '4321';

  await createSeason(page, seasonName);
  await createTeam(page, {
    name: teamName,
    description: `Created by E2E ${suffix}`,
  });
  await createCostCentreWithBudget(page, {
    costCentreName,
    costCentreDescription: `Created by E2E ${suffix}`,
    budgetName,
    budgetAmount,
    seasonName,
    teamName,
  });

  await openCostCentreDetail(page, costCentreName);
  await expect(page.getByRole('heading', { name: costCentreName })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Budgets (1)' })).toBeVisible();
  await expect(page.locator('table.budget-table')).toContainText(/4\.321,00\s*€/);
  await expect(page.locator('table.budget-table')).toContainText('2026-07-01');
  await expect(page.locator('table.budget-table')).toContainText('2026-12-31');
});

async function createSeason(page: Page, seasonName: string): Promise<void> {
  await page.goto('/season');
  await expect(page.getByRole('heading', { name: 'Season Management' })).toBeVisible();

  await page.getByPlaceholder('Season name').fill(seasonName);
  await page.getByRole('button', { name: 'Create' }).click();

  await expect(page.getByText('Season created successfully')).toBeVisible();
  await expect(page.locator('.season-list__row').filter({ hasText: seasonName })).toBeVisible();
}

async function createTeam(
  page: Page,
  options: {
    name: string;
    description: string;
  },
): Promise<void> {
  await page.goto('/team');
  await expect(page.getByRole('heading', { name: 'Team Management' })).toBeVisible();

  await page.getByRole('button', { name: 'Create' }).click();
  const dialog = page.getByRole('dialog').filter({ hasText: 'Create Team' });
  await expect(dialog).toBeVisible();

  await dialog.locator('input').nth(0).fill(options.name);
  await dialog.locator('input').nth(1).fill(options.description);
  await dialog.getByRole('button', { name: 'Create' }).click();

  await expect(page.getByText(`Successfully created team ${options.name}`)).toBeVisible();
  await filterTableByName(page, options.name);
  await expect(page.locator('tbody tr').filter({ hasText: options.name })).toBeVisible();
}

async function createCostCentreWithBudget(
  page: Page,
  options: {
    costCentreName: string;
    costCentreDescription: string;
    budgetName: string;
    budgetAmount: string;
    seasonName: string;
    teamName: string;
  },
): Promise<void> {
  await page.goto('/cost-centre');
  await expect(page.getByRole('heading', { name: 'Cost Centre Management' })).toBeVisible();

  await page.getByRole('button', { name: 'Create' }).click();
  const dialog = page.getByRole('dialog').filter({ hasText: 'Create Cost Centre' });
  await expect(dialog).toBeVisible();

  await dialog.locator('input').nth(0).fill(options.costCentreName);
  await dialog.locator('input').nth(1).fill(options.costCentreDescription);
  await dialog.locator('input').nth(2).fill(options.budgetName);
  await dialog.locator('select').nth(1).selectOption({ label: options.seasonName });
  await dialog.locator('select').nth(2).selectOption({ label: options.teamName });
  await dialog.locator('input').nth(3).fill(options.budgetAmount);
  await dialog.locator('input').nth(4).fill('2026-07-01');
  await dialog.locator('input').nth(5).fill('2026-12-31');
  await dialog.getByRole('button', { name: '+ Add Budget' }).click();

  const newBudgetRow = dialog.locator('table.new-budgets tbody tr').filter({
    hasText: options.budgetName,
  });
  await expect(newBudgetRow).toBeVisible();
  await expect(newBudgetRow).toContainText(options.seasonName);
  await expect(newBudgetRow).toContainText(options.teamName);

  await dialog.getByRole('button', { name: 'Create' }).click();
  await expect(page.getByText('Cost centre created successfully')).toBeVisible();
  await filterTableByName(page, options.costCentreName);
  await expect(page.locator('tbody tr').filter({ hasText: options.costCentreName })).toBeVisible();
}

async function openCostCentreDetail(page: Page, costCentreName: string): Promise<void> {
  await page.goto('/cost-centre');
  await expect(page.getByRole('heading', { name: 'Cost Centre Management' })).toBeVisible();
  await filterTableByName(page, costCentreName);

  const row = page.locator('tbody tr').filter({ hasText: costCentreName });
  await row.getByRole('link', { name: costCentreName }).click();
}

async function filterTableByName(page: Page, name: string): Promise<void> {
  await getNameFilter(page).fill(name);
}

function getNameFilter(page: Page): Locator {
  return page.locator('.toolbar').getByPlaceholder('Name...');
}
