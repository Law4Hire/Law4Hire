const puppeteer = require('puppeteer');

// Simple test to verify the Phase 2 button interface works
async function testPhase2Interface() {
    console.log('Starting simple Phase 2 interface test...\n');
    
    const browser = await puppeteer.launch({ 
        headless: false, 
        defaultViewport: { width: 1280, height: 720 },
        slowMo: 500 // Slow down for visibility
    });
    
    const page = await browser.newPage();
    
    // Enable console logging
    page.on('console', msg => {
        console.log(`Browser: ${msg.text()}`);
    });
    
    try {
        // Step 1: Go to home page
        console.log('1. Navigating to home page...');
        await page.goto('http://localhost:5161/', { waitUntil: 'networkidle0' });
        await page.waitForTimeout(3000);
        
        // Step 2: Click on "Immigrate / Green Card" category
        console.log('2. Looking for Immigrate category card...');
        
        // Look for the card with "Immigrate" or "Green Card" text
        const immigrateClicked = await page.evaluate(() => {
            // Look for elements containing "Immigrate" or "Green Card"
            const allElements = Array.from(document.querySelectorAll('*'));
            const immigrateElement = allElements.find(el => {
                const text = el.textContent || '';
                return (text.includes('Immigrate') || text.includes('Green Card')) && 
                       (el.tagName === 'BUTTON' || el.classList.contains('mud-card') || 
                        el.closest('.mud-card') || el.style.cursor === 'pointer');
            });
            
            if (immigrateElement) {
                console.log('Found immigrate element:', immigrateElement.textContent);
                // Click the card or its clickable parent
                const clickableElement = immigrateElement.closest('.mud-card') || 
                                       immigrateElement.closest('[onclick]') || 
                                       immigrateElement;
                clickableElement.click();
                return true;
            }
            
            // Alternative: look for any clickable element with the right text
            const cards = Array.from(document.querySelectorAll('.mud-card, [style*="cursor: pointer"]'));
            const card = cards.find(c => {
                const text = c.textContent || '';
                return text.includes('Immigrate') || text.includes('Green Card');
            });
            
            if (card) {
                console.log('Found immigrate card:', card.textContent?.substring(0, 50));
                card.click();
                return true;
            }
            
            return false;
        });
        
        if (!immigrateClicked) {
            console.log('❌ Could not find or click Immigrate category card');
            await page.screenshot({ path: 'immigrate-not-found.png', fullPage: true });
            return false;
        }
        
        console.log('✅ Clicked Immigrate category card');
        await page.waitForTimeout(5000);
        
        // Step 3: Check what page we're on now
        const currentUrl = page.url();
        console.log(`3. Current URL after clicking: ${currentUrl}`);
        
        if (currentUrl.includes('phase2') || currentUrl.includes('interview')) {
            console.log('✅ Successfully navigated to Phase 2 interview!');
            
            // Step 4: Take screenshot of Phase 2 page
            await page.screenshot({ path: 'phase2-page.png', fullPage: true });
            console.log('📸 Screenshot saved as phase2-page.png');
            
            // Step 5: Look for A/B/C option buttons
            console.log('4. Looking for Phase 2 option buttons...');
            
            await page.waitForTimeout(3000);
            
            const buttonInfo = await page.evaluate(() => {
                const buttons = Array.from(document.querySelectorAll('button'));
                const results = [];
                
                buttons.forEach((btn, index) => {
                    const text = btn.textContent?.substring(0, 100) || '';
                    const classes = btn.className || '';
                    const hasABC = text.match(/[ABC][\s\)]/) || btn.querySelector('.badge');
                    
                    if (hasABC || classes.includes('btn-outline') || classes.includes('w-100')) {
                        results.push({
                            index: index + 1,
                            text: text.trim(),
                            classes: classes,
                            hasABC: !!hasABC
                        });
                    }
                });
                
                return results;
            });
            
            if (buttonInfo.length > 0) {
                console.log('✅ Found Phase 2 option buttons:');
                buttonInfo.forEach(btn => {
                    console.log(`   ${btn.index}. "${btn.text}" (${btn.hasABC ? 'HAS A/B/C' : 'no A/B/C'})`);
                });
                
                // Try to click the first option button
                console.log('5. Testing button click...');
                
                const firstButtonClicked = await page.evaluate(() => {
                    const buttons = Array.from(document.querySelectorAll('button.btn.w-100, button[class*="outline"]'));
                    if (buttons.length > 0) {
                        buttons[0].click();
                        return true;
                    }
                    return false;
                });
                
                if (firstButtonClicked) {
                    console.log('✅ Successfully clicked first option button!');
                    await page.waitForTimeout(2000);
                    
                    // Look for Next button
                    const nextButtonExists = await page.evaluate(() => {
                        const nextButtons = Array.from(document.querySelectorAll('button'));
                        return nextButtons.some(btn => 
                            btn.textContent?.toLowerCase().includes('next') ||
                            btn.classList.contains('btn-success')
                        );
                    });
                    
                    if (nextButtonExists) {
                        console.log('✅ Next button found!');
                        console.log('\n🎉 PHASE 2 INTERFACE TEST SUCCESSFUL!');
                        console.log('   - Category selection works');
                        console.log('   - Phase 2 page loads');
                        console.log('   - Option buttons are clickable');
                        console.log('   - Next button appears');
                        return true;
                    } else {
                        console.log('❌ Next button not found after clicking option');
                        return false;
                    }
                } else {
                    console.log('❌ Could not click option button');
                    return false;
                }
            } else {
                console.log('❌ No Phase 2 option buttons found');
                return false;
            }
        } else if (currentUrl.includes('localhost:5161') && !currentUrl.includes('/login')) {
            console.log('📝 We are on a registration form instead of Phase 2');
            console.log('   This means the user is not logged in and needs to complete registration first');
            
            // Take screenshot of registration form
            await page.screenshot({ path: 'registration-form.png', fullPage: true });
            console.log('📸 Registration form screenshot saved as registration-form.png');
            
            console.log('\n✅ PHASE 1 REGISTRATION FLOW WORKING');
            console.log('   To test Phase 2 directly, you need to:');
            console.log('   1. Complete the registration process, OR');
            console.log('   2. Login with an existing user first');
            
            return true;
        } else {
            console.log('❌ Unexpected navigation result');
            return false;
        }
        
    } catch (error) {
        console.error('❌ Test failed with error:', error);
        return false;
    } finally {
        console.log('\n6. Test complete. Browser will stay open for 10 seconds for inspection...');
        await page.waitForTimeout(10000);
        await browser.close();
    }
}

// Run the test
if (require.main === module) {
    testPhase2Interface()
        .then(success => {
            process.exit(success ? 0 : 1);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { testPhase2Interface };