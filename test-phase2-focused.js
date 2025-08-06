// Focused test to verify Phase2 interview loads without BadRequest error
const puppeteer = require('puppeteer');

async function testPhase2Focused() {
    let browser;
    let page;
    
    try {
        console.log('🚀 Starting FOCUSED Phase2 test...');
        
        browser = await puppeteer.launch({ 
            headless: false,
            slowMo: 300,
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        });
        
        page = await browser.newPage();
        
        // Enable console logging
        page.on('console', msg => console.log('PAGE LOG:', msg.text()));
        page.on('pageerror', error => console.error('PAGE ERROR:', error.message));
        page.on('response', response => {
            if (response.url().includes('api')) {
                console.log(`API CALL: ${response.status()} ${response.url()}`);
            }
        });
        
        // Step 1: Use testuser@example.com to test (we know this user exists)
        console.log('📝 Step 1: Using testuser@example.com user...');
        const testUser = {
            email: 'testuser@example.com', // Use the working test user
            password: 'TestPassword123!',
            category: 'Immigrate'
        };
        console.log('✅ Using existing test user credentials');
        
        // Step 2: Go to login page
        console.log('📝 Step 2: Going to login page...');
        await page.goto('http://localhost:5161/login', { waitUntil: 'networkidle2' });
        await page.waitForTimeout(2000);
        
        // Step 3: Login with test user
        console.log('📝 Step 3: Logging in with test user...');
        
        // Fill email - wait for MudBlazor input to be ready  
        await page.waitForSelector('input[type="email"]', { timeout: 5000 });
        await page.type('input[type="email"]', testUser.email);
        console.log('✅ Filled email');
        
        // Fill password - MudBlazor might use different selectors
        const passwordInputs = await page.$$('input[type="password"], input[type="text"]');
        let passwordFilled = false;
        for (const input of passwordInputs) {
            try {
                const isVisible = await page.evaluate(el => {
                    const rect = el.getBoundingClientRect();
                    return rect.width > 0 && rect.height > 0;
                }, input);
                
                if (isVisible) {
                    await input.type(testUser.password);
                    console.log('✅ Filled password');
                    passwordFilled = true;
                    break;
                }
            } catch (e) {
                continue;
            }
        }
        
        if (!passwordFilled) {
            console.log('❌ Could not fill password field');
            return false;
        }
        
        // Submit login
        const submitButton = await page.waitForSelector('button[type="submit"]', { timeout: 3000 });
        await submitButton.click();
        console.log('✅ Clicked login submit');
        
        // Wait for login to complete and check result
        await page.waitForTimeout(5000);
        await page.screenshot({ path: 'login-result.png', fullPage: true });
        
        // Check if login was successful
        const currentUrl = page.url();
        const pageText = await page.evaluate(() => document.body.innerText);
        const loginSuccess = currentUrl.includes('dashboard') || !pageText.includes('Invalid email or password') && !currentUrl.includes('login');
        
        console.log('Login check:');
        console.log('  Current URL:', currentUrl);
        console.log('  Login Success:', loginSuccess);
        
        if (!loginSuccess) {
            console.log('❌ Login failed, checking error message...');
            if (pageText.includes('Invalid email or password')) {
                console.log('❌ Invalid credentials - user may not exist');
                return false;
            }
        } else {
            console.log('✅ Login appears successful');
        }
        
        // Step 4: Navigate to Phase2 interview (simulate how user actually gets there)
        console.log('📝 Step 4: Navigating to Phase2 interview...');
        
        // For authenticated users, clicking Immigrate should redirect to Phase2
        // But first let's check what happens when we stay on dashboard and look for interview link
        let foundInterviewPath = false;
        
        // Check if there's a direct interview link in the dashboard
        try {
            const interviewLink = await page.$('a[href*="interview"], button[onclick*="interview"]');
            if (interviewLink) {
                await interviewLink.click();
                console.log('✅ Found and clicked interview link from dashboard');
                foundInterviewPath = true;
                await page.waitForTimeout(3000);
            }
        } catch (e) {
            console.log('No direct interview link found in dashboard');
        }
        
        // If no direct link, go to home and click Immigrate (for authenticated users this should go to Phase2)
        if (!foundInterviewPath) {
            await page.goto('http://localhost:5161/', { waitUntil: 'networkidle2' });
            await page.waitForTimeout(3000);
            
            const cards = await page.$$('.mud-card, .immigration-card');
            for (const card of cards) {
                const text = await page.evaluate(el => el.textContent?.trim(), card);
                if (text && (text.toLowerCase().includes('immigrate') || text.toLowerCase().includes('green card'))) {
                    await card.click();
                    console.log('✅ Clicked Immigrate card from authenticated session');
                    foundInterviewPath = true;
                    break;
                }
            }
        }
        
        // Wait for navigation and check URL
        await page.waitForTimeout(5000);
        const navigationUrl = page.url();
        console.log('Current URL after navigation:', navigationUrl);
        
        // If not on phase2, try direct navigation
        if (!navigationUrl.includes('phase2')) {
            console.log('⚠️ Not on Phase2 page, trying direct navigation...');
            await page.goto('http://localhost:5161/interview/phase2', { waitUntil: 'networkidle2' });
            await page.waitForTimeout(5000);
        }
        
        // Step 5: Check for errors or success
        console.log('📝 Step 5: Checking for Phase2 interview result...');
        await page.screenshot({ path: 'phase2-result.png', fullPage: true });
        
        // Get page content to check for errors
        const pageContent = await page.content();
        const phase2PageText = await page.evaluate(() => document.body.innerText);
        
        // Check for the specific error user reported
        const hasBadRequestError = phase2PageText.includes('Failed to load question') && phase2PageText.includes('BadRequest');
        const hasAuthError = phase2PageText.includes('User not authenticated') || phase2PageText.includes('Please log in');
        
        // Check for the EXACT question that should appear for Immigrate category
        const expectedQuestion = 'Are you looking to immigrate based on family relationships, employment opportunities, or investment?';
        const hasCorrectQuestion = phase2PageText.includes(expectedQuestion) || phase2PageText.includes('Are you looking to immigrate based on family');
        
        // Also check for any question at all as backup
        const hasAnyQuestion = phase2PageText.includes('Are you looking') || phase2PageText.includes('What type of') || phase2PageText.includes('Do you');
        
        // Check for API errors in network
        let apiErrors = [];
        page.on('response', response => {
            if (response.url().includes('api') && !response.ok()) {
                apiErrors.push(`${response.status()} ${response.url()}`);
            }
        });
        
        console.log('\n📄 PHASE2 INTERVIEW TEST RESULTS:');
        console.log('Current URL:', page.url());
        console.log('Has BadRequest Error:', hasBadRequestError);
        console.log('Has Auth Error:', hasAuthError);
        console.log('Has Correct Immigrate Question:', hasCorrectQuestion);
        console.log('Has Any Question:', hasAnyQuestion);
        console.log('API Errors:', apiErrors.length > 0 ? apiErrors : 'None');
        console.log('\nExpected Question:', expectedQuestion);
        console.log('\nFull page text preview:');
        console.log(phase2PageText.substring(0, 800));
        console.log('\n--- End page text ---');
        
        if (hasBadRequestError) {
            console.error('\n❌ TEST FAILED: Phase2 interview still shows BadRequest error');
            console.log('Error found in page text');
            return false;
        } else if (hasAuthError) {
            console.error('\n❌ TEST FAILED: Authentication issue preventing Phase2 access');
            console.log('User is not authenticated properly');
            return false;
        } else if (hasCorrectQuestion) {
            console.log('\n✅ TEST PASSED: Phase2 interview shows correct Immigrate question');
            console.log('Found expected question for Immigrate category');
            return true;
        } else if (hasAnyQuestion) {
            console.log('\n⚠️ TEST PARTIAL: Phase2 shows a question, but not the expected Immigrate question');
            console.log('Found some question, but may not be correct for category');
            return false;
        } else {
            console.error('\n❌ TEST FAILED: No question found at all');
            console.log('Expected to find the Immigrate category question but found none');
            return false;
        }
        
    } catch (error) {
        console.error('❌ Test failed with error:', error.message);
        if (page) {
            await page.screenshot({ path: 'error-focused-test.png', fullPage: true });
        }
        return false;
    } finally {
        if (browser) {
            await browser.close();
        }
    }
}

// Run the focused test
testPhase2Focused().then(success => {
    if (success) {
        console.log('\n🎉 FOCUSED Phase2 test PASSED! BadRequest error is FIXED!');
        process.exit(0);  
    } else {
        console.log('\n💥 FOCUSED Phase2 test FAILED! BadRequest error still exists!');
        process.exit(1);
    }
}).catch(error => {
    console.error('Test runner error:', error);
    process.exit(1);
});