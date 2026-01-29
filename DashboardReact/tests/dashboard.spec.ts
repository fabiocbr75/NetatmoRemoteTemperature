import { test, expect } from '@playwright/test';

test.describe('Dashboard Page', () => {
  test('should display the dashboard page', async ({ page }) => {
    await page.goto('/');
    
    // Should redirect to /iot-dashboard
    await expect(page).toHaveURL(/\/iot-dashboard/);
    
    // Should show the page title
    await expect(page.locator('h2')).toContainText('IoT Dashboard');
  });

  test('should display header with title and navigation', async ({ page }) => {
    await page.goto('/iot-dashboard');
    
    // Check header title
    await expect(page.locator('.header-title')).toContainText('IoT Dashboard');
    
    // Check navigation link
    await expect(page.locator('.nav-link')).toContainText('Dashboard');
  });

  test('should handle sensor data loading states', async ({ page }) => {
    await page.goto('/iot-dashboard');
    
    // Initially should show loading or error message if no backend
    // Or should show sensor cards if backend is available
    const loadingOrError = page.locator('.loading-message, .error-message, .sensors-grid');
    await expect(loadingOrError).toBeVisible();
  });

  test('should navigate to dashboard from root', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/\/iot-dashboard/);
  });
});
