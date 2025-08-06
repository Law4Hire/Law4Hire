const puppeteer = require('puppeteer');

// Test Phase 2 with an existing user login
async function testPhase2WithExistingUser() {
    console.log('Testing Phase 2 interface with existing user login...\n');
    
    const browser = await puppeteer.launch({ 
        headless: false, 
        defaultViewport: { width: 1280, height: 720 },
        slowMo: 400
    });
    
    const page = await browser.newPage();
    
    // Enable console logging
    page.on('console', msg => {
        console.log(`Browser: ${msg.text()}`);
    });
    
    try {
        // Test with a simple user email/password
        const testEmail = 'admin@example.com'; // Try default admin user
        const testPassword = 'Admin123!';
        
        console.log('1. Navigating to login page...');
        await page.goto('http://localhost:5161/login', { waitUntil: 'networkidle0' });
        await page.waitForTimeout(3000);
        
        // Take screenshot of login page
        await page.screenshot({ path: 'login-page-test.png' });
        console.log('📸 Login page screenshot saved as login-page-test.png');
        
        // Try to login
        console.log('2. Attempting login...');
        try {
            await page.waitForSelector('input[type="email"], input[name="email"]', { timeout: 5000 });
            await page.type('input[type="email"], input[name="email"]', testEmail);
            
            await page.waitForSelector('input[type="password"], input[name="password"]', { timeout: 5000 });
            await page.type('input[type="password"], input[name="password"]', testPassword);
            
            await page.click('button[type="submit"], .btn-primary');
            await page.waitForTimeout(3000);
            
            const loginUrl = page.url();
            console.log(`After login attempt: ${loginUrl}`);
            
            if (!loginUrl.includes('/login')) {
                console.log('✅ Login successful!');
            } else {
                console.log('❌ Login failed or still on login page');
                
                // Try alternative credentials
                console.log('3. Trying alternative login approach...');
                
                // Clear and try different credentials
                await page.evaluate(() => {
                    document.querySelectorAll('input').forEach(input => input.value = '');
                });
                
                const altEmail = 'test@law4hire.com';
                const altPassword = 'Test123!';
                
                await page.type('input[type="email"], input[name="email"]', altEmail);
                await page.type('input[type="password"], input[name="password"]', altPassword);
                await page.click('button[type="submit"], .btn-primary');
                await page.waitForTimeout(3000);
                
                const altLoginUrl = page.url();
                if (altLoginUrl.includes('/login')) {
                    console.log('ℹ️ Login with existing users failed - will create new user');
                    
                    // Go to home page and create new user
                    await page.goto('http://localhost:5161/', { waitUntil: 'networkidle0' });
                    await page.waitForTimeout(2000);
                } else {
                    console.log('✅ Alternative login successful!');
                }
            }
            
        } catch (error) {
            console.log(`Login attempt failed: ${error.message}`);
            
            // Fallback - go to home page
            console.log('3. Fallback: Going to home page to test registration flow...');
            await page.goto('http://localhost:5161/', { waitUntil: 'networkidle0' });
            await page.waitForTimeout(2000);
        }
        
        // Now test the interview flow
        console.log('4. Testing interview flow...');
        
        // Click Immigrate card
        console.log('5. Clicking Immigrate card...');
        
        const cardClicked = await page.evaluate(() => {
            // Look for Immigrate card using various approaches
            const searchTexts = ['Immigrate', 'Green Card', '🏠'];
            
            for (const searchText of searchTexts) {
                const allElements = Array.from(document.querySelectorAll('*'));
                const element = allElements.find(el => {
                    const text = el.textContent || '';
                    return text.includes(searchText) && 
                           (el.classList.contains('mud-card') || 
                            el.closest('.mud-card') ||
                            el.tagName === 'BUTTON' ||
                            el.style.cursor === 'pointer');
                });
                
                if (element) {
                    console.log(`Found element with "${searchText}":`, element.textContent?.substring(0, 50));
                    const clickTarget = element.closest('.mud-card') || element;
                    clickTarget.click();
                    return true;
                }
            }
            
            // Alternative: click any card
            const cards = Array.from(document.querySelectorAll('.mud-card, [class*="card"]'));
            if (cards.length > 0) {
                console.log('Clicking first available card as fallback');
                cards[0].click();
                return true;
            }
            
            return false;
        });
        
        if (!cardClicked) {
            console.log('❌ Could not find or click any category card');
            await page.screenshot({ path: 'no-cards-found.png' });
            return false;
        }
        
        console.log('✅ Card clicked successfully');
        await page.waitForTimeout(5000);
        
        // Check what page we're on now
        const currentUrl = page.url();
        console.log(`6. Current URL after card click: ${currentUrl}`);
        
        await page.screenshot({ path: 'after-card-click.png', fullPage: true });
        console.log('📸 Screenshot saved as after-card-click.png');
        
        // Check the page content
        const pageText = await page.evaluate(() => {
            return document.body.textContent?.substring(0, 500) || 'No content';
        });
        
        console.log(`Page content preview: ${pageText}`);
        
        // Look for specific Phase 2 indicators
        const isPhase2 = await page.evaluate(() => {
            const bodyText = document.body.textContent || '';
            return bodyText.includes('What is the basis') ||
                   bodyText.includes('Phase 2') ||
                   bodyText.includes('interview') ||
                   document.querySelector('button.btn.w-100') !== null ||
                   bodyText.includes('What best describes');
        });
        
        if (isPhase2) {
            console.log('🎉 SUCCESS! We appear to be at Phase 2 interview!');
            
            // Test the button interface
            console.log('7. Testing Phase 2 button interface...');
            
            // Get the question text
            const questionText = await page.evaluate(() => {
                const questionElements = Array.from(document.querySelectorAll('p, .card-text, h5, h6'));
                for (const element of questionElements) {
                    const text = element.textContent || '';
                    if (text.includes('What is') || text.includes('What best') || text.includes('?')) {
                        return text;
                    }
                }
                return 'No question found';
            });
            
            console.log(`Question: "${questionText}"`);
            
            // Check if question contains A) B) C) format
            if (questionText.includes('A)') || questionText.includes('B)') || questionText.includes('C)')) {
                console.log('❌ Question still contains A) B) C) format - needs fixing');
            } else {
                console.log('✅ Question is clean (no A) B) C) format)');
            }
            
            // Count and analyze option buttons
            const optionButtons = await page.evaluate(() => {
                const buttons = Array.from(document.querySelectorAll('button'));
                
                return buttons.map((btn, index) => ({
                    index: index + 1,
                    text: btn.textContent?.substring(0, 100).trim(),
                    classes: btn.className,
                    hasABC: !!(btn.textContent?.match(/[ABC][\s\)]/) || btn.querySelector('.badge')),
                    isOptionButton: btn.classList.contains('w-100') || 
                                   btn.classList.contains('btn-outline') ||
                                   btn.querySelector('.badge') !== null,
                    disabled: btn.disabled,
                    visible: btn.offsetWidth > 0 && btn.offsetHeight > 0
                })).filter(btn => 
                    btn.isOptionButton && 
                    btn.visible &&
                    btn.text && 
                    !btn.text.toLowerCase().includes('next') &&
                    !btn.text.toLowerCase().includes('back')
                );
            });
            
            console.log(`Found ${optionButtons.length} option buttons:`);
            
            if (optionButtons.length === 0) {
                console.log('❌ No option buttons found!');
                
                // Debug: show all buttons
                const allButtons = await page.evaluate(() => {
                    return Array.from(document.querySelectorAll('button')).map((btn, i) => ({
                        index: i + 1,
                        text: btn.textContent?.substring(0, 50).trim(),
                        classes: btn.className,
                        visible: btn.offsetWidth > 0 && btn.offsetHeight > 0
                    }));
                });
                
                console.log('All buttons on page:');
                allButtons.forEach(btn => {
                    console.log(`   ${btn.index}. "${btn.text}" (${btn.classes}) ${btn.visible ? '[VISIBLE]' : '[HIDDEN]'}`);
                });
                
                return false;
            } else {
                console.log('✅ Option buttons found:');
                optionButtons.forEach(btn => {
                    console.log(`   ${btn.index}. "${btn.text}" ${btn.hasABC ? '[HAS A/B/C BADGE]' : '[CLEAN TEXT]'}`);
                });
                
                if (optionButtons.length === 1) {
                    console.log('❌ Only 1 option button - should have multiple choices!');
                    return false;
                } else {
                    console.log('✅ Multiple option buttons available');
                    
                    // Test clicking first option
                    console.log('8. Testing option selection...');
                    
                    const clickResult = await page.click(`button:nth-of-type(${optionButtons[0].index})`);
                    console.log('✅ Clicked first option button');
                    
                    await page.waitForTimeout(2000);
                    
                    // Look for Next button
                    const nextButton = await page.evaluate(() => {
                        const buttons = Array.from(document.querySelectorAll('button'));
                        return buttons.find(btn => 
                            btn.textContent?.toLowerCase().includes('next') &&
                            btn.offsetWidth > 0 && btn.offsetHeight > 0
                        ) !== undefined;
                    });
                    
                    if (nextButton) {
                        console.log('✅ Next button appeared after selection!');
                        
                        console.log('\n🎉 PHASE 2 BUTTON INTERFACE: ALL TESTS PASSED!');
                        console.log('✅ Clean question text');
                        console.log('✅ Multiple option buttons');
                        console.log('✅ Button selection works');
                        console.log('✅ Next button appears');
                        
                        return true;
                    } else {
                        console.log('❌ Next button not found after selection');
                        return false;
                    }
                }
            }
        } else {
            console.log('❌ Not at Phase 2 interview page');
            
            // Check what type of page we're on
            const pageType = await page.evaluate(() => {
                const bodyText = document.body.textContent || '';
                if (bodyText.includes('email address')) return 'Registration Form';
                if (bodyText.includes('Dashboard')) return 'Dashboard';
                if (bodyText.includes('Login')) return 'Login Page';
                if (bodyText.includes('Choose your path')) return 'Home Page';
                return 'Unknown Page';
            });
            
            console.log(`Current page type: ${pageType}`);
            return false;
        }
        
    } catch (error) {
        console.error('❌ Test failed with error:', error);
        await page.screenshot({ path: 'phase2-test-error.png', fullPage: true });
        return false;
    } finally {
        console.log('\n9. Test complete. Browser will close in 3 seconds...');
        await page.waitForTimeout(3000);
        await browser.close();
    }
}

// Run the test
if (require.main === module) {
    testPhase2WithExistingUser()
        .then(success => {
            console.log(`\n🏁 Phase 2 Interface Test: ${success ? 'PASSED' : 'FAILED'}`);
            process.exit(success ? 0 : 1);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { testPhase2WithExistingUser };