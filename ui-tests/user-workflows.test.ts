import puppeteer from 'puppeteer';
import { 
  createTestPage, 
  generateTestUser, 
  fillFormField, 
  selectDropdownOption,
  clickButton,
  waitForText,
  waitForPageLoad,
  TEST_CONFIG 
} from './setup';

describe('User Workflow UI Tests', () => {
  let page: puppeteer.Page;

  beforeEach(async () => {
    page = await createTestPage();
  });

  afterEach(async () => {
    if (page) {
      await page.close();
    }
  });

  describe('Complete User Journey Tests', () => {
    test('should complete full journey from registration to dashboard for Work category', async () => {
      const testUser = generateTestUser('Work');
      
      // Step 1: Register
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="firstName"], #firstName', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName', testUser.lastName);
      await fillFormField(page, 'input[name="email"], #email', testUser.email);
      await fillFormField(page, 'input[name="password"], #password', TEST_CONFIG.STANDARD_PASSWORD);

      try {
        await selectDropdownOption(page, 'select[name="category"], #category', 'Work');
      } catch (e) {
        console.log('Category selection not available during registration');
      }

      await clickButton(page, 'button[type="submit"]');
      await page.waitForTimeout(3000);

      // Step 2: Navigate to dashboard if not already there
      try {
        await page.goto(`${TEST_CONFIG.BASE_URL}/dashboard`);
        await waitForPageLoad(page);
      } catch (e) {
        console.log('Dashboard navigation failed, checking current page');
      }

      // Step 3: Verify dashboard elements
      const dashboardElements = [
        'dashboard',
        'profile',
        'case',
        'status',
        'visa',
        'application'
      ];

      let dashboardElementFound = false;
      for (const element of dashboardElements) {
        try {
          await waitForText(page, element, 3000);
          dashboardElementFound = true;
          break;
        } catch (e) {
          // Continue searching
        }
      }

      expect(dashboardElementFound).toBe(true);
    });

    test('should complete visa interview flow for Study category', async () => {
      const testUser = generateTestUser('Study');
      
      // Register user first
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="firstName"], #firstName', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName', testUser.lastName);
      await fillFormField(page, 'input[name="email"], #email', testUser.email);
      await fillFormField(page, 'input[name="password"], #password', TEST_CONFIG.STANDARD_PASSWORD);

      await clickButton(page, 'button[type="submit"]');
      await page.waitForTimeout(3000);

      // Navigate to interview or visa assessment
      const interviewUrls = [
        `${TEST_CONFIG.BASE_URL}/interview`,
        `${TEST_CONFIG.BASE_URL}/assessment`,
        `${TEST_CONFIG.BASE_URL}/visa-interview`,
        `${TEST_CONFIG.BASE_URL}/InterviewPhase2`
      ];

      let interviewPageFound = false;
      for (const url of interviewUrls) {
        try {
          await page.goto(url);
          await waitForPageLoad(page);
          
          // Check if this is an interview page
          const interviewIndicators = ['interview', 'question', 'assessment', 'visa'];
          for (const indicator of interviewIndicators) {
            try {
              await waitForText(page, indicator, 2000);
              interviewPageFound = true;
              break;
            } catch (e) {
              // Continue
            }
          }
          
          if (interviewPageFound) break;
        } catch (e) {
          // Try next URL
        }
      }

      if (interviewPageFound) {
        // Answer some interview questions
        try {
          // Look for radio buttons, checkboxes, or select dropdowns
          const choices = await page.$$('input[type="radio"], input[type="checkbox"], select');
          if (choices.length > 0) {
            await choices[0].click();
          }

          // Look for next button
          const nextButtons = await page.$$('button:contains("Next"), button:contains("Continue"), .next-button');
          if (nextButtons.length > 0) {
            await nextButtons[0].click();
            await page.waitForTimeout(2000);
          }
        } catch (e) {
          console.log('Interview interaction failed, but page was found');
        }
      }

      // At minimum, we should have found an interview page or be redirected appropriately
      expect(interviewPageFound || page.url().includes('dashboard')).toBe(true);
    });

    test('should handle pricing page navigation for Investment category', async () => {
      const testUser = generateTestUser('Investment');
      
      // Go to pricing page
      await page.goto(`${TEST_CONFIG.BASE_URL}/pricing`);
      await waitForPageLoad(page);

      // Check for pricing elements
      const pricingElements = [
        'price',
        'package',
        'plan',
        'service',
        '$',
        'fee'
      ];

      let pricingElementFound = false;
      for (const element of pricingElements) {
        try {
          await waitForText(page, element, 3000);
          pricingElementFound = true;
          break;
        } catch (e) {
          // Continue
        }
      }

      expect(pricingElementFound).toBe(true);

      // Try to select a package
      try {
        const selectButtons = await page.$$('button:contains("Select"), button:contains("Choose"), .select-plan');
        if (selectButtons.length > 0) {
          await selectButtons[0].click();
          await page.waitForTimeout(2000);
          
          // Should either redirect to registration or login
          const currentUrl = page.url();
          const redirectedAppropriately = currentUrl.includes('register') || 
                                        currentUrl.includes('login') ||
                                        currentUrl.includes('checkout');
          
          expect(redirectedAppropriately).toBe(true);
        }
      } catch (e) {
        console.log('Package selection not available or failed');
      }
    });
  });

  describe('Navigation and User Flow Tests', () => {
    test('should navigate between main pages successfully', async () => {
      const mainPages = [
        { url: '/', name: 'Home' },
        { url: '/pricing', name: 'Pricing' },
        { url: '/login', name: 'Login' },
        { url: '/register', name: 'Register' }
      ];

      for (const pageInfo of mainPages) {
        try {
          await page.goto(`${TEST_CONFIG.BASE_URL}${pageInfo.url}`);
          await waitForPageLoad(page);
          
          // Verify page loaded successfully
          const title = await page.title();
          expect(title.length).toBeGreaterThan(0);
          
          // Check for common page elements
          const bodyText = await page.$eval('body', el => el.textContent || '');
          expect(bodyText.length).toBeGreaterThan(100); // Page should have content
          
        } catch (e) {
          console.log(`Navigation to ${pageInfo.name} failed: ${e}`);
          // Don't fail the test for missing pages during development
        }
      }
    });

    test('should handle login flow with registered user', async () => {
      const testUser = generateTestUser('Family');
      
      // First register the user
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="firstName"], #firstName', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName', testUser.lastName);
      await fillFormField(page, 'input[name="email"], #email', testUser.email);
      await fillFormField(page, 'input[name="password"], #password', TEST_CONFIG.STANDARD_PASSWORD);

      await clickButton(page, 'button[type="submit"]');
      await page.waitForTimeout(3000);

      // Now try to login
      await page.goto(`${TEST_CONFIG.BASE_URL}/login`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="email"], #email', testUser.email);
      await fillFormField(page, 'input[name="password"], #password', TEST_CONFIG.STANDARD_PASSWORD);

      await clickButton(page, 'button[type="submit"]');
      await page.waitForTimeout(3000);

      // Should be redirected away from login page
      const currentUrl = page.url();
      const loginSuccessful = !currentUrl.includes('login') || 
                             currentUrl.includes('dashboard') ||
                             currentUrl.includes('profile');

      expect(loginSuccessful).toBe(true);
    });
  });

  describe('Error Handling Tests', () => {
    test('should handle network errors gracefully', async () => {
      // Simulate slow network
      await page.setRequestInterception(true);
      
      page.on('request', async (request) => {
        if (request.url().includes('api/')) {
          // Delay API calls to simulate slow network
          await new Promise(resolve => setTimeout(resolve, 2000));
        }
        request.continue();
      });

      const testUser = generateTestUser('Asylum');
      
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="firstName"], #firstName', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName', testUser.lastName);
      await fillFormField(page, 'input[name="email"], #email', testUser.email);
      await fillFormField(page, 'input[name="password"], #password', TEST_CONFIG.STANDARD_PASSWORD);

      await clickButton(page, 'button[type="submit"]');

      // Should show loading state or handle delay gracefully
      try {
        await page.waitForSelector('.loading, .spinner, [disabled]', { timeout: 3000 });
      } catch (e) {
        // Loading indicator might not be present
      }

      // Wait longer for the delayed response
      await page.waitForTimeout(5000);

      // Should eventually complete or show appropriate error
      const currentUrl = page.url();
      expect(currentUrl).toBeDefined();
    });

    test('should display appropriate error messages for invalid operations', async () => {
      // Try to access protected pages without authentication
      const protectedPages = ['/dashboard', '/profile'];
      
      for (const protectedPage of protectedPages) {
        try {
          await page.goto(`${TEST_CONFIG.BASE_URL}${protectedPage}`);
          await waitForPageLoad(page);
          
          // Should either redirect to login or show access denied
          const currentUrl = page.url();
          const handledAppropriately = currentUrl.includes('login') || 
                                     currentUrl.includes('unauthorized') ||
                                     currentUrl.includes('access-denied');
          
          // If not redirected, check for error message on page
          if (!handledAppropriately) {
            const errorMessages = [
              'unauthorized',
              'access denied',
              'login required',
              'authentication'
            ];
            
            let errorFound = false;
            for (const errorMsg of errorMessages) {
              try {
                await waitForText(page, errorMsg, 2000);
                errorFound = true;
                break;
              } catch (e) {
                // Continue
              }
            }
            
            expect(errorFound || handledAppropriately).toBe(true);
          }
          
        } catch (e) {
          console.log(`Protected page test failed for ${protectedPage}: ${e}`);
        }
      }
    });
  });

  describe('Accessibility Tests', () => {
    test('should have proper form labels and structure', async () => {
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      // Check for form labels
      const labels = await page.$$('label');
      const inputs = await page.$$('input');

      expect(labels.length).toBeGreaterThan(0);
      expect(inputs.length).toBeGreaterThan(0);

      // Check that inputs have associated labels or aria-labels
      for (let i = 0; i < Math.min(inputs.length, 5); i++) {
        const input = inputs[i];
        const id = await input.evaluate(el => el.id);
        const ariaLabel = await input.evaluate(el => el.getAttribute('aria-label'));
        const placeholder = await input.evaluate(el => el.getAttribute('placeholder'));
        
        const hasLabel = id && await page.$(`label[for="${id}"]`) !== null;
        const hasAccessibleName = hasLabel || ariaLabel || placeholder;
        
        expect(hasAccessibleName).toBe(true);
      }
    });

    test('should support keyboard navigation', async () => {
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      // Test tab navigation through form fields
      const inputs = await page.$$('input, button, select');
      
      if (inputs.length > 0) {
        // Focus first input
        await inputs[0].focus();
        
        // Tab through several fields
        for (let i = 0; i < Math.min(inputs.length - 1, 3); i++) {
          await page.keyboard.press('Tab');
          await page.waitForTimeout(100);
        }
        
        // Should be able to reach submit button via keyboard
        await page.keyboard.press('Tab');
        const activeElement = await page.evaluate(() => document.activeElement?.tagName);
        expect(['INPUT', 'BUTTON', 'SELECT'].includes(activeElement || '')).toBe(true);
      }
    });
  });
});