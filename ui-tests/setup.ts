import puppeteer from 'puppeteer';

// Global test configuration
export const TEST_CONFIG = {
  BASE_URL: 'http://localhost:5161',
  API_BASE_URL: 'https://localhost:7280',
  DEFAULT_TIMEOUT: 10000,
  STANDARD_PASSWORD: 'SecureTest123!',
  TEST_EMAIL_DOMAIN: '@testing.com'
};

// Global browser instance for tests
let browser: puppeteer.Browser;

beforeAll(async () => {
  browser = await puppeteer.launch({
    headless: process.env.NODE_ENV === 'production',
    slowMo: 50,
    args: ['--no-sandbox', '--disable-setuid-sandbox', '--ignore-certificate-errors']
  });
});

afterAll(async () => {
  if (browser) {
    await browser.close();
  }
});

// Helper function to create a new page with common setup
export const createTestPage = async () => {
  const page = await browser.newPage();
  await page.setViewport({ width: 1280, height: 720 });
  
  // Handle certificate errors for HTTPS API calls
  await page.setExtraHTTPHeaders({
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  });
  
  return page;
};

// Generate test user data
export const generateTestUser = (category: string = 'Work') => {
  const timestamp = Date.now();
  const randomId = Math.random().toString(36).substring(2, 8);
  
  return {
    firstName: `Test${category}`,
    lastName: 'User',
    email: `${category.toLowerCase()}${randomId}${timestamp}${TEST_CONFIG.TEST_EMAIL_DOMAIN}`,
    phoneNumber: '555-0123',
    password: TEST_CONFIG.STANDARD_PASSWORD,
    address1: '123 Test Street',
    city: 'Test City',
    state: 'CA',
    country: 'United States',
    postalCode: '12345',
    category: category,
    dateOfBirth: '1990-01-01',
    maritalStatus: 'Single',
    educationLevel: "Bachelor's Degree"
  };
};

// Helper to wait for navigation and ensure page is loaded
export const waitForPageLoad = async (page: puppeteer.Page, timeout = TEST_CONFIG.DEFAULT_TIMEOUT) => {
  await page.waitForLoadState?.('networkidle') || 
        await page.waitForTimeout(1000); // Fallback for older versions
};

// Helper to fill form fields
export const fillFormField = async (page: puppeteer.Page, selector: string, value: string) => {
  await page.waitForSelector(selector, { timeout: TEST_CONFIG.DEFAULT_TIMEOUT });
  await page.click(selector);
  await page.evaluate((sel) => {
    const element = document.querySelector(sel) as HTMLInputElement;
    if (element) element.value = '';
  }, selector);
  await page.type(selector, value);
};

// Helper to select dropdown option
export const selectDropdownOption = async (page: puppeteer.Page, selector: string, value: string) => {
  await page.waitForSelector(selector, { timeout: TEST_CONFIG.DEFAULT_TIMEOUT });
  await page.select(selector, value);
};

// Helper to wait for and click button
export const clickButton = async (page: puppeteer.Page, selector: string) => {
  await page.waitForSelector(selector, { timeout: TEST_CONFIG.DEFAULT_TIMEOUT });
  await page.click(selector);
};

// Helper to wait for element with text
export const waitForText = async (page: puppeteer.Page, text: string, timeout = TEST_CONFIG.DEFAULT_TIMEOUT) => {
  await page.waitForFunction(
    (searchText) => document.body.innerText.includes(searchText),
    { timeout },
    text
  );
};

export { browser };