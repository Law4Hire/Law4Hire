const puppeteer = require('puppeteer');
const fs = require('fs');

// Configuration
const BASE_URL = 'http://localhost:5161';
const API_URL = 'http://localhost:5237';
const TEST_PASSWORD = 'SecureTest123!';

// Test user - will be created if doesn't exist
const TEST_USER = {
    email: 'phase2testuser@example.com',
    password: TEST_PASSWORD,
    firstName: 'Phase2',
    lastName: 'TestUser'
};

// Test Results Class
class TestResults {
    constructor() {
        this.results = [];
        this.totalTests = 0;
        this.passedTests = 0;
        this.failedTests = 0;
    }

    addResult(testName, passed, details, error = null) {
        this.results.push({
            testName,
            passed,
            details,
            error: error ? error.message : null,
            timestamp: new Date().toISOString()
        });
        
        this.totalTests++;
        if (passed) {
            this.passedTests++;
            console.log(`✅ ${testName}: PASSED - ${details}`);
        } else {
            this.failedTests++;
            console.log(`❌ ${testName}: FAILED - ${details}`);
            if (error) console.log(`   Error: ${error.message}`);
        }
    }

    generateReport() {
        const report = {
            summary: {
                total: this.totalTests,
                passed: this.passedTests,
                failed: this.failedTests,
                successRate: `${((this.passedTests / this.totalTests) * 100).toFixed(2)}%`
            },
            results: this.results,
            generatedAt: new Date().toISOString()
        };

        fs.writeFileSync('phase2-step-progression-results.json', JSON.stringify(report, null, 2));
        
        console.log('\n' + '='.repeat(60));
        console.log('PHASE 2 STEP PROGRESSION TEST RESULTS');
        console.log('='.repeat(60));
        console.log(`Total Tests: ${this.totalTests}`);
        console.log(`Passed: ${this.passedTests}`);
        console.log(`Failed: ${this.failedTests}`);
        console.log(`Success Rate: ${report.summary.successRate}`);
        console.log('='.repeat(60));
        
        return report;
    }
}

class Phase2StepProgressionTester {
    constructor() {
        this.browser = null;
        this.page = null;
        this.testResults = new TestResults();
    }

    async init() {
        console.log('Launching browser...');
        this.browser = await puppeteer.launch({ 
            headless: false,
            defaultViewport: { width: 1280, height: 720 },
            args: ['--no-sandbox', '--disable-setuid-sandbox'],
            slowMo: 300
        });
        this.page = await this.browser.newPage();
        
        // Enable console logging
        this.page.on('console', msg => {
            if (msg.type() === 'error') {
                console.log(`Browser Error: ${msg.text()}`);
            }
        });
        
        this.page.on('pageerror', error => {
            console.log(`Page Error: ${error.message}`);
        });
    }

    async cleanup() {
        if (this.browser) {
            await this.browser.close();
        }
    }

    // Login user
    async loginUser(email, password) {
        try {
            console.log(`Logging in user: ${email}`);
            
            await this.page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(2000);
            
            // Fill login form
            await this.page.waitForSelector('input[type="email"]');
            await this.page.type('input[type="email"]', email);
            await this.page.type('input[type="password"]', password);
            
            // Submit login
            await this.page.click('button[type="submit"]');
            await this.page.waitForTimeout(3000);
            
            // Check if login successful
            const url = this.page.url();
            return !url.includes('/login');
            
        } catch (error) {
            console.log(`Login failed: ${error.message}`);
            return false;
        }
    }

    // Go directly to Phase 2
    async goToPhase2() {
        try {
            console.log('Navigating directly to Phase 2...');
            await this.page.goto(`${BASE_URL}/interview/phase2`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(3000);
            return this.page.url().includes('phase2');
        } catch (error) {
            console.log(`Failed to navigate to Phase 2: ${error.message}`);
            return false;
        }
    }

    // Test button styling consistency
    async testButtonStyling() {
        try {
            console.log('Testing button styling consistency...');
            
            // Wait for buttons to load
            await this.page.waitForSelector('button.btn', { timeout: 10000 });
            
            // Get all option buttons
            const buttonInfo = await this.page.evaluate(() => {
                const buttons = Array.from(document.querySelectorAll('button.btn-outline-primary, button.btn-primary'));
                return buttons.map((btn, index) => {
                    const rect = btn.getBoundingClientRect();
                    const styles = window.getComputedStyle(btn);
                    return {
                        index,
                        width: rect.width,
                        height: rect.height,
                        minHeight: styles.minHeight,
                        text: btn.textContent?.trim(),
                        className: btn.className
                    };
                });
            });

            if (buttonInfo.length === 0) {
                this.testResults.addResult('Button Styling', false, 'No option buttons found');
                return false;
            }

            console.log(`Found ${buttonInfo.length} buttons:`);
            buttonInfo.forEach(btn => {
                console.log(`  Button ${btn.index + 1}: ${btn.width}x${btn.height} (${btn.minHeight}) - "${btn.text?.substring(0, 30)}"`);
            });

            // Check if all buttons have consistent min-height
            const minHeights = buttonInfo.map(btn => parseFloat(btn.minHeight));
            const uniqueMinHeights = [...new Set(minHeights)];
            
            if (uniqueMinHeights.length === 1 && uniqueMinHeights[0] >= 80) {
                this.testResults.addResult('Button Styling', true, `All buttons have consistent min-height: ${uniqueMinHeights[0]}px`);
                return true;
            } else {
                this.testResults.addResult('Button Styling', false, `Inconsistent button heights: ${uniqueMinHeights.join(', ')}`);
                return false;
            }

        } catch (error) {
            this.testResults.addResult('Button Styling', false, 'Error testing button styling', error);
            return false;
        }
    }

    // Test step progression without next button disappearing
    async testStepProgression() {
        try {
            console.log('Testing step progression...');
            
            let stepCount = 0;
            const maxSteps = 5;
            
            while (stepCount < maxSteps) {
                stepCount++;
                console.log(`Step ${stepCount}: Testing progression...`);
                
                // Wait for buttons to be available
                await this.page.waitForSelector('button.btn', { timeout: 10000 });
                
                // Take screenshot before action
                await this.page.screenshot({ path: `step-${stepCount}-before.png` });
                
                // Check if interview is complete
                const isComplete = await this.page.evaluate(() => {
                    const text = document.body.textContent || '';
                    return text.includes('Complete') || text.includes('Perfect') || text.includes('Dashboard');
                });
                
                if (isComplete) {
                    console.log('Interview completed');
                    this.testResults.addResult('Step Progression', true, `Interview completed successfully after ${stepCount - 1} steps`);
                    return true;
                }
                
                // Get available option buttons
                const optionButtons = await this.page.$$('button.btn-outline-primary, button.btn-primary');
                if (optionButtons.length === 0) {
                    this.testResults.addResult('Step Progression', false, `No option buttons found at step ${stepCount}`);
                    return false;
                }
                
                // Click the first option
                await optionButtons[0].click();
                console.log(`Clicked option button at step ${stepCount}`);
                
                // Wait for selection to register
                await this.page.waitForTimeout(1000);
                
                // Check if Next button appears
                const nextButtonExists = await this.page.evaluate(() => {
                    const nextButtons = Array.from(document.querySelectorAll('button'));
                    return nextButtons.some(btn => 
                        btn.textContent?.toLowerCase().includes('next') ||
                        btn.classList.contains('btn-success')
                    );
                });
                
                if (!nextButtonExists) {
                    this.testResults.addResult('Step Progression', false, `Next button not found after selecting option at step ${stepCount}`);
                    return false;
                }
                
                console.log('Next button found, clicking...');
                
                // Click Next button
                const nextClicked = await this.page.evaluate(() => {
                    const nextButtons = Array.from(document.querySelectorAll('button'));
                    const nextBtn = nextButtons.find(btn => 
                        btn.textContent?.toLowerCase().includes('next') ||
                        btn.classList.contains('btn-success')
                    );
                    
                    if (nextBtn && !nextBtn.disabled) {
                        nextBtn.click();
                        return true;
                    }
                    return false;
                });
                
                if (!nextClicked) {
                    this.testResults.addResult('Step Progression', false, `Failed to click Next button at step ${stepCount}`);
                    return false;
                }
                
                // Wait for next step to load
                await this.page.waitForTimeout(3000);
                
                // Take screenshot after action
                await this.page.screenshot({ path: `step-${stepCount}-after.png` });
                
                // Verify we progressed (step number should increase or complete)
                const newStepInfo = await this.page.evaluate(() => {
                    const stepElement = document.querySelector('h5');
                    const stepText = stepElement?.textContent || '';
                    const bodyText = document.body.textContent || '';
                    
                    return {
                        stepText,
                        isComplete: bodyText.includes('Complete') || bodyText.includes('Perfect') || bodyText.includes('Dashboard'),
                        hasQuestionBox: !!document.querySelector('.question-box, .card')
                    };
                });
                
                console.log(`After step ${stepCount}: ${JSON.stringify(newStepInfo)}`);
                
                if (newStepInfo.isComplete) {
                    console.log('Interview completed after clicking Next');
                    this.testResults.addResult('Step Progression', true, `Interview completed successfully after ${stepCount} steps`);
                    return true;
                }
                
                if (!newStepInfo.hasQuestionBox) {
                    this.testResults.addResult('Step Progression', false, `No question box found after step ${stepCount} progression`);
                    return false;
                }
            }
            
            this.testResults.addResult('Step Progression', false, `Interview did not complete after ${maxSteps} steps`);
            return false;
            
        } catch (error) {
            this.testResults.addResult('Step Progression', false, 'Error during step progression test', error);
            return false;
        }
    }

    // Register a new user for testing
    async registerTestUser() {
        try {
            console.log('Registering test user...');
            
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(2000);
            
            // Click on a category card to trigger registration
            const categoryClicked = await this.page.evaluate(() => {
                const cards = Array.from(document.querySelectorAll('.immigration-card, .mud-card'));
                if (cards.length > 0) {
                    cards[0].click();
                    return true;
                }
                return false;
            });
            
            if (!categoryClicked) return false;
            
            await this.page.waitForTimeout(3000);
            
            // Fill registration form
            const emailInput = await this.page.$('input[type="email"]');
            if (!emailInput) return false;
            
            await emailInput.type(TEST_USER.email);
            
            const passwordInput = await this.page.$('input[type="password"]');
            if (passwordInput) {
                await passwordInput.type(TEST_USER.password);
            }
            
            // Fill additional fields if present
            const firstNameInput = await this.page.$('input[name*="first"]');
            if (firstNameInput) {
                await firstNameInput.type(TEST_USER.firstName);
            }
            
            const lastNameInput = await this.page.$('input[name*="last"]');
            if (lastNameInput) {
                await lastNameInput.type(TEST_USER.lastName);
            }
            
            // Submit registration
            await this.page.click('button[type="submit"]');
            await this.page.waitForTimeout(5000);
            
            // Check if registration successful (not on login page)
            const url = this.page.url();
            return !url.includes('/login');
            
        } catch (error) {
            console.log(`Registration error: ${error.message}`);
            return false;
        }
    }

    // Test complete workflow
    async testCompleteWorkflow() {
        try {
            console.log('\n=== STARTING PHASE 2 STEP PROGRESSION TESTS ===\n');
            
            // Try login first, register if it fails
            let loggedIn = await this.loginUser(TEST_USER.email, TEST_USER.password);
            if (!loggedIn) {
                console.log('Login failed, trying registration...');
                const registered = await this.registerTestUser();
                if (registered) {
                    loggedIn = await this.loginUser(TEST_USER.email, TEST_USER.password);
                }
            }
            
            if (!loggedIn) {
                this.testResults.addResult('Login/Registration', false, 'Failed to login or register user');
                return this.testResults.generateReport();
            }
            this.testResults.addResult('Login/Registration', true, 'User authenticated successfully');
            
            // Navigate to Phase 2
            const phase2Started = await this.goToPhase2();
            if (!phase2Started) {
                this.testResults.addResult('Navigation', false, 'Failed to navigate to Phase 2');
                return false;
            }
            this.testResults.addResult('Navigation', true, 'Successfully navigated to Phase 2');
            
            // Test button styling
            await this.testButtonStyling();
            
            // Test step progression
            await this.testStepProgression();
            
            return this.testResults.generateReport();
            
        } catch (error) {
            console.error('Complete workflow test failed:', error);
            this.testResults.addResult('Complete Workflow', false, 'Unexpected error during complete workflow test', error);
            return this.testResults.generateReport();
        }
    }
}

// Main execution function
async function runPhase2StepProgressionTests() {
    const tester = new Phase2StepProgressionTester();
    
    try {
        await tester.init();
        console.log('Browser initialized successfully');
        
        const results = await tester.testCompleteWorkflow();
        
        console.log('\nTest execution completed!');
        console.log(`Results saved to: phase2-step-progression-results.json`);
        
        if (results.summary.failed > 0) {
            console.log('\n⚠️  FAILED TESTS:');
            results.results.filter(r => !r.passed).forEach(result => {
                console.log(`   ${result.testName}: ${result.details}`);
            });
        }
        
        return results;
        
    } catch (error) {
        console.error('❌ Test execution failed:', error);
        throw error;
    } finally {
        await tester.cleanup();
        console.log('Browser closed');
    }
}

// Run tests if this file is executed directly
if (require.main === module) {
    runPhase2StepProgressionTests()
        .then(results => {
            process.exit(results.summary.failed > 0 ? 1 : 0);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { Phase2StepProgressionTester, runPhase2StepProgressionTests };