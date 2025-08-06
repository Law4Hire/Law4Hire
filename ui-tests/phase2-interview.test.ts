import puppeteer, { Browser, Page } from 'puppeteer';
import { generateTestUser, TEST_CONFIG } from './setup';

describe('Phase2 Interview Flow', () => {
    let browser: Browser;
    let page: Page;

    beforeAll(async () => {
        browser = await puppeteer.launch({ 
            headless: false, // Show browser for debugging
            slowMo: 100, // Slow down for visibility
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        });
        page = await browser.newPage();
    });

    afterAll(async () => {
        await browser.close();
    });

    test('Complete flow: Register user, login, and start Phase2 interview', async () => {
        console.log('🚀 Starting comprehensive Phase2 interview test...');
        
        // Step 1: Navigate to registration
        console.log('📝 Step 1: Navigating to registration page...');
        await page.goto('http://localhost:5161');
        await page.waitForTimeout(2000);
        
        // Look for registration link
        await page.waitForSelector('a[href*="register"], a, button', { timeout: 10000 });
        const registerLinks = await page.$$('a, button');
        let registerLink = null;
        
        for (const link of registerLinks) {
            const text = await page.evaluate(el => el.textContent, link);
            const href = await page.evaluate(el => el.getAttribute('href'), link);
            if (text?.includes('Register') || href?.includes('register')) {
                registerLink = link;
                break;
            }
        }
        
        if (registerLink) {
            await registerLink.click();
        } else {
            // Try direct navigation to register page
            await page.goto('http://localhost:5161/register');
        }
        await page.waitForTimeout(2000);

        // Step 2: Fill registration form for Immigrate category
        console.log('📋 Step 2: Filling registration form for Immigrate category...');
        const testUser = generateTestUser('Immigrate');
        
        // Fill required fields
        await page.fill('input[name="FirstName"], input[placeholder*="First"], #firstName', 'ImmigrateTest');
        await page.fill('input[name="LastName"], input[placeholder*="Last"], #lastName', 'User');
        await page.fill('input[name="Email"], input[type="email"], #email', testUser.email);
        await page.fill('input[name="Password"], input[type="password"], #password', testUser.password);
        
        // Select Immigrate category
        const categorySelect = page.locator('select[name="Category"], select[name="ImmigrationGoal"], #category').first();
        if (await categorySelect.isVisible()) {
            await categorySelect.selectOption('Immigrate');
        } else {
            // Try radio buttons or other category selection methods
            const immigrateOption = page.locator('input[value="Immigrate"], label:has-text("Immigrate")').first();
            if (await immigrateOption.isVisible()) {
                await immigrateOption.click();
            }
        }

        // Fill additional required fields
        await page.fill('input[name="PhoneNumber"], input[type="tel"], #phone', '+1234567890');
        await page.fill('input[name="City"], #city', 'Test City');
        await page.fill('input[name="State"], #state', 'CA');
        await page.fill('input[name="Country"], #country', 'United States');
        
        // Set date of birth
        const dobField = page.locator('input[name="DateOfBirth"], input[type="date"], #dateOfBirth').first();
        if (await dobField.isVisible()) {
            await dobField.fill('1990-01-01');
        }

        // Submit registration
        console.log('✅ Step 3: Submitting registration...');
        const submitButton = page.locator('button[type="submit"], button:has-text("Register"), input[type="submit"]').first();
        await submitButton.click();
        
        // Wait for registration success or redirect
        await page.waitForTimeout(3000);
        
        // Step 4: Login if redirected to login page
        console.log('🔐 Step 4: Attempting login...');
        const currentUrl = page.url();
        if (currentUrl.includes('login') || currentUrl.includes('Login')) {
            console.log('Redirected to login page, logging in...');
            await page.fill('input[name="Email"], input[type="email"]', testUser.email);
            await page.fill('input[name="Password"], input[type="password"]', testUser.password);
            const loginButton = page.locator('button[type="submit"], button:has-text("Login"), input[type="submit"]').first();
            await loginButton.click();
            await page.waitForTimeout(3000);
        }

        // Step 5: Navigate to Dashboard or Interview
        console.log('📊 Step 5: Navigating to interview...');
        
        // Try to find and click interview/dashboard link
        const interviewLink = page.locator('a[href*="interview"], a[href*="dashboard"], a:has-text("Interview"), a:has-text("Dashboard")').first();
        if (await interviewLink.isVisible({ timeout: 5000 })) {
            await interviewLink.click();
            await page.waitForTimeout(2000);
        }

        // Step 6: Test Phase2 Interview
        console.log('🎯 Step 6: Testing Phase2 interview...');
        
        // Look for Phase2 interview elements
        const phase2Container = page.locator('#phase2-container, .phase2-interview, [data-testid="phase2"]').first();
        if (await phase2Container.isVisible({ timeout: 5000 })) {
            console.log('✅ Phase2 container found');
        } else {
            console.log('⚠️ Phase2 container not found, checking current page...');
            console.log('Current URL:', page.url());
            console.log('Current page title:', await page.title());
        }

        // Step 7: Check for question or error
        console.log('❓ Step 7: Checking for interview question...');
        
        // Wait for question to load
        await page.waitForTimeout(5000);
        
        // Check for error messages
        const errorMessage = page.locator(':has-text("Failed to load question"), :has-text("BadRequest"), .error, .alert-danger').first();
        const hasError = await errorMessage.isVisible({ timeout: 2000 });
        
        if (hasError) {
            const errorText = await errorMessage.textContent();
            console.error('❌ ERROR FOUND:', errorText);
            
            // Take screenshot for debugging
            await page.screenshot({ path: 'phase2-error.png', fullPage: true });
            
            // Log network activity
            const responses = [];
            page.on('response', response => {
                if (response.url().includes('phase2') || response.url().includes('interview')) {
                    responses.push({
                        url: response.url(),
                        status: response.status(),
                        statusText: response.statusText()
                    });
                }
            });
            
            throw new Error(`Phase2 interview failed with error: ${errorText}`);
        }

        // Check for successful question
        const questionElement = page.locator('.question, [data-testid="question"], h3, h4, p').filter({ hasText: /Are you looking to|What type of|Tell me about/ }).first();
        const hasQuestion = await questionElement.isVisible({ timeout: 2000 });
        
        if (hasQuestion) {
            const questionText = await questionElement.textContent();
            console.log('✅ SUCCESS: Question loaded:', questionText);
            expect(questionText).toBeTruthy();
            expect(questionText.length).toBeGreaterThan(10);
        } else {
            console.log('⚠️ No question found, checking page content...');
            const pageContent = await page.content();
            console.log('Page content preview:', pageContent.substring(0, 1000));
            
            // Take screenshot for debugging
            await page.screenshot({ path: 'phase2-no-question.png', fullPage: true });
            
            throw new Error('No interview question found on the page');
        }

        console.log('🎉 Phase2 interview test completed successfully!');
        
    }, 60000); // 60 second timeout for full test

    test('Direct API test for Phase2 endpoint', async () => {
        console.log('🔧 Testing Phase2 API endpoint directly...');
        
        // First register a user via API
        const testUser = generateTestUser('Immigrate');
        
        const registerResponse = await fetch('https://localhost:7280/api/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                firstName: 'APITest',
                lastName: 'User',
                email: testUser.email,
                password: testUser.password,
                category: 'Immigrate',
                phoneNumber: '+1234567890',
                city: 'Test City',
                state: 'CA',
                country: 'United States',
                dateOfBirth: '1990-01-01T00:00:00Z',
                immigrationGoal: 'Immigrate'
            })
        });

        console.log('Register response status:', registerResponse.status);
        expect(registerResponse.status).toBe(200);
        
        const registerData = await registerResponse.json();
        console.log('Register response:', registerData);
        
        const userId = registerData.userId || registerData.id;
        expect(userId).toBeTruthy();

        // Test Phase2 step endpoint
        const phase2Response = await fetch('https://localhost:7280/api/VisaInterview/phase2/step', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                userId: userId,
                category: 'Immigrate',
                answer: null // First request - should return initial question
            })
        });

        console.log('Phase2 response status:', phase2Response.status);
        console.log('Phase2 response headers:', Object.fromEntries(phase2Response.headers.entries()));
        
        if (phase2Response.status !== 200) {
            const errorText = await phase2Response.text();
            console.error('Phase2 API error:', errorText);
            throw new Error(`Phase2 API returned ${phase2Response.status}: ${errorText}`);
        }

        const phase2Data = await phase2Response.json();
        console.log('Phase2 response data:', phase2Data);
        
        expect(phase2Data.question).toBeTruthy();
        expect(phase2Data.question.length).toBeGreaterThan(10);
        expect(phase2Data.isComplete).toBe(false);
        
        console.log('✅ Phase2 API test passed!');
    }, 30000);
});