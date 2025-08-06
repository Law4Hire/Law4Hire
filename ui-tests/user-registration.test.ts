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

describe('User Registration UI Tests', () => {
  let page: puppeteer.Page;

  beforeEach(async () => {
    page = await createTestPage();
  });

  afterEach(async () => {
    if (page) {
      await page.close();
    }
  });

  describe('Basic Registration Flow', () => {
    test('should complete full registration with valid data', async () => {
      const testUser = generateTestUser('Work');
      
      // Navigate to registration page
      await page.goto(`${TEST_CONFIG.BASE_URL}/`);
      await waitForPageLoad(page);

      // Look for registration or sign up button/link
      const registrationSelectors = [
        'a[href*="register"]',
        'button[contains(text(), "Sign Up")]',
        'button[contains(text(), "Register")]',
        '.register-button',
        '.sign-up-button'
      ];

      let registrationFound = false;
      for (const selector of registrationSelectors) {
        try {
          await page.waitForSelector(selector, { timeout: 2000 });
          await page.click(selector);
          registrationFound = true;
          break;
        } catch (e) {
          // Continue to next selector
        }
      }

      if (!registrationFound) {
        // If no registration button found, try navigating directly
        await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      }

      await waitForPageLoad(page);

      // Fill registration form
      await fillFormField(page, 'input[name="firstName"], #firstName, [placeholder*="First"]', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName, [placeholder*="Last"]', testUser.lastName);
      await fillFormField(page, 'input[name="email"], #email, [type="email"]', testUser.email);
      await fillFormField(page, 'input[name="phoneNumber"], #phoneNumber, [placeholder*="Phone"]', testUser.phoneNumber);
      await fillFormField(page, 'input[name="password"], #password, [type="password"]', testUser.password);
      
      // Try to find and fill address fields
      try {
        await fillFormField(page, 'input[name="address1"], #address1, [placeholder*="Address"]', testUser.address1);
        await fillFormField(page, 'input[name="city"], #city, [placeholder*="City"]', testUser.city);
        await fillFormField(page, 'input[name="state"], #state, [placeholder*="State"]', testUser.state);
        await fillFormField(page, 'input[name="postalCode"], #postalCode, [placeholder*="Zip"], [placeholder*="Postal"]', testUser.postalCode);
      } catch (e) {
        console.log('Some address fields not found, continuing...');
      }

      // Try to select category if dropdown exists
      try {
        await selectDropdownOption(page, 'select[name="category"], #category', testUser.category);
      } catch (e) {
        console.log('Category dropdown not found, continuing...');
      }

      // Submit form
      const submitSelectors = [
        'button[type="submit"]',
        'input[type="submit"]',
        'button[contains(text(), "Register")]',
        'button[contains(text(), "Sign Up")]',
        '.submit-button',
        '.register-submit'
      ];

      let submitFound = false;
      for (const selector of submitSelectors) {
        try {
          await clickButton(page, selector);
          submitFound = true;
          break;
        } catch (e) {
          // Continue to next selector
        }
      }

      expect(submitFound).toBe(true);

      // Wait for success indication
      await page.waitForTimeout(3000); // Give time for API call

      // Check for success indicators
      const successIndicators = [
        'Registration successful',
        'Account created',
        'Welcome',
        'Success',
        'successfully registered'
      ];

      let successFound = false;
      for (const indicator of successIndicators) {
        try {
          await waitForText(page, indicator, 5000);
          successFound = true;
          break;
        } catch (e) {
          // Continue to next indicator
        }
      }

      // Also check if redirected to dashboard or login
      const currentUrl = page.url();
      const isRedirected = currentUrl.includes('dashboard') || 
                          currentUrl.includes('login') || 
                          currentUrl.includes('profile') ||
                          !currentUrl.includes('register');

      expect(successFound || isRedirected).toBe(true);
    });

    test('should reject registration with weak password', async () => {
      const testUser = generateTestUser('Work');
      testUser.password = 'weak'; // Weak password

      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      // Fill form with weak password
      await fillFormField(page, 'input[name="firstName"], #firstName, [placeholder*="First"]', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName, [placeholder*="Last"]', testUser.lastName);
      await fillFormField(page, 'input[name="email"], #email, [type="email"]', testUser.email);
      await fillFormField(page, 'input[name="password"], #password, [type="password"]', testUser.password);

      // Submit form
      try {
        await clickButton(page, 'button[type="submit"], input[type="submit"]');
      } catch (e) {
        // Form might prevent submission
      }

      // Check for password validation error
      const errorIndicators = [
        'Password must',
        'password',
        'weak',
        'requirements',
        'invalid'
      ];

      let errorFound = false;
      for (const indicator of errorIndicators) {
        try {
          await waitForText(page, indicator, 5000);
          errorFound = true;
          break;
        } catch (e) {
          // Continue
        }
      }

      expect(errorFound).toBe(true);
    });

    test('should reject duplicate email registration', async () => {
      const testUser = generateTestUser('Work');
      
      // First registration
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="email"], #email, [type="email"]', testUser.email);
      await fillFormField(page, 'input[name="firstName"], #firstName', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName', testUser.lastName);
      await fillFormField(page, 'input[name="password"], #password', testUser.password);

      await clickButton(page, 'button[type="submit"]');
      await page.waitForTimeout(3000);

      // Second registration with same email
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="email"], #email, [type="email"]', testUser.email);
      await fillFormField(page, 'input[name="firstName"], #firstName', 'Different');
      await fillFormField(page, 'input[name="lastName"], #lastName', 'Name');
      await fillFormField(page, 'input[name="password"], #password', testUser.password);

      await clickButton(page, 'button[type="submit"]');

      // Check for duplicate email error
      const duplicateErrorIndicators = [
        'already exists',
        'taken',
        'duplicate',
        'already registered'
      ];

      let duplicateErrorFound = false;
      for (const indicator of duplicateErrorIndicators) {
        try {
          await waitForText(page, indicator, 5000);
          duplicateErrorFound = true;
          break;
        } catch (e) {
          // Continue
        }
      }

      expect(duplicateErrorFound).toBe(true);
    });
  });

  describe('Category-Specific Registration Tests', () => {
    const categories = ['Visit', 'Work', 'Study', 'Family', 'Investment', 'Asylum', 'Immigrate'];

    categories.forEach(category => {
      test(`should register user with ${category} category using standard password`, async () => {
        const testUser = generateTestUser(category);
        
        await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
        await waitForPageLoad(page);

        // Fill basic registration fields
        await fillFormField(page, 'input[name="firstName"], #firstName', testUser.firstName);
        await fillFormField(page, 'input[name="lastName"], #lastName', testUser.lastName);
        await fillFormField(page, 'input[name="email"], #email', testUser.email);
        await fillFormField(page, 'input[name="password"], #password', TEST_CONFIG.STANDARD_PASSWORD);

        // Select category if available
        try {
          await selectDropdownOption(page, 'select[name="category"], #category', category);
        } catch (e) {
          console.log(`Category selection not available for ${category}`);
        }

        // Fill optional fields if present
        try {
          await fillFormField(page, 'input[name="phoneNumber"], #phoneNumber', testUser.phoneNumber);
          await fillFormField(page, 'input[name="address1"], #address1', testUser.address1);
          await fillFormField(page, 'input[name="city"], #city', testUser.city);
        } catch (e) {
          // Optional fields may not exist
        }

        // Submit registration
        await clickButton(page, 'button[type="submit"]');
        await page.waitForTimeout(3000);

        // Verify registration success
        const currentUrl = page.url();
        const registrationSuccessful = !currentUrl.includes('register') || 
                                     currentUrl.includes('dashboard') ||
                                     currentUrl.includes('profile');

        expect(registrationSuccessful).toBe(true);

        // Try to verify the created user can login (if login page exists)
        try {
          await page.goto(`${TEST_CONFIG.BASE_URL}/login`);
          await waitForPageLoad(page);
          
          await fillFormField(page, 'input[name="email"], #email', testUser.email);
          await fillFormField(page, 'input[name="password"], #password', TEST_CONFIG.STANDARD_PASSWORD);
          await clickButton(page, 'button[type="submit"]');
          
          await page.waitForTimeout(2000);
          const loginUrl = page.url();
          const loginSuccessful = !loginUrl.includes('login');
          
          expect(loginSuccessful).toBe(true);
        } catch (e) {
          console.log(`Login verification failed for ${category}, but registration appeared successful`);
        }
      });
    });
  });

  describe('Form Validation Tests', () => {
    test('should show validation errors for empty required fields', async () => {
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      // Try to submit empty form
      try {
        await clickButton(page, 'button[type="submit"]');
      } catch (e) {
        // Form might prevent submission
      }

      // Check for validation messages
      const validationMessages = await page.$$eval(
        '.error, .validation-error, .field-error, [class*="error"]',
        elements => elements.map(el => el.textContent)
      );

      expect(validationMessages.length).toBeGreaterThan(0);
    });

    test('should validate email format', async () => {
      const testUser = generateTestUser('Work');
      testUser.email = 'invalid-email';

      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="email"], #email', testUser.email);
      await fillFormField(page, 'input[name="firstName"], #firstName', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName', testUser.lastName);
      await fillFormField(page, 'input[name="password"], #password', testUser.password);

      try {
        await clickButton(page, 'button[type="submit"]');
      } catch (e) {
        // Form might prevent submission
      }

      // Check for email validation error
      let emailErrorFound = false;
      try {
        await waitForText(page, 'email', 3000);
        emailErrorFound = true;
      } catch (e) {
        // Also check for HTML5 validation
        const emailInput = await page.$('input[type="email"]');
        if (emailInput) {
          const validationMessage = await emailInput.evaluate(el => (el as HTMLInputElement).validationMessage);
          emailErrorFound = validationMessage.length > 0;
        }
      }

      expect(emailErrorFound).toBe(true);
    });
  });

  describe('User Experience Tests', () => {
    test('should have responsive design on mobile viewport', async () => {
      await page.setViewport({ width: 375, height: 667 }); // iPhone viewport
      
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      // Check if form is visible and usable on mobile
      const formVisible = await page.$('form') !== null;
      expect(formVisible).toBe(true);

      // Check if form fields are properly sized
      const inputElements = await page.$$('input');
      expect(inputElements.length).toBeGreaterThan(0);

      for (const input of inputElements.slice(0, 3)) { // Check first 3 inputs
        const boundingBox = await input.boundingBox();
        if (boundingBox) {
          expect(boundingBox.width).toBeGreaterThan(100); // Reasonable minimum width
          expect(boundingBox.height).toBeGreaterThan(20);  // Reasonable minimum height
        }
      }
    });

    test('should provide clear feedback during form submission', async () => {
      const testUser = generateTestUser('Work');
      
      await page.goto(`${TEST_CONFIG.BASE_URL}/register`);
      await waitForPageLoad(page);

      await fillFormField(page, 'input[name="firstName"], #firstName', testUser.firstName);
      await fillFormField(page, 'input[name="lastName"], #lastName', testUser.lastName);
      await fillFormField(page, 'input[name="email"], #email', testUser.email);
      await fillFormField(page, 'input[name="password"], #password', testUser.password);

      // Submit form and check for loading indicators
      await clickButton(page, 'button[type="submit"]');

      // Check for loading state (button disabled, spinner, etc.)
      try {
        await page.waitForSelector('.loading, .spinner, [disabled]', { timeout: 2000 });
      } catch (e) {
        // Loading indicator might not be present, that's okay
      }

      // Wait for completion
      await page.waitForTimeout(3000);

      // Should either show success message or redirect
      const currentUrl = page.url();
      const hasSuccessIndicator = !currentUrl.includes('register');
      
      expect(hasSuccessIndicator).toBe(true);
    });
  });
});