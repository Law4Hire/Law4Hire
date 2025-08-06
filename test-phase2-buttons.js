const puppeteer = require('puppeteer');

// Test the Phase 2 interface with a quick user login
async function testPhase2Buttons() {
    console.log('Testing Phase 2 button interface...\n');
    
    const browser = await puppeteer.launch({ 
        headless: false, 
        defaultViewport: { width: 1280, height: 720 },
        slowMo: 500
    });
    
    const page = await browser.newPage();
    
    // Enable console logging
    page.on('console', msg => {
        console.log(`Browser: ${msg.text()}`);
    });
    
    try {
        // Quick user creation and login simulation
        const testEmail = `buttontest${Date.now()}@example.com`;
        const testPassword = 'SecureTest123!';
        
        console.log('1. Creating a quick test user through registration...');
        
        // Go to home page
        await page.goto('http://localhost:5161/', { waitUntil: 'networkidle0' });
        await page.waitForTimeout(2000);
        
        // Click Immigrate card to start registration
        console.log('2. Clicking Immigrate card...');
        const cardClicked = await page.evaluate(() => {
            const allElements = Array.from(document.querySelectorAll('*'));
            const immigrateElement = allElements.find(el => {
                const text = el.textContent || '';
                return text.includes('Immigrate') && text.includes('Green Card');
            });
            
            if (immigrateElement) {
                const clickableElement = immigrateElement.closest('.mud-card') || immigrateElement;
                clickableElement.click();
                return true;
            }
            return false;
        });
        
        if (!cardClicked) {
            console.log('❌ Could not click Immigrate card');
            return false;
        }
        
        await page.waitForTimeout(3000);
        
        console.log('3. Filling minimal registration to get to Phase 2...');
        
        // Speed through registration with minimal data
        const steps = [
            // Email
            { type: 'input[type="email"]', value: testEmail },
            // First Name
            { type: 'input[type="text"]', value: 'TestUser' },
            // Last Name  
            { type: 'input[type="text"]', value: 'ButtonTest' },
            // Middle Name (optional)
            { type: 'input[type="text"]', value: '' },
            // Date of Birth
            { type: 'input[type="date"]', value: '1990-01-01' },
        ];
        
        let stepCount = 0;
        for (const step of steps) {
            stepCount++;
            try {
                console.log(`   Step ${stepCount}: Filling ${step.type} with "${step.value}"...`);
                
                await page.waitForSelector(step.type, { timeout: 5000 });
                await page.click(step.type);
                await page.evaluate((selector) => document.querySelector(selector).value = '', step.type);
                if (step.value) {
                    await page.type(step.type, step.value);
                }
                
                // Click Next button
                await page.click('button[type="submit"], .btn-primary');
                await page.waitForTimeout(2000);
                
                // Check if we've reached Phase 2
                const currentUrl = page.url();
                if (currentUrl.includes('phase2') || currentUrl.includes('interview')) {
                    console.log(`✅ Reached Phase 2 after step ${stepCount}!`);
                    break;
                }
                
            } catch (error) {
                console.log(`   Step ${stepCount} failed: ${error.message}`);
                // Continue trying - might need to skip some fields
            }
        }
        
        // Try to fast-forward through remaining steps
        console.log('4. Fast-forwarding through remaining registration steps...');
        for (let i = 0; i < 20; i++) {
            try {
                // Fill any visible inputs with default values
                await page.evaluate(() => {
                    const inputs = Array.from(document.querySelectorAll('input:not([type="hidden"]):not([type="submit"])'));
                    const visibleInput = inputs.find(input => {
                        const rect = input.getBoundingClientRect();
                        return rect.width > 0 && rect.height > 0 && !input.value;
                    });
                    
                    if (visibleInput) {
                        const type = visibleInput.type || 'text';
                        switch (type) {
                            case 'password':
                                visibleInput.value = 'SecureTest123!';
                                break;
                            case 'tel':
                                visibleInput.value = '555-0123';
                                break;
                            case 'date':
                                visibleInput.value = '1990-01-01';
                                break;
                            default:
                                if (visibleInput.placeholder?.toLowerCase().includes('address')) {
                                    visibleInput.value = '123 Test St';
                                } else if (visibleInput.placeholder?.toLowerCase().includes('city')) {
                                    visibleInput.value = 'Test City';
                                } else if (visibleInput.placeholder?.toLowerCase().includes('zip') || visibleInput.placeholder?.toLowerCase().includes('postal')) {
                                    visibleInput.value = '12345';
                                } else {
                                    visibleInput.value = 'TestValue';
                                }
                                break;
                        }
                    }
                    
                    // Select first radio button if any
                    const radios = Array.from(document.querySelectorAll('input[type="radio"]'));
                    if (radios.length > 0 && !radios.some(r => r.checked)) {
                        radios[0].click();
                    }
                    
                    // Select option in dropdowns
                    const selects = Array.from(document.querySelectorAll('select'));
                    selects.forEach(select => {
                        if (select.options.length > 1 && select.selectedIndex === 0) {
                            select.selectedIndex = 1;
                            select.dispatchEvent(new Event('change'));
                        }
                    });
                });
                
                // Click next button
                const nextClicked = await page.evaluate(() => {
                    const buttons = Array.from(document.querySelectorAll('button'));
                    const nextButton = buttons.find(btn => 
                        btn.type === 'submit' || 
                        btn.textContent?.toLowerCase().includes('next') ||
                        btn.classList.contains('btn-primary')
                    );
                    
                    if (nextButton && !nextButton.disabled) {
                        nextButton.click();
                        return true;
                    }
                    return false;
                });
                
                if (!nextClicked) {
                    console.log(`   No next button found at step ${i + 6}`);
                    break;
                }
                
                await page.waitForTimeout(3000);
                
                // Check if we've reached Phase 2 or Dashboard
                const currentUrl = page.url();
                if (currentUrl.includes('phase2') || currentUrl.includes('interview')) {
                    console.log(`✅ Reached Phase 2 at fast-forward step ${i + 6}!`);
                    break;
                }
                
                if (currentUrl.includes('dashboard')) {
                    console.log('📊 Reached dashboard - trying to start new interview...');
                    
                    // Go back to home and click Immigrate card as logged-in user
                    await page.goto('http://localhost:5161/', { waitUntil: 'networkidle0' });
                    await page.waitForTimeout(2000);
                    
                    const cardClickedAgain = await page.evaluate(() => {
                        const allElements = Array.from(document.querySelectorAll('*'));
                        const immigrateElement = allElements.find(el => {
                            const text = el.textContent || '';
                            return text.includes('Immigrate') && text.includes('Green Card');
                        });
                        
                        if (immigrateElement) {
                            const clickableElement = immigrateElement.closest('.mud-card') || immigrateElement;
                            clickableElement.click();
                            return true;
                        }
                        return false;
                    });
                    
                    if (cardClickedAgain) {
                        await page.waitForTimeout(3000);
                        const newUrl = page.url();
                        if (newUrl.includes('phase2')) {
                            console.log('✅ Now at Phase 2 as logged-in user!');
                            break;
                        }
                    }
                }
                
                console.log(`   Step ${i + 6}: Continuing...`);
                
            } catch (error) {
                console.log(`   Fast-forward step ${i + 6} error: ${error.message}`);
            }
        }
        
        // Final check - are we at Phase 2?
        await page.waitForTimeout(3000);
        const finalUrl = page.url();
        
        if (finalUrl.includes('phase2') || finalUrl.includes('interview')) {
            console.log('🎉 SUCCESS! We are at Phase 2. Now testing the button interface...');
            
            // Take screenshot of Phase 2 page
            await page.screenshot({ path: 'phase2-buttons-test.png', fullPage: true });
            
            // Test the button interface
            console.log('5. Testing Phase 2 button interface...');
            
            await page.waitForTimeout(3000);
            
            // Check the question text (should be clean without A) B) C))
            const questionText = await page.evaluate(() => {
                const questionElement = document.querySelector('.card-text, p');
                return questionElement ? questionElement.textContent : 'No question found';
            });
            
            console.log(`Question text: "${questionText}"`);
            
            if (questionText.includes('A)') || questionText.includes('B)') || questionText.includes('C)')) {
                console.log('❌ Question still contains A) B) C) format');
                return false;
            } else {
                console.log('✅ Question text is clean (no A) B) C) format)');
            }
            
            // Count option buttons
            const buttons = await page.evaluate(() => {
                const allButtons = Array.from(document.querySelectorAll('button.btn.w-100, button[class*="outline"], button[class*="primary"]'));
                return allButtons.map((btn, index) => ({
                    index: index + 1,
                    text: btn.textContent?.substring(0, 80).trim(),
                    classes: btn.className,
                    hasABC: !!(btn.textContent?.match(/[ABC][\s\)]/) || btn.querySelector('.badge')),
                    disabled: btn.disabled
                })).filter(btn => 
                    btn.text && 
                    !btn.text.toLowerCase().includes('next') && 
                    !btn.text.toLowerCase().includes('processing')
                );
            });
            
            console.log(`Found ${buttons.length} option buttons:`);
            
            if (buttons.length === 0) {
                console.log('❌ No option buttons found!');
                return false;
            } else if (buttons.length === 1) {
                console.log('❌ Only 1 button found - should have multiple options!');
                buttons.forEach(btn => {
                    console.log(`   Button: "${btn.text}" ${btn.hasABC ? '[HAS A/B/C]' : '[NO A/B/C]'}`);
                });
                return false;
            } else {
                console.log('✅ Multiple option buttons found:');
                buttons.forEach(btn => {
                    console.log(`   ${btn.index}. "${btn.text}" ${btn.hasABC ? '[HAS A/B/C BADGE]' : '[CLEAN TEXT]'} ${btn.disabled ? '[DISABLED]' : '[CLICKABLE]'}`);
                });
                
                // Try clicking the first button
                console.log('6. Testing button click and Next button appearance...');
                
                const firstButtonClicked = await page.evaluate(() => {
                    const optionButtons = Array.from(document.querySelectorAll('button.btn.w-100, button[class*="outline"]'));
                    const firstButton = optionButtons.find(btn => 
                        btn.textContent && 
                        !btn.textContent.toLowerCase().includes('next') &&
                        !btn.disabled
                    );
                    
                    if (firstButton) {
                        firstButton.click();
                        return true;
                    }
                    return false;
                });
                
                if (firstButtonClicked) {
                    console.log('✅ Successfully clicked first option button');
                    await page.waitForTimeout(2000);
                    
                    // Check for Next button
                    const nextButtonVisible = await page.evaluate(() => {
                        const nextButtons = Array.from(document.querySelectorAll('button'));
                        return nextButtons.some(btn => 
                            btn.textContent?.toLowerCase().includes('next') &&
                            btn.style.display !== 'none' &&
                            !btn.hidden &&
                            btn.offsetWidth > 0 &&
                            btn.offsetHeight > 0
                        );
                    });
                    
                    if (nextButtonVisible) {
                        console.log('✅ Next button appeared after selection!');
                        
                        console.log('\n🎉 PHASE 2 BUTTON INTERFACE TEST: SUCCESS!');
                        console.log('✅ Clean question text (no A) B) C) format)');
                        console.log('✅ Multiple option buttons showing');
                        console.log('✅ Buttons are clickable');
                        console.log('✅ Next button appears after selection');
                        console.log('📸 Screenshot saved as phase2-buttons-test.png');
                        
                        return true;
                    } else {
                        console.log('❌ Next button did not appear after selection');
                        return false;
                    }
                } else {
                    console.log('❌ Could not click first option button');
                    return false;
                }
            }
        } else {
            console.log(`❌ Did not reach Phase 2. Final URL: ${finalUrl}`);
            await page.screenshot({ path: 'phase2-not-reached.png', fullPage: true });
            console.log('📸 Screenshot saved as phase2-not-reached.png for debugging');
            return false;
        }
        
    } catch (error) {
        console.error('❌ Test failed with error:', error);
        await page.screenshot({ path: 'phase2-error.png', fullPage: true });
        return false;
    } finally {
        console.log('\n7. Test complete. Browser will close in 5 seconds...');
        await page.waitForTimeout(5000);
        await browser.close();
    }
}

// Run the test
if (require.main === module) {
    testPhase2Buttons()
        .then(success => {
            console.log(`\n🏁 Phase 2 Button Interface Test: ${success ? 'PASSED' : 'FAILED'}`);
            process.exit(success ? 0 : 1);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { testPhase2Buttons };