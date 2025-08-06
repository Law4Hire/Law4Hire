const puppeteer = require('puppeteer');

async function runPhase2StepTest() {
    console.log('🚀 Running Final Phase 2 Step Progression Test...');
    
    const browser = await puppeteer.launch({
        headless: false,
        defaultViewport: { width: 1280, height: 720 },
        args: ['--ignore-certificate-errors', '--ignore-ssl-errors']
    });
    
    const page = await browser.newPage();
    
    try {
        // Go directly to Phase 2 page
        console.log('📍 Navigating to Phase 2 page...');
        await page.goto('http://localhost:5161/interview/phase2', { 
            waitUntil: 'networkidle0',
            timeout: 30000 
        });
        
        // Wait a moment for the page to load
        await page.waitForTimeout(3000);
        
        // Take screenshot of initial state
        await page.screenshot({ path: 'phase2-initial.png' });
        
        // Check if we're on the Phase 2 page
        const currentUrl = page.url();
        console.log(`📍 Current URL: ${currentUrl}`);
        
        if (!currentUrl.includes('phase2')) {
            console.log('❌ Not on Phase 2 page, might need authentication');
            
            // Try to go to home page and start interview
            await page.goto('http://localhost:5161/', { waitUntil: 'networkidle0' });
            await page.waitForTimeout(2000);
            
            // Look for and click a category card
            const cardClicked = await page.evaluate(() => {
                const cards = document.querySelectorAll('.immigration-card, .mud-card, [class*="card"]');
                if (cards.length > 0) {
                    cards[0].click();
                    return true;
                }
                return false;
            });
            
            if (!cardClicked) {
                console.log('❌ Could not find category cards on home page');
                return false;
            }
            
            await page.waitForTimeout(5000);
            console.log(`📍 After clicking card: ${page.url()}`);
        }
        
        // Look for Phase 2 content
        await page.waitForSelector('h3, h5, .question-box', { timeout: 10000 });
        
        // Check for Step 1
        const step1Info = await page.evaluate(() => {
            const stepElement = document.querySelector('h5');
            const stepText = stepElement?.textContent || '';
            const questionBox = document.querySelector('.question-box, .card');
            const buttons = document.querySelectorAll('button.btn-outline-primary, button.btn-primary');
            
            return {
                stepText,
                hasQuestionBox: !!questionBox,
                buttonCount: buttons.length,
                pageText: document.body.textContent.substring(0, 500)
            };
        });
        
        console.log('📊 Step 1 Info:', step1Info);
        
        if (!step1Info.hasQuestionBox || step1Info.buttonCount === 0) {
            console.log('❌ No question box or buttons found on Phase 2 page');
            await page.screenshot({ path: 'phase2-no-content.png' });
            return false;
        }
        
        // Check if we're at Step 1
        const isStep1 = step1Info.stepText.includes('Step 1') || step1Info.stepText.includes('1');
        console.log(`📊 Is Step 1: ${isStep1}`);
        
        // Click the first option button
        console.log('🖱️ Clicking first option button...');
        const optionClicked = await page.evaluate(() => {
            const buttons = document.querySelectorAll('button.btn-outline-primary, button.btn-primary');
            if (buttons.length > 0) {
                buttons[0].click();
                return true;
            }
            return false;
        });
        
        if (!optionClicked) {
            console.log('❌ Failed to click option button');
            return false;
        }
        
        // Wait for selection to register
        await page.waitForTimeout(2000);
        
        // Look for and click Next button
        console.log('🖱️ Looking for Next button...');
        const nextClicked = await page.evaluate(() => {
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
        
        if (!nextClicked) {
            console.log('❌ Next button not found or not clickable');
            await page.screenshot({ path: 'phase2-no-next-button.png' });
            return false;
        }
        
        console.log('✅ Next button clicked, waiting for Step 2...');
        
        // Wait for next step to load
        await page.waitForTimeout(5000);
        
        // Take screenshot after clicking Next
        await page.screenshot({ path: 'phase2-after-next.png' });
        
        // Check if we reached Step 2
        const step2Info = await page.evaluate(() => {
            const stepElement = document.querySelector('h5');
            const stepText = stepElement?.textContent || '';
            const questionBox = document.querySelector('.question-box, .card');
            const pageText = document.body.textContent;
            
            return {
                stepText,
                hasQuestionBox: !!questionBox,
                pageText: pageText.substring(0, 500),
                isComplete: pageText.includes('Complete') || pageText.includes('Perfect')
            };
        });
        
        console.log('📊 Step 2 Info:', step2Info);
        
        // Check if we're at Step 2
        const isStep2 = step2Info.stepText.includes('Step 2') || step2Info.stepText.includes('2');
        
        if (isStep2) {
            console.log('🎉 SUCCESS: Reached Step 2!');
            console.log('✅ Step progression is working correctly');
            return true;
        } else if (step2Info.isComplete) {
            console.log('🎉 SUCCESS: Interview completed (which means step progression worked)');
            return true;
        } else {
            console.log('❌ FAILED: Did not reach Step 2');
            console.log(`❌ Step text: "${step2Info.stepText}"`);
            console.log(`❌ Page content: "${step2Info.pageText}"`);
            return false;
        }
        
    } catch (error) {
        console.log('❌ Test failed with error:', error.message);
        await page.screenshot({ path: 'phase2-error.png' });
        return false;
    } finally {
        await browser.close();
    }
}

// Run the test
runPhase2StepTest().then(success => {
    if (success) {
        console.log('\n🎉 PHASE 2 STEP PROGRESSION TEST PASSED!');
        process.exit(0);
    } else {
        console.log('\n❌ PHASE 2 STEP PROGRESSION TEST FAILED!');
        process.exit(1);
    }
}).catch(error => {
    console.error('❌ Test execution failed:', error);
    process.exit(1);
});