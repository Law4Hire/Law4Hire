const puppeteer = require('puppeteer');

// Test configuration
const BASE_URL = 'http://localhost:5161';
const TEST_USER = {
    email: 'jimmy@testing.com',
    password: 'SecureTest123!'
};

class Phase2Step2Test {
    constructor() {
        this.browser = null;
        this.page = null;
    }

    async init() {
        console.log('🚀 Starting Phase2Step2 Test...');
        this.browser = await puppeteer.launch({
            headless: false,
            defaultViewport: { width: 1280, height: 720 },
            args: ['--ignore-certificate-errors', '--ignore-ssl-errors'],
            slowMo: 300
        });
        this.page = await this.browser.newPage();
        
        // Enable console logging for debugging
        this.page.on('console', msg => {
            if (msg.type() === 'error') {
                console.log(`Browser Error: ${msg.text()}`);
            }
        });
    }

    async cleanup() {
        if (this.browser) {
            await this.browser.close();
        }
    }

    async loginUser() {
        console.log('📍 Step 1: Logging in user...');
        
        try {
            await this.page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(2000);
            
            // Fill login form
            await this.page.waitForSelector('input[type="email"]', { timeout: 10000 });
            await this.page.type('input[type="email"]', TEST_USER.email);
            
            await this.page.waitForSelector('input[type="password"]');
            await this.page.type('input[type="password"]', TEST_USER.password);
            
            // Submit login
            await this.page.click('button[type="submit"]');
            await this.page.waitForTimeout(3000);
            
            // Check if login successful
            const url = this.page.url();
            const success = !url.includes('/login');
            
            if (success) {
                console.log('✅ Login successful');
                return true;
            } else {
                console.log('❌ Login failed - still on login page');
                return false;
            }
            
        } catch (error) {
            console.log(`❌ Login failed: ${error.message}`);
            return false;
        }
    }

    async goToPhase2() {
        console.log('📍 Step 2: Navigating to Phase 2 page...');
        
        try {
            await this.page.goto(`${BASE_URL}/interview/phase2`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(3000);
            
            // Take screenshot for reference
            await this.page.screenshot({ path: 'phase2step2-initial.png' });
            
            // Check if we successfully reached Phase 2 with content
            const hasContent = await this.page.evaluate(() => {
                const questionBox = document.querySelector('.question-box, .card');
                const buttons = document.querySelectorAll('button.btn-outline-primary, button.btn-primary');
                const errorMessage = document.body.textContent.includes('not authenticated');
                
                return {
                    hasQuestionBox: !!questionBox,
                    buttonCount: buttons.length,
                    hasError: errorMessage
                };
            });
            
            if (hasContent.hasError) {
                console.log('❌ Authentication error on Phase 2 page');
                return false;
            }
            
            if (!hasContent.hasQuestionBox || hasContent.buttonCount === 0) {
                console.log('❌ No Phase 2 content found');
                return false;
            }
            
            console.log('✅ Successfully reached Phase 2 with content');
            return true;
            
        } catch (error) {
            console.log(`❌ Failed to reach Phase 2: ${error.message}`);
            return false;
        }
    }

    async testStepProgression() {
        console.log('📍 Step 3: Testing step progression from Step 1 to Step 2...');
        
        try {
            // Check initial step
            const initialStep = await this.page.evaluate(() => {
                const stepElement = document.querySelector('h5');
                const stepText = stepElement?.textContent || '';
                const buttons = document.querySelectorAll('button.btn-outline-primary, button.btn-primary');
                
                return {
                    stepText,
                    buttonCount: buttons.length,
                    currentStep: stepText.includes('1') ? 1 : (stepText.includes('2') ? 2 : 0)
                };
            });
            
            console.log(`📊 Initial state: ${initialStep.stepText}, ${initialStep.buttonCount} buttons`);
            
            if (initialStep.currentStep !== 1) {
                console.log(`❌ Expected to start at Step 1, but found: ${initialStep.stepText}`);
                return false;
            }
            
            // Select first option
            console.log('🖱️ Selecting first option...');
            await this.page.evaluate(() => {
                const buttons = document.querySelectorAll('button.btn-outline-primary, button.btn-primary');
                if (buttons.length > 0) {
                    buttons[0].click();
                }
            });
            
            await this.page.waitForTimeout(2000);
            
            // Look for and click Next button
            console.log('🖱️ Looking for Next button...');
            const nextButtonFound = await this.page.evaluate(() => {
                const buttons = Array.from(document.querySelectorAll('button'));
                const nextButton = buttons.find(btn => 
                    btn.textContent?.toLowerCase().includes('next') ||
                    btn.classList.contains('btn-success')
                );
                
                if (nextButton && !nextButton.disabled) {
                    console.log('Found Next button, clicking...');
                    nextButton.click();
                    return true;
                }
                return false;
            });
            
            if (!nextButtonFound) {
                console.log('❌ Next button not found or not clickable');
                await this.page.screenshot({ path: 'phase2step2-no-next.png' });
                return false;
            }
            
            console.log('✅ Next button clicked, waiting for Step 2...');
            await this.page.waitForTimeout(5000);
            
            // Take screenshot after clicking Next
            await this.page.screenshot({ path: 'phase2step2-after-next.png' });
            
            // Check if we reached Step 2
            const finalStep = await this.page.evaluate(() => {
                const stepElement = document.querySelector('h5');
                const stepText = stepElement?.textContent || '';
                const questionBox = document.querySelector('.question-box, .card');
                const pageText = document.body.textContent;
                
                return {
                    stepText,
                    hasQuestionBox: !!questionBox,
                    currentStep: stepText.includes('2') ? 2 : (stepText.includes('1') ? 1 : 0),
                    isComplete: pageText.includes('Complete') || pageText.includes('Perfect'),
                    pageContent: pageText.substring(0, 500)
                };
            });
            
            console.log(`📊 Final state: ${finalStep.stepText}`);
            
            if (finalStep.currentStep === 2) {
                console.log('🎉 SUCCESS: Reached Step 2!');
                console.log(`✅ Step text: "${finalStep.stepText}"`);
                return true;
            } else if (finalStep.isComplete) {
                console.log('🎉 SUCCESS: Interview completed (step progression worked through to completion)');
                return true;
            } else {
                console.log('❌ FAILED: Did not reach Step 2');
                console.log(`❌ Current step: ${finalStep.currentStep}`);
                console.log(`❌ Step text: "${finalStep.stepText}"`);
                console.log(`❌ Page content: "${finalStep.pageContent.substring(0, 200)}..."`);
                return false;
            }
            
        } catch (error) {
            console.log(`❌ Step progression test failed: ${error.message}`);
            await this.page.screenshot({ path: 'phase2step2-error.png' });
            return false;
        }
    }

    async runTest() {
        console.log('🎯 Phase2Step2 Test - Testing step progression from Step 1 to Step 2');
        console.log('=' .repeat(60));
        
        try {
            // Step 1: Login
            const loginSuccess = await this.loginUser();
            if (!loginSuccess) {
                console.log('❌ Test failed: Could not login');
                return false;
            }
            
            // Step 2: Go to Phase 2
            const phase2Success = await this.goToPhase2();
            if (!phase2Success) {
                console.log('❌ Test failed: Could not access Phase 2');
                return false;
            }
            
            // Step 3: Test step progression
            const progressionSuccess = await this.testStepProgression();
            if (!progressionSuccess) {
                console.log('❌ Test failed: Step progression did not work');
                return false;
            }
            
            console.log('🎉 Phase2Step2 Test PASSED!');
            console.log('✅ Successfully progressed from Step 1 to Step 2');
            return true;
            
        } catch (error) {
            console.log(`❌ Test execution failed: ${error.message}`);
            return false;
        }
    }
}

// Main execution
async function runPhase2Step2Test() {
    const test = new Phase2Step2Test();
    
    try {
        await test.init();
        const success = await test.runTest();
        
        if (success) {
            console.log('\n🎉 PHASE2STEP2 TEST COMPLETED SUCCESSFULLY!');
            console.log('✅ Step progression from Step 1 to Step 2 is working correctly!');
            return true;
        } else {
            console.log('\n❌ PHASE2STEP2 TEST FAILED!');
            return false;
        }
        
    } catch (error) {
        console.error('❌ Test execution failed:', error);
        return false;
    } finally {
        await test.cleanup();
        console.log('🏁 Test completed and browser closed');
    }
}

// Run the test if executed directly
if (require.main === module) {
    runPhase2Step2Test().then(success => {
        process.exit(success ? 0 : 1);
    }).catch(error => {
        console.error('Fatal error:', error);
        process.exit(1);
    });
}

module.exports = { Phase2Step2Test, runPhase2Step2Test };