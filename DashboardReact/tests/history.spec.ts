import { test, expect } from '@playwright/test';

test.describe('History Page', () => {
  const testMac = '00:00:00:00:00:00';

  test('should display the history page for a sensor', async ({ page }) => {
    await page.goto(`/history/${testMac}`);
    
    // Should show the page title
    await expect(page.locator('h2')).toContainText('History Graph');
  });

  test('should display date range picker controls', async ({ page }) => {
    await page.goto(`/history/${testMac}`);
    
    // Check for date inputs
    const dateInputs = page.locator('input[type="date"]');
    await expect(dateInputs).toHaveCount(2);
    
    // Check for load button
    await expect(page.locator('.load-button')).toContainText('Load Data');
  });

  test('should display min/max analysis checkbox', async ({ page }) => {
    await page.goto(`/history/${testMac}`);
    
    // Check for checkbox
    const checkbox = page.locator('input[type="checkbox"]');
    await expect(checkbox).toBeVisible();
    
    // Check label text
    await expect(page.locator('.checkbox-label')).toContainText('Min/Max Analysis');
  });

  test('should toggle between graph and analysis view', async ({ page }) => {
    await page.goto(`/history/${testMac}`);
    
    // Initially should show graph or loading/error
    const initialView = page.locator('.history-graph, .loading-message, .error-message');
    await expect(initialView).toBeVisible();
    
    // Toggle checkbox
    const checkbox = page.locator('input[type="checkbox"]');
    await checkbox.check();
    
    // Should show analysis view or loading/error
    const analysisView = page.locator('.history-analysis, .loading-message, .error-message');
    await expect(analysisView).toBeVisible();
  });

  test('should allow date range selection', async ({ page }) => {
    await page.goto(`/history/${testMac}`);
    
    // Get date inputs
    const fromDateInput = page.locator('input[type="date"]').first();
    const toDateInput = page.locator('input[type="date"]').last();
    
    // Set date values
    await fromDateInput.fill('2024-01-01');
    await toDateInput.fill('2024-01-07');
    
    // Click load button
    await page.locator('.load-button').click();
    
    // Should trigger a data load (could show loading state)
    // This is a basic check - actual data loading depends on backend
    await expect(page.locator('.history-content')).toBeVisible();
  });
});
