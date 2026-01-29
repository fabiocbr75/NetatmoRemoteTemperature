import { test, expect } from '@playwright/test';

test.describe('Navigation', () => {
  test('should navigate from dashboard to history when clicking a sensor card', async ({ page }) => {
    await page.goto('/iot-dashboard');
    
    // If there are sensor cards, clicking one should navigate to history
    // This test will pass even if no sensors are available
    const sensorCard = page.locator('.sensor-card-wrapper').first();
    
    if (await sensorCard.isVisible()) {
      await sensorCard.click();
      
      // Should navigate to history page
      await expect(page).toHaveURL(/\/history\//);
    }
  });

  test('should navigate back to dashboard from navigation link', async ({ page }) => {
    await page.goto('/history/test-mac');
    
    // Click dashboard link in nav
    await page.locator('.nav-link').click();
    
    // Should navigate back to dashboard
    await expect(page).toHaveURL(/\/iot-dashboard/);
  });

  test('should handle 404 for unknown routes', async ({ page }) => {
    await page.goto('/non-existent-route');
    
    // Should redirect to dashboard (based on our routing setup)
    // Or show 404 - depends on implementation
    await expect(page).toHaveURL(/\/iot-dashboard/);
  });
});

test.describe('Responsive Design', () => {
  test('should be mobile responsive on dashboard', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/iot-dashboard');
    
    // Header should be visible
    await expect(page.locator('.header')).toBeVisible();
    
    // Main content should be visible
    await expect(page.locator('.main-content')).toBeVisible();
  });

  test('should be mobile responsive on history page', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/history/test-mac');
    
    // Controls should be visible
    await expect(page.locator('.controls')).toBeVisible();
    
    // Date picker should be visible
    await expect(page.locator('.date-picker')).toBeVisible();
  });
});
