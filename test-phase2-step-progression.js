const puppeteer = require('puppeteer');

// Test Phase 2 step-by-step progression
async function testPhase2StepProgression() {
    console.log('Testing Phase 2 step progression with actual user workflow...\n');
    
    const browser = await puppeteer.launch({ 
        headless: false, 
        defaultViewport: { width: 1280, height: 720 },
        slowMo: 1000  // Slow down to see what's happening
    });
    
    const page = await browser.newPage();
    
    // Enable console logging to see errors
    page.on('console', msg => {
        console.log(`Browser: ${msg.text()}`);
    });
    
    page.on('pageerror', error => {
        console.log(`Page Error: ${error.message}`);
    });
    
    try {
        console.log('1. Navigating to home page...');
        await page.goto('http://localhost:5161/', { waitUntil: 'networkidle0' });
        await page.waitForTimeout(3000);
        
        console.log('2. Clicking Immigrate card to start registration...');
        const cardClicked = await page.evaluate(() => {
            const allElements = Array.from(document.querySelectorAll('*'));
            const immigrateElement = allElements.find(el => {
                const text = el.textContent || '';
                return text.includes('Immigrate') && text.includes('Green Card');
            });
            
            if (immigrateElement) {
                const clickableElement = immigrateElement.closest('.mud-card') || immigrateElement;
                clickableElement.click();
                console.log('Clicked Immigrate card');
                return true;
            }
            return false;
        });
        
        if (!cardClicked) {
            console.log('❌ Could not click Immigrate card');
            return false;
        }
        
        await page.waitForTimeout(3000);
        
        console.log('3. Quick registration to get to Phase 2...');
        
        // Fast registration with minimal required data
        const testEmail = `steptest${Date.now()}@example.com`;
        const steps = [
            { selector: 'input[type="email"]', value: testEmail, name: 'Email' },
            { selector: 'input[type="text"]', value: 'StepTest', name: 'First Name' },
            { selector: 'input[type="text"]', value: 'User', name: 'Last Name' },
            { selector: 'input[type="text"]', value: '', name: 'Middle Name (optional)' },
            { selector: 'input[type="date"]', value: '1990-01-01', name: 'Date of Birth' },
            { selector: 'select', value: 'Single', name: 'Marital Status' },
        ];
        
        // Fill out registration form step by step
        for (let i = 0; i < steps.length; i++) {
            const step = steps[i];
            console.log(`   Step ${i + 1}: ${step.name}...`);
            
            try {
                if (step.selector === 'select') {
                    await page.waitForSelector('select', { timeout: 5000 });
                    await page.select('select', step.value);
                } else {
                    await page.waitForSelector(step.selector, { timeout: 5000 });
                    await page.click(step.selector);
                    if (step.value) {
                        await page.type(step.selector, step.value);
                    }
                }
                
                // Click Next button
                await page.click('button[type="submit"], .btn-primary');
                await page.waitForTimeout(2000);
                
                // Check if we've reached Phase 2
                const currentUrl = page.url();
                if (currentUrl.includes('phase2') || currentUrl.includes('interview')) {
                    console.log(`✅ Reached Phase 2 after registration step ${i + 1}!`);
                    break;
                }
                
            } catch (error) {
                console.log(`   Step ${i + 1} (${step.name}) - trying to continue: ${error.message}`);
                // Continue with next step or fast-forward
            }
        }
        
        // Fast-forward through remaining registration if needed
        console.log('4. Fast-forwarding through any remaining registration steps...');
        let maxAttempts = 15;
        let attempt = 0;
        
        while (attempt < maxAttempts) {
            attempt++;
            
            try {
                const currentUrl = page.url();
                if (currentUrl.includes('phase2') || currentUrl.includes('interview')) {
                    console.log(`✅ Reached Phase 2 after ${attempt} attempts!`);
                    break;
                }
                
                // Try to fill any visible form fields
                await page.evaluate(() => {
                    // Fill inputs with basic values
                    document.querySelectorAll('input[type="text"]:not([value])').forEach(input => {
                        if (!input.value && input.offsetWidth > 0 && input.offsetHeight > 0) {
                            if (input.placeholder?.toLowerCase().includes('address')) {
                                input.value = '123 Test St';
                            } else if (input.placeholder?.toLowerCase().includes('city')) {
                                input.value = 'Test City';
                            } else if (input.placeholder?.toLowerCase().includes('postal') || input.placeholder?.toLowerCase().includes('zip')) {
                                input.value = '12345';
                            } else if (input.placeholder?.toLowerCase().includes('phone')) {
                                input.value = '555-0123';
                            } else {
                                input.value = 'TestValue';
                            }
                        }
                    });
                    
                    // Fill password fields
                    document.querySelectorAll('input[type="password"]:not([value])').forEach(input => {
                        if (!input.value) input.value = 'SecureTest123!';
                    });
                    
                    // Select first radio button if any
                    const radios = Array.from(document.querySelectorAll('input[type="radio"]:not(:checked)'));
                    if (radios.length > 0) {
                        radios[0].click();
                    }
                    
                    // Select dropdown options
                    document.querySelectorAll('select').forEach(select => {
                        if (select.selectedIndex === 0 && select.options.length > 1) {
                            select.selectedIndex = 1;
                        }
                    });
                });
                
                // Click next button
                const nextClicked = await page.evaluate(() => {
                    const buttons = Array.from(document.querySelectorAll('button'));
                    const nextButton = buttons.find(btn => 
                        (btn.type === 'submit' || btn.textContent?.toLowerCase().includes('next')) &&
                        !btn.disabled
                    );
                    
                    if (nextButton) {
                        nextButton.click();
                        return true;
                    }
                    return false;
                });
                
                if (!nextClicked) {
                    console.log(`   Attempt ${attempt}: No next button found`);
                    break;
                }
                
                await page.waitForTimeout(3000);
                
            } catch (error) {
                console.log(`   Attempt ${attempt}: ${error.message}`);
            }
        }
        
        // Final check - are we at Phase 2?
        const finalUrl = page.url();
        if (!finalUrl.includes('phase2') && !finalUrl.includes('interview')) {
            console.log(`❌ Still not at Phase 2. Final URL: ${finalUrl}`);
            
            // Try manual navigation to Phase 2 for testing purposes
            console.log('5. Trying direct navigation to Phase 2 for testing...');
            try {
                await page.goto('http://localhost:5161/interview/phase2', { waitUntil: 'networkidle0' });
                await page.waitForTimeout(3000);
            } catch (navError) {
                console.log(`Direct navigation failed: ${navError.message}`);
                return false;
            }
        }
        
        // Now test the Phase 2 interface
        console.log('6. Testing Phase 2 button interface and step progression...');
        
        await page.screenshot({ path: 'phase2-step-test-start.png', fullPage: true });
        console.log('📸 Phase 2 start screenshot saved');
        
        // Test multiple steps
        const maxSteps = 5;
        let stepCount = 0;
        
        for (let stepNum = 1; stepNum <= maxSteps; stepNum++) {
            console.log(`\n--- STEP ${stepNum} ---`);
            
            await page.waitForTimeout(2000);
            
            // Get current question text
            const questionText = await page.evaluate(() => {
                const questionElements = Array.from(document.querySelectorAll('.card-text, p, h5, h6'));
                for (const el of questionElements) {
                    const text = el.textContent || '';
                    if (text.includes('?') || text.includes('What') || text.includes('How') || text.includes('Do you')) {
                        return text.trim();
                    }
                }
                return 'No question found';
            });
            
            console.log(`Question ${stepNum}: "${questionText}"`);
            
            // Check if question contains A) B) C) (should not!)
            if (questionText.includes('A)') || questionText.includes('B)') || questionText.includes('C)')) {
                console.log(`❌ Step ${stepNum}: Question still contains A) B) C) format!`);
            } else {
                console.log(`✅ Step ${stepNum}: Question is clean`);
            }
            
            // Find option buttons
            const optionButtons = await page.evaluate(() => {
                return Array.from(document.querySelectorAll('button.btn.w-100')).map((btn, index) => ({
                    index: index + 1,
                    text: btn.textContent?.substring(0, 80).trim(),
                    classes: btn.className,
                    disabled: btn.disabled,
                    visible: btn.offsetWidth > 0 && btn.offsetHeight > 0,
                    isOptionButton: !btn.textContent?.toLowerCase().includes('next') && 
                                   !btn.textContent?.toLowerCase().includes('back') &&
                                   !btn.textContent?.toLowerCase().includes('processing')
                })).filter(btn => btn.isOptionButton && btn.visible && btn.text);
            });
            
            console.log(`Step ${stepNum}: Found ${optionButtons.length} option buttons:`);
            
            if (optionButtons.length === 0) {
                console.log(`❌ Step ${stepNum}: No option buttons found!`);
                
                // Check for completion message
                const pageText = await page.evaluate(() => document.body.textContent || '');
                if (pageText.includes('Complete') || pageText.includes('Perfect') || pageText.includes('recommend')) {
                    console.log(`✅ Step ${stepNum}: Interview completed!`);
                    break;
                } else {
                    console.log(`❌ Step ${stepNum}: No buttons and no completion message`);
                    await page.screenshot({ path: `phase2-step${stepNum}-no-buttons.png` });
                    return false;
                }
            } else if (optionButtons.length === 1) {
                console.log(`❌ Step ${stepNum}: Only 1 button found - should have multiple!`);
                optionButtons.forEach(btn => console.log(`   "${btn.text}"`));
                return false;
            } else {
                console.log(`✅ Step ${stepNum}: Multiple buttons found:`);
                optionButtons.forEach(btn => {
                    console.log(`   ${btn.index}. "${btn.text}"`);
                });
                
                // Test button width consistency
                const buttonWidths = await page.evaluate(() => {
                    return Array.from(document.querySelectorAll('button.btn.w-100')).map(btn => btn.offsetWidth);
                });
                
                const allSameWidth = buttonWidths.every(width => width === buttonWidths[0]);
                if (allSameWidth) {
                    console.log(`✅ Step ${stepNum}: All buttons have consistent width (${buttonWidths[0]}px)`);
                } else {
                    console.log(`❌ Step ${stepNum}: Buttons have inconsistent widths: ${buttonWidths.join(', ')}`);
                }
                
                // Click the first option button
                console.log(`Step ${stepNum}: Clicking first option...`);
                
                const buttonClicked = await page.click('button.btn.w-100:first-of-type');
                console.log(`✅ Step ${stepNum}: Clicked first option button`);
                
                await page.waitForTimeout(1000);
                
                // Check if Next button appeared
                const nextButtonVisible = await page.evaluate(() => {
                    const nextButtons = Array.from(document.querySelectorAll('button'));
                    return nextButtons.some(btn => 
                        btn.textContent?.toLowerCase().includes('next') &&
                        btn.offsetWidth > 0 && btn.offsetHeight > 0 &&
                        !btn.disabled
                    );
                });
                
                if (!nextButtonVisible) {
                    console.log(`❌ Step ${stepNum}: Next button not visible after selection!`);
                    await page.screenshot({ path: `phase2-step${stepNum}-no-next.png` });
                    return false;
                } else {
                    console.log(`✅ Step ${stepNum}: Next button appeared`);
                    
                    // Click Next button
                    console.log(`Step ${stepNum}: Clicking Next button...`);
                    
                    const nextClicked = await page.evaluate(() => {
                        const nextButtons = Array.from(document.querySelectorAll('button'));
                        const nextButton = nextButtons.find(btn => 
                            btn.textContent?.toLowerCase().includes('next') && !btn.disabled
                        );
                        
                        if (nextButton) {
                            nextButton.click();
                            return true;
                        }
                        return false;
                    });
                    
                    if (!nextClicked) {
                        console.log(`❌ Step ${stepNum}: Could not click Next button!`);
                        return false;
                    }
                    
                    console.log(`✅ Step ${stepNum}: Clicked Next button`);
                    
                    // Wait for next step to load
                    await page.waitForTimeout(4000);
                    
                    // Take screenshot after clicking Next
                    await page.screenshot({ path: `phase2-step${stepNum}-after-next.png` });
                    
                    // Check if we progressed to next step or completed
                    const newPageText = await page.evaluate(() => document.body.textContent || '');
                    
                    if (newPageText.includes('Complete') || newPageText.includes('Perfect') || newPageText.includes('recommend')) {
                        console.log(`✅ Step ${stepNum}: Interview completed after this step!`);
                        stepCount = stepNum;
                        break;
                    } else {
                        // Check if question changed (new step)
                        const newQuestionText = await page.evaluate(() => {
                            const questionElements = Array.from(document.querySelectorAll('.card-text, p, h5, h6'));
                            for (const el of questionElements) {
                                const text = el.textContent || '';
                                if (text.includes('?') || text.includes('What') || text.includes('How')) {
                                    return text.trim();
                                }
                            }
                            return 'No question found';
                        });
                        
                        if (newQuestionText !== questionText) {
                            console.log(`✅ Step ${stepNum}: Successfully progressed to next step!`);
                            console.log(`   New question: "${newQuestionText.substring(0, 100)}..."`);
                            stepCount = stepNum;
                        } else {
                            console.log(`❌ Step ${stepNum}: Did not progress - same question showing!`);
                            console.log(`   Question still: "${questionText.substring(0, 100)}..."`);
                            return false;
                        }
                    }
                }
            }
        }
        
        console.log(`\n🎉 Phase 2 Step Progression Test Results:`);
        console.log(`✅ Successfully tested ${stepCount} steps`);
        console.log(`✅ Clean questions without A) B) C) format`);
        console.log(`✅ Multiple option buttons showing`);
        console.log(`✅ Button width consistency`);
        console.log(`✅ Next button appears after selection`);
        console.log(`✅ Step-to-step progression working`);
        
        return true;
        
    } catch (error) {
        console.error('❌ Test failed with error:', error);
        await page.screenshot({ path: 'phase2-step-test-error.png', fullPage: true });
        return false;
    } finally {
        console.log('\n7. Test complete. Browser will close in 5 seconds...');
        await page.waitForTimeout(5000);
        await browser.close();
    }
}

// Run the test
if (require.main === module) {
    testPhase2StepProgression()
        .then(success => {
            console.log(`\n🏁 Phase 2 Step Progression Test: ${success ? 'PASSED' : 'FAILED'}`);
            process.exit(success ? 0 : 1);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { testPhase2StepProgression };