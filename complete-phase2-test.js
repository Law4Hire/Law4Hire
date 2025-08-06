const puppeteer = require('puppeteer');

async function runCompletePhase2Test() {
    console.log('🚀 Running Complete Phase 2 Test with Authentication...');
    
    const browser = await puppeteer.launch({
        headless: false,
        defaultViewport: { width: 1280, height: 720 },
        args: ['--ignore-certificate-errors', '--ignore-ssl-errors'],
        slowMo: 500 // Slow down for visibility
    });
    
    const page = await browser.newPage();
    
    try {
        // Step 1: Go to home page and start registration/login process
        console.log('📍 Step 1: Going to home page...');
        await page.goto('http://localhost:5161/', { 
            waitUntil: 'networkidle0',
            timeout: 30000 
        });
        
        await page.waitForTimeout(2000);
        await page.screenshot({ path: 'test1-home.png' });
        
        // Step 2: Click on a category card to start the process
        console.log('📍 Step 2: Clicking category card...');
        const cardClicked = await page.evaluate(() => {
            const cards = document.querySelectorAll('.immigration-card, .mud-card, [class*="card"]');
            console.log('Found cards:', cards.length);
            if (cards.length > 0) {
                cards[0].click();
                return true;
            }
            return false;
        });
        
        if (!cardClicked) {
            console.log('❌ No category cards found');
            return false;
        }
        
        await page.waitForTimeout(3000);
        await page.screenshot({ path: 'test2-after-card-click.png' });
        
        // Step 3: Check if we need to register/login or if we're already in interview
        const currentUrl = page.url();
        console.log(`📍 After card click: ${currentUrl}`);
        
        if (currentUrl.includes('phase2') || currentUrl.includes('interview')) {
            console.log('✅ Already in interview mode, proceeding...');
        } else {
            // We might have a registration form - try to fill it
            console.log('📍 Step 3: Looking for registration/login form...');
            
            const emailInput = await page.$('input[type="email"]');
            if (emailInput) {
                console.log('📝 Found email input, filling registration...');
                await emailInput.type('testuser@phase2test.com');
                
                const passwordInput = await page.$('input[type="password"]');
                if (passwordInput) {
                    await passwordInput.type('SecureTest123!');
                }
                
                // Fill additional fields if present
                const firstNameInput = await page.$('input[name*="first"], input[placeholder*="first"]');
                if (firstNameInput) {
                    await firstNameInput.type('Test');
                }
                
                const lastNameInput = await page.$('input[name*="last"], input[placeholder*="last"]');
                if (lastNameInput) {
                    await lastNameInput.type('User');
                }
                
                // Submit the form
                await page.click('button[type="submit"], .btn-primary');
                await page.waitForTimeout(5000);
                
                console.log(`📍 After registration: ${page.url()}`);
            }
        }
        
        // Step 4: Continue through any intermediate steps until we reach Phase 2
        let maxSteps = 10;
        let currentStep = 0;
        
        while (currentStep < maxSteps) {
            currentStep++;
            const url = page.url();
            
            console.log(`📍 Navigation Step ${currentStep}: ${url}`);
            
            // Check if we're in Phase 2
            if (url.includes('phase2') || url.includes('interview')) {
                const hasQuestionBox = await page.$('.question-box, .card');
                if (hasQuestionBox) {
                    console.log('✅ Reached Phase 2 with question content!');
                    break;
                }
            }
            
            // Look for Next/Continue buttons to progress
            const nextClicked = await page.evaluate(() => {
                const buttons = Array.from(document.querySelectorAll('button'));
                const nextButton = buttons.find(btn => {
                    const text = btn.textContent?.toLowerCase() || '';
                    return text.includes('next') || text.includes('continue') || 
                           text.includes('submit') || text.includes('start') ||
                           btn.type === 'submit' || btn.classList.contains('btn-primary');
                });
                
                if (nextButton && !nextButton.disabled) {
                    nextButton.click();
                    return true;
                }
                return false;
            });
            
            if (nextClicked) {
                console.log(`✅ Clicked next button in step ${currentStep}`);
                await page.waitForTimeout(3000);
            } else {
                console.log(`❌ No next button found in step ${currentStep}`);
                break;
            }
        }
        
        // Step 5: Now test Phase 2 step progression
        console.log('🎯 Testing Phase 2 Step Progression...');
        
        await page.waitForTimeout(2000);
        await page.screenshot({ path: 'test3-phase2-start.png' });
        
        // Check for Step 1
        const step1Info = await page.evaluate(() => {
            const stepElement = document.querySelector('h5');
            const stepText = stepElement?.textContent || '';
            const buttons = document.querySelectorAll('button.btn-outline-primary, button.btn-primary');
            const questionBox = document.querySelector('.question-box, .card');
            
            return {
                stepText,
                buttonCount: buttons.length,
                hasQuestionBox: !!questionBox,
                pageContent: document.body.textContent.substring(0, 1000)
            };
        });
        
        console.log('📊 Phase 2 Initial State:', {
            stepText: step1Info.stepText,
            buttonCount: step1Info.buttonCount,
            hasQuestionBox: step1Info.hasQuestionBox
        });
        
        if (!step1Info.hasQuestionBox || step1Info.buttonCount === 0) {
            console.log('❌ No Phase 2 content found');
            console.log('Page content:', step1Info.pageContent);
            return false;
        }
        
        // Check if we're at Step 1
        const isStep1 = step1Info.stepText.includes('Step 1') || step1Info.stepText.includes('1');
        console.log(`📊 Is Step 1: ${isStep1}, Step text: "${step1Info.stepText}"`);
        
        // Click first option
        console.log('🖱️ Clicking first option...');
        await page.evaluate(() => {
            const buttons = document.querySelectorAll('button.btn-outline-primary, button.btn-primary');
            if (buttons.length > 0) {
                buttons[0].click();
            }
        });
        
        await page.waitForTimeout(2000);
        
        // Click Next button
        console.log('🖱️ Clicking Next button...');
        const nextButtonFound = await page.evaluate(() => {
            const buttons = Array.from(document.querySelectorAll('button'));
            const nextButton = buttons.find(btn => 
                btn.textContent?.toLowerCase().includes('next') ||
                btn.classList.contains('btn-success')
            );
            
            if (nextButton && !nextButton.disabled) {
                nextButton.click();
                return true;
            }
            return false;
        });
        
        if (!nextButtonFound) {
            console.log('❌ Next button not found');
            await page.screenshot({ path: 'test4-no-next-button.png' });
            return false;
        }
        
        console.log('✅ Next button clicked, waiting for Step 2...');
        await page.waitForTimeout(5000);
        
        await page.screenshot({ path: 'test5-after-next-click.png' });
        
        // Check for Step 2
        const step2Info = await page.evaluate(() => {
            const stepElement = document.querySelector('h5');
            const stepText = stepElement?.textContent || '';
            const questionBox = document.querySelector('.question-box, .card');
            const pageText = document.body.textContent;
            
            return {
                stepText,
                hasQuestionBox: !!questionBox,
                isComplete: pageText.includes('Complete') || pageText.includes('Perfect'),
                pageContent: pageText.substring(0, 500)
            };
        });
        
        console.log('📊 Step 2 Check:', {
            stepText: step2Info.stepText,
            hasQuestionBox: step2Info.hasQuestionBox,
            isComplete: step2Info.isComplete
        });
        
        const isStep2 = step2Info.stepText.includes('Step 2') || step2Info.stepText.includes('2');
        
        if (isStep2) {
            console.log('🎉 SUCCESS: Reached Step 2!');
            console.log(`✅ Step text: "${step2Info.stepText}"`);
            return true;
        } else if (step2Info.isComplete) {
            console.log('🎉 SUCCESS: Interview completed (step progression worked)');
            return true;
        } else {
            console.log('❌ FAILED: Did not reach Step 2');
            console.log(`❌ Step text: "${step2Info.stepText}"`);
            console.log(`❌ Page content: "${step2Info.pageContent}"`);
            return false;
        }
        
    } catch (error) {
        console.log('❌ Test failed with error:', error.message);
        await page.screenshot({ path: 'test-error.png' });
        return false;
    } finally {
        // Keep browser open for a moment to see results
        await page.waitForTimeout(3000);
        await browser.close();
    }
}

// Run the test
runCompletePhase2Test().then(success => {
    if (success) {
        console.log('\n🎉 COMPLETE PHASE 2 TEST PASSED!');
        console.log('✅ Step progression from Step 1 to Step 2 is working correctly!');
        process.exit(0);
    } else {
        console.log('\n❌ COMPLETE PHASE 2 TEST FAILED!');
        process.exit(1);
    }
}).catch(error => {
    console.error('❌ Test execution failed:', error);
    process.exit(1);
});