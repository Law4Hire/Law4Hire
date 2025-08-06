// Simple test to actually verify the Phase2 interview process
const puppeteer = require('puppeteer');

async function testPhase2Interview() {
    let browser;
    let page;
    
    try {
        console.log('🚀 Starting ACTUAL Phase2 UI test...');
        
        browser = await puppeteer.launch({ 
            headless: false, // Show browser to see what's happening
            slowMo: 500,
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        });
        
        page = await browser.newPage();
        
        // Enable console logging from the page
        page.on('console', msg => console.log('PAGE LOG:', msg.text()));
        page.on('pageerror', error => console.error('PAGE ERROR:', error.message));
        page.on('response', response => {
            if (response.url().includes('api')) {
                console.log(`API CALL: ${response.status()} ${response.url()}`);
            }
        });
        
        // Step 1: Go to home page
        console.log('📝 Step 1: Going to home page...');
        await page.goto('http://localhost:5161', { waitUntil: 'networkidle2' });
        await page.waitForTimeout(2000);
        
        // Take screenshot for debugging
        await page.screenshot({ path: 'step1-home.png', fullPage: true });
        
        // Step 2: Click on Immigrate category to start registration
        console.log('📝 Step 2: Clicking Immigrate category button...');
        
        // First, let's see what's actually on the page
        const pageText = await page.evaluate(() => document.body.textContent || '');
        console.log('Page contains "Immigrate":', pageText.includes('Immigrate'));
        
        // Look for Immigrate category button - the user said registration happens by clicking category buttons
        let foundImmigrate = false;
        
        try {
            // Wait for the home page to fully load
            await page.waitForTimeout(3000);
            
            // Method 1: Look for MudCard elements (the actual immigration category cards)
            const cards = await page.$$('.mud-card, .immigration-card, .mud-paper');
            console.log(`Found ${cards.length} card elements`);
            
            for (const card of cards) {
                const text = await page.evaluate(el => el.textContent?.trim(), card);
                console.log('Card text:', text);
                if (text && (text.toLowerCase().includes('immigrate') || text.toLowerCase().includes('green card'))) {
                    console.log('Found Immigrate card with text:', text);
                    await card.click();
                    console.log('✅ Clicked Immigrate card');
                    foundImmigrate = true;
                    break;
                }
            }
            
            // Method 2: Look for any clickable element with "Immigrate" text
            if (!foundImmigrate) {
                const allElements = await page.$$('button, a, div, .mud-card, .mud-paper, [role="button"]');
                console.log(`Found ${allElements.length} total clickable elements`);
                
                for (const element of allElements) {
                    const text = await page.evaluate(el => el.textContent?.trim(), element);
                    if (text && (text.toLowerCase().includes('immigrate') || text.toLowerCase().includes('green card'))) {
                        console.log('Found Immigrate element with text:', text);
                        await element.click();
                        console.log('✅ Clicked Immigrate element');
                        foundImmigrate = true;
                        break;
                    }
                }
            }
            
            // Method 3: Try specific MudBlazor selectors
            if (!foundImmigrate) {
                const mudSelectors = [
                    '.mud-card:has-text("Immigrate")',
                    '.mud-card:has-text("Green Card")',
                    '.immigration-card:has-text("Immigrate")',
                    'div[style*="cursor: pointer"]:has-text("Immigrate")'
                ];
                
                for (const selector of mudSelectors) {
                    try {
                        await page.waitForSelector(selector, { timeout: 1000 });
                        await page.click(selector);
                        console.log('✅ Clicked Immigrate card with selector:', selector);
                        foundImmigrate = true;
                        break;
                    } catch (e) {
                        console.log('❌ MudBlazor selector not found:', selector);
                    }
                }
            }
        } catch (e) {
            console.log('❌ Error finding Immigrate button:', e.message);
        }
        
        if (!foundImmigrate) {
            console.error('❌ Could not find Immigrate category button');
            console.log('Available page text (first 500 chars):', pageText.substring(0, 500));
            
            // Log all clickable elements with text for debugging
            const allButtons = await page.$$('button, a, div[role="button"]');
            console.log('All clickable elements found:');
            for (let i = 0; i < Math.min(allButtons.length, 10); i++) {
                const text = await page.evaluate(el => el.textContent?.trim(), allButtons[i]);
                console.log(`  ${i+1}: "${text}"`);
            }
            
            return false;
        }
        
        await page.waitForTimeout(3000); // Wait for navigation/modal/form to appear
        await page.screenshot({ path: 'step2-after-immigrate-click.png', fullPage: true });
        
        // Step 3: Check what happens after clicking Immigrate - could be login, registration, or direct to interview
        console.log('📝 Step 3: Handling post-click flow...');
        const currentUrl = page.url();
        console.log('Current URL after click:', currentUrl);
        
        // Check if we're on login page, registration page, or interview page
        const pageContent = await page.content();
        const isLoginPage = (pageContent.includes('Client Login') || pageContent.includes('Welcome back')) && !pageContent.includes('Back to Options');
        const isRegistrationPage = pageContent.includes('register') || pageContent.includes('Register') || pageContent.includes('Sign Up');
        const isInterviewPage = currentUrl.includes('interview') || pageContent.includes('Interview') || pageContent.includes('Back to Options');
        
        console.log('Page analysis:');
        console.log('  Is Login Page:', isLoginPage);
        console.log('  Is Registration Page:', isRegistrationPage);
        console.log('  Is Interview Page:', isInterviewPage);
        
        // If we need to login/register, try to find existing test user or create new one
        if (isLoginPage && !isInterviewPage) {
            console.log('📝 On login page - trying to login with test user...');
            
            // Try to login with existing test user first (using MudBlazor field selectors)
            const loginFields = [
                { selector: 'input[type="email"], .mud-input-input-input[type="email"], input[aria-label*="Email"], input[aria-label*="email"]', value: 'immigratetest@testing.com' },
                { selector: 'input[type="password"], .mud-input-input-input[type="password"], input[aria-label*="Password"], input[aria-label*="password"]', value: 'SecureTest123!' }
            ];
            
            let canLogin = true;
            for (const field of loginFields) {
                try {
                    await page.waitForSelector(field.selector, { timeout: 2000 });
                    await page.type(field.selector, field.value);
                    console.log('✅ Filled login field:', field.selector);
                } catch (e) {
                    console.log('❌ Login field not found:', field.selector);
                    canLogin = false;
                    break;
                }
            }
            
            if (canLogin) {
                // Try to submit login (MudBlazor button selectors)
                const loginSubmitSelectors = [
                    'button[type="submit"]',
                    '.mud-button[type="submit"]',
                    'button:contains("Sign In")',
                    'button:contains("Login")',
                    '.mud-button:contains("Sign In")'
                ];
                
                let loginSubmitted = false;
                for (const selector of loginSubmitSelectors) {
                    try {
                        await page.click(selector);
                        console.log('✅ Clicked login submit with selector:', selector);
                        loginSubmitted = true;
                        break;
                    } catch (e) {
                        console.log('❌ Login submit selector not found:', selector);
                    }
                }
                
                if (loginSubmitted) {
                    await page.waitForTimeout(3000);
                    console.log('✅ Login attempt completed');
                } else {
                    console.log('❌ Could not submit login form');
                    return false;
                }
            } else {
                console.log('❌ Could not fill login form - may need to register first');
                
                // Let's debug what's actually on the page
                await page.screenshot({ path: 'debug-login-page.png', fullPage: true });
                
                // List all input fields
                const allInputs = await page.$$('input');
                console.log(`Found ${allInputs.length} input fields:`);
                for (let i = 0; i < allInputs.length; i++) {
                    const type = await page.evaluate(el => el.type, allInputs[i]);
                    const name = await page.evaluate(el => el.name, allInputs[i]);
                    const id = await page.evaluate(el => el.id, allInputs[i]);
                    const className = await page.evaluate(el => el.className, allInputs[i]);
                    console.log(`  Input ${i+1}: type="${type}", name="${name}", id="${id}", class="${className}"`);
                }
                
                // List all buttons
                const allButtons = await page.$$('button');
                console.log(`Found ${allButtons.length} buttons:`);
                for (let i = 0; i < allButtons.length; i++) {
                    const text = await page.evaluate(el => el.textContent?.trim(), allButtons[i]);
                    const type = await page.evaluate(el => el.type, allButtons[i]);
                    const className = await page.evaluate(el => el.className, allButtons[i]);
                    console.log(`  Button ${i+1}: text="${text}", type="${type}", class="${className}"`);
                }
                
                return false;
            }
        } else if (isRegistrationPage) {
            console.log('📝 On registration page - filling registration form...');
            
            const timestamp = Date.now();
            const testEmail = `immigratetest${timestamp}@testing.com`;
            
            // Fill registration form
            const regFields = [
                { name: 'FirstName', value: 'ImmigrateTest', selectors: ['input[name="FirstName"]', '#firstName', 'input[placeholder*="First"]'] },
                { name: 'LastName', value: 'User', selectors: ['input[name="LastName"]', '#lastName', 'input[placeholder*="Last"]'] },
                { name: 'Email', value: testEmail, selectors: ['input[name="Email"]', '#email', 'input[type="email"]'] },
                { name: 'Password', value: 'SecureTest123!', selectors: ['input[name="Password"]', '#password', 'input[type="password"]'] }
            ];
            
            for (const field of regFields) {
                let filled = false;
                for (const selector of field.selectors) {
                    try {
                        await page.waitForSelector(selector, { timeout: 2000 });
                        await page.type(selector, field.value);
                        console.log(`✅ Filled ${field.name} with selector: ${selector}`);
                        filled = true;
                        break;
                    } catch (e) {
                        console.log(`❌ ${field.name} selector not found: ${selector}`);
                    }
                }
                if (!filled) {
                    console.error(`❌ Could not fill ${field.name}`);
                }
            }
            
            // Submit registration
            const submitSelectors = [
                'button[type="submit"]',
                'input[type="submit"]',
                'button:contains("Register")',
                'button:contains("Sign Up")'
            ];
            
            let submitted = false;
            for (const selector of submitSelectors) {
                try {
                    await page.click(selector);
                    console.log('✅ Clicked register submit with selector:', selector);
                    submitted = true;
                    break;
                } catch (e) {
                    console.log('❌ Register submit selector not found:', selector);
                }
            }
            
            if (!submitted) {
                console.error('❌ Could not find registration submit button');
                return false;
            }
            
            await page.waitForTimeout(5000);
        } else if (isInterviewPage) {
            console.log('📝 Already in interview flow - filling registration form...');
            
            // We're in the interview registration flow - fill out the form step by step
            const timestamp = Date.now();
            const testEmail = `immigratetest${timestamp}@testing.com`;
            
            // Fill the email field (which we found in debugging)
            try {
                const emailInput = await page.waitForSelector('input[type="email"]', { timeout: 3000 });
                await emailInput.type(testEmail);
                console.log('✅ Filled email field');
                
                // Click Next to proceed
                const nextButton = await page.waitForSelector('button[type="submit"]', { timeout: 3000 });
                await nextButton.click();
                console.log('✅ Clicked Next button');
                
                await page.waitForTimeout(2000);
                
                // Now we should be on the first name step
                const firstNameInput = await page.waitForSelector('input[type="text"], .form-control', { timeout: 3000 });
                await firstNameInput.type('ImmigrateTest');
                console.log('✅ Filled first name');
                
                // Continue through more steps quickly
                for (let step = 0; step < 10; step++) {
                    try {
                        // Look for Next button and click it
                        const nextBtn = await page.waitForSelector('button[type="submit"]', { timeout: 2000 });
                        
                        // Check what field we're on and fill appropriate data
                        const currentInput = await page.$('input[type="text"], input[type="email"], select, input[type="date"], input[type="tel"]');
                        if (currentInput) {
                            const inputType = await page.evaluate(el => el.type, currentInput);
                            const placeholder = await page.evaluate(el => el.placeholder, currentInput);
                            console.log(`Step ${step + 1}: Found input type="${inputType}", placeholder="${placeholder}"`);
                            
                            // Fill appropriate test data based on the field
                            if (inputType === 'text') {
                                if (placeholder?.toLowerCase().includes('last')) {
                                    await currentInput.type('User');
                                } else if (placeholder?.toLowerCase().includes('city')) {
                                    await currentInput.type('Test City');
                                } else if (placeholder?.toLowerCase().includes('address')) {
                                    await currentInput.type('123 Test St');
                                } else if (placeholder?.toLowerCase().includes('postal') || placeholder?.toLowerCase().includes('zip')) {
                                    await currentInput.type('12345');
                                } else {
                                    await currentInput.type('TestValue');
                                }
                            } else if (inputType === 'email') {
                                await currentInput.type(`test${step}@example.com`);
                            } else if (inputType === 'date') {
                                await currentInput.type('1990-01-01');
                            } else if (inputType === 'tel') {
                                await currentInput.type('+1234567890');
                            } else if (inputType === 'password') {
                                await currentInput.type('SecureTest123!');
                            }
                        }
                        
                        // Handle select dropdowns
                        const currentSelect = await page.$('select');
                        if (currentSelect) {
                            const options = await page.$$eval('select option', options => 
                                options.map(option => option.value).filter(value => value && value !== '')
                            );
                            if (options.length > 0) {
                                await page.select('select', options[0]); // Select first available option
                                console.log(`Selected option: ${options[0]}`);
                            }
                        }
                        
                        // Handle radio buttons
                        const radioButtons = await page.$$('input[type="radio"]');
                        if (radioButtons.length > 0) {
                            await radioButtons[0].click(); // Click first radio option
                            console.log('Selected first radio option');
                        }
                        
                        await nextBtn.click();
                        console.log(`✅ Completed step ${step + 1}`);
                        await page.waitForTimeout(1000);
                        
                    } catch (e) {
                        console.log(`Step ${step + 1} complete or error:`, e.message);
                        break;
                    }
                }
                
            } catch (e) {
                console.log('❌ Error in interview flow:', e.message);
            }
        }
        
        await page.screenshot({ path: 'step3-after-auth.png', fullPage: true });
        
        // Step 4: Navigate to interview page if not already there
        console.log('📝 Step 4: Navigating to Phase2 interview...');
        const finalUrl = page.url();
        console.log('Current URL after auth:', finalUrl);
        
        if (!finalUrl.includes('interview/phase2')) {
            console.log('📝 Not on interview page yet, navigating...');
            
            // Try to find interview link first
            const interviewSelectors = [
                'a[href*="interview"]',
                'a[href*="phase2"]',
                'button[onclick*="interview"]',
                '.interview-btn',
                'a:contains("Interview")',
                'button:contains("Interview")'
            ];
            
            let foundInterviewLink = false;
            for (const selector of interviewSelectors) {
                try {
                    await page.waitForSelector(selector, { timeout: 2000 });
                    await page.click(selector);
                    console.log('✅ Found and clicked interview link with selector:', selector);
                    foundInterviewLink = true;
                    await page.waitForTimeout(2000);
                    break;
                } catch (e) {
                    console.log('❌ Interview link selector not found:', selector);
                }
            }
            
            if (!foundInterviewLink) {
                console.log('⚠️ No interview link found, trying direct navigation...');
                await page.goto('http://localhost:5161/interview/phase2', { waitUntil: 'networkidle2' });
            }
        }
        
        await page.waitForTimeout(3000);
        await page.screenshot({ path: 'step4-interview-page.png', fullPage: true });
        
        // Step 5: Check for Phase2 interview question or error
        console.log('📝 Step 5: Checking Phase2 interview...');
        
        // Wait for page to load
        await page.waitForTimeout(5000);
        
        // Check for error messages
        const errorSelectors = [
            ':has-text("Failed to load question")',
            ':has-text("BadRequest")',
            '.alert-danger',
            '.error',
            '[role="alert"]'
        ];
        
        let hasError = false;
        let errorText = '';
        
        for (const selector of errorSelectors) {
            try {
                const element = await page.waitForSelector(selector, { timeout: 2000 });
                if (element) {
                    errorText = await page.evaluate(el => el.textContent, element);
                    hasError = true;
                    console.error('❌ ERROR FOUND:', errorText);
                    break;
                }
            } catch (e) {
                // Continue checking other selectors
            }
        }
        
        // Check for successful question
        const questionSelectors = [
            '.question',
            '.card-text',
            'p:contains("Are you looking")',
            'p:contains("What type")',
            '[data-testid="question"]'
        ];
        
        let hasQuestion = false;
        let questionText = '';
        
        for (const selector of questionSelectors) {
            try {
                const element = await page.waitForSelector(selector, { timeout: 2000 });
                if (element) {
                    questionText = await page.evaluate(el => el.textContent, element);
                    if (questionText && questionText.length > 20) {
                        hasQuestion = true;
                        console.log('✅ QUESTION FOUND:', questionText);
                        break;
                    }
                }
            } catch (e) {
                // Continue checking other selectors
            }
        }
        
        // Final screenshot and analysis
        await page.screenshot({ path: 'step5-final-result.png', fullPage: true });
        
        // Get page content for analysis
        const finalPageContent = await page.content();
        console.log('\n📄 PAGE CONTENT ANALYSIS:');
        console.log('URL:', page.url());
        console.log('Title:', await page.title());
        
        if (hasError) {
            console.error('\n❌ TEST FAILED - ERROR FOUND:');
            console.error('Error message:', errorText);
            
            // Let's check what API calls are being made
            console.log('\n🔍 Investigating the error...');
            
            // Check network tab info
            const networkRequests = [];
            page.on('response', response => {
                if (response.url().includes('api')) {
                    networkRequests.push({
                        url: response.url(),
                        status: response.status(),
                        statusText: response.statusText()
                    });
                }
            });
            
            return false;
        } else if (hasQuestion) {
            console.log('\n✅ TEST PASSED - QUESTION LOADED:');
            console.log('Question:', questionText);
            return true;
        } else {
            console.error('\n❌ TEST FAILED - NO QUESTION OR ERROR FOUND');
            console.log('Page might still be loading or have different structure');
            
            // Log what we found on the page
            const visibleText = await page.evaluate(() => document.body.innerText);
            console.log('Visible text preview:', visibleText.substring(0, 500));
            
            return false;
        }
        
    } catch (error) {
        console.error('❌ Test failed with error:', error.message);
        if (page) {
            await page.screenshot({ path: 'error-screenshot.png', fullPage: true });
        }
        return false;
    } finally {
        if (browser) {
            await browser.close();
        }
    }
}

// Run the test
testPhase2Interview().then(success => {
    if (success) {
        console.log('\n🎉 Phase2 interview test PASSED!');
        process.exit(0);
    } else {
        console.log('\n💥 Phase2 interview test FAILED!');
        process.exit(1);
    }
}).catch(error => {
    console.error('Test runner error:', error);
    process.exit(1);
});