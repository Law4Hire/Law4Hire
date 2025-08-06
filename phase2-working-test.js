const puppeteer = require('puppeteer');
const fs = require('fs');

// Configuration
const BASE_URL = 'http://localhost:5161';
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

        fs.writeFileSync('phase2-working-test-results.json', JSON.stringify(report, null, 2));
        
        console.log('\n' + '='.repeat(60));
        console.log('PHASE 2 WORKING TEST RESULTS');
        console.log('='.repeat(60));
        console.log(`Total Tests: ${this.totalTests}`);
        console.log(`Passed: ${this.passedTests}`);
        console.log(`Failed: ${this.failedTests}`);
        console.log(`Success Rate: ${report.summary.successRate}`);
        console.log('='.repeat(60));
        
        return report;
    }
}

class Phase2WorkingTester {
    constructor() {
        this.browser = null;
        this.page = null;
        this.testResults = new TestResults();
    }

    async init() {
        console.log('Launching browser...');
        this.browser = await puppeteer.launch({ 
            headless: false, // Show browser for debugging
            defaultViewport: { width: 1280, height: 720 },
            args: ['--no-sandbox', '--disable-setuid-sandbox'],
            slowMo: 300 // Slow down for visibility
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

    // Ensure user is logged in - try login first, then registration if needed
    async ensureUserLoggedIn() {
        // First try to login
        const loginSuccess = await this.loginUser(TEST_USER.email, TEST_USER.password);
        if (loginSuccess) {
            return true;
        }
        
        // If login failed, try to register
        console.log('Login failed, attempting registration...');
        const registerSuccess = await this.registerUser();
        if (registerSuccess) {
            // Now try to login again
            return await this.loginUser(TEST_USER.email, TEST_USER.password);
        }
        
        return false;
    }

    // Register new user through complete multi-step process
    async registerUser() {
        try {
            console.log(`Registering user: ${TEST_USER.email}`);
            
            // Step 1: Go to home page
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(2000);
            
            console.log('Step 1: On home page, clicking category card to start registration...');
            
            // Step 2: Click on a category card to trigger registration flow
            const categoryClicked = await this.page.evaluate(() => {
                const cards = Array.from(document.querySelectorAll('.immigration-card, .mud-card'));
                if (cards.length > 0) {
                    console.log('Found cards:', cards.length);
                    cards[0].click(); // Click first card (should be Visit USA)
                    return true;
                }
                return false;
            });
            
            if (!categoryClicked) {
                console.log('Failed to find category cards');
                return false;
            }
            
            await this.page.waitForTimeout(3000);
            
            console.log('Step 2: Category clicked, looking for registration form...');
            
            // Step 3: Fill in the registration form that appears
            const emailInput = await this.page.$('input[type="email"], input[name="email"]');
            if (!emailInput) {
                console.log('Email input not found');
                return false;
            }
            
            await emailInput.type(TEST_USER.email);
            console.log('Email entered');
            
            const passwordInput = await this.page.$('input[type="password"], input[name="password"]');
            if (passwordInput) {
                await passwordInput.type(TEST_USER.password);
                console.log('Password entered');
            }
            
            // Fill additional fields
            try {
                const firstNameInput = await this.page.$('input[name*="first"], input[name*="First"]');
                if (firstNameInput) {
                    await firstNameInput.type(TEST_USER.firstName);
                    console.log('First name entered');
                }
                
                const lastNameInput = await this.page.$('input[name*="last"], input[name*="Last"]');
                if (lastNameInput) {
                    await lastNameInput.type(TEST_USER.lastName);
                    console.log('Last name entered');
                }
            } catch (e) {
                console.log('Some optional fields not found');
            }
            
            console.log('Step 3: Form filled, submitting...');
            
            // Step 4: Submit the registration form
            await this.page.click('button[type="submit"], .btn-primary');
            await this.page.waitForTimeout(5000); // Give more time for processing
            
            console.log('Step 4: Form submitted, checking for next steps...');
            
            // Step 5: Check if we need to continue through multi-step interview
            // The home page registration might continue with interview questions
            let currentUrl = this.page.url();
            console.log(`Current URL after submission: ${currentUrl}`);
            
            // If we're still on home page or interview page, continue the process
            if (currentUrl.includes(BASE_URL) && !currentUrl.includes('/login')) {
                // Look for "Next" buttons or continue buttons and keep clicking them
                let maxSteps = 10; // Prevent infinite loop
                let stepCount = 0;
                
                while (stepCount < maxSteps) {
                    stepCount++;
                    console.log(`Registration step ${stepCount + 4}: Looking for next button...`);
                    
                    // Look for various types of "next" or "continue" buttons
                    const nextButton = await this.page.evaluate(() => {
                        const buttons = Array.from(document.querySelectorAll('button'));
                        const nextBtn = buttons.find(btn => {
                            const text = btn.textContent?.toLowerCase() || '';
                            return text.includes('next') || text.includes('continue') || 
                                   text.includes('submit') || text.includes('proceed') ||
                                   btn.type === 'submit' || btn.classList.contains('btn-primary');
                        });
                        
                        if (nextBtn && !nextBtn.disabled) {
                            nextBtn.click();
                            return true;
                        }
                        return false;
                    });
                    
                    if (nextButton) {
                        console.log(`Clicked next button in step ${stepCount + 4}`);
                        await this.page.waitForTimeout(3000);
                        
                        // Check if we've reached dashboard or completed registration
                        currentUrl = this.page.url();
                        if (currentUrl.includes('/dashboard') || currentUrl.includes('/phase2')) {
                            console.log('Registration completed - reached dashboard or phase2');
                            return true;
                        }
                    } else {
                        // No more next buttons found - check if registration is complete
                        console.log('No more next buttons found');
                        break;
                    }
                }
                
                // Final check - if we're not on login page, consider it successful
                currentUrl = this.page.url();
                const success = !currentUrl.includes('/login') && !currentUrl.includes('/error');
                console.log(`Registration final status: ${success ? 'SUCCESS' : 'FAILED'}, URL: ${currentUrl}`);
                return success;
            }
            
            return false;
            
        } catch (error) {
            console.log(`Registration error: ${error.message}`);
            return false;
        }
    }

    // Login with existing user
    async loginUser(email, password) {
        try {
            console.log(`Logging in user: ${email}`);
            
            await this.page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle0', timeout: 10000 });
            await this.page.waitForTimeout(2000);
            
            // Fill login form
            await this.page.waitForSelector('input[type="email"], input[name="email"]', { timeout: 5000 });
            await this.page.type('input[type="email"], input[name="email"]', email);
            
            await this.page.waitForSelector('input[type="password"], input[name="password"]', { timeout: 5000 });
            await this.page.type('input[type="password"], input[name="password"]', password);
            
            // Submit login
            await this.page.click('button[type="submit"], .btn-primary');
            await this.page.waitForTimeout(3000);
            
            // Check if login successful
            const url = this.page.url();
            const success = !url.includes('/login');
            
            if (success) {
                console.log('Login successful');
            } else {
                console.log('Login failed - still on login page');
            }
            
            return success;
            
        } catch (error) {
            console.log(`Login failed: ${error.message}`);
            return false;
        }
    }

    // Find and click category card
    async clickCategoryCard(categoryName) {
        try {
            console.log(`Looking for category card: ${categoryName}`);
            
            // Go to home page
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(2000);
            
            // Take screenshot for debugging
            await this.page.screenshot({ path: `category-search-${categoryName}.png` });
            
            // Look for category cards using multiple approaches
            let cardClicked = false;
            
            // Approach 1: Look for cards with immigration goal text
            const categoryMappings = {
                'Immigrate': ['ImmigrateGreenCard', 'Immigrate', '🏠'],
                'Work': ['WorkUSA', 'Work', '💼'],
                'Study': ['StudyUSA', 'Study', '📚'],
                'Family': ['JoinFamily', 'Family', '👨‍👩‍👧‍👦'],
                'Visit': ['VisitUSA', 'Visit', '✈️'],
                'Investment': ['InvestUSA', 'Investment', '💰'],
                'Asylum': ['ApplyAsylum', 'Asylum', '🛡️']
            };
            
            const searchTerms = categoryMappings[categoryName] || [categoryName];
            
            for (const term of searchTerms) {
                const clicked = await this.page.evaluate((searchTerm) => {
                    // Look for cards containing the search term
                    const cards = Array.from(document.querySelectorAll('.immigration-card, .mud-card, [class*="card"]'));
                    
                    const matchingCard = cards.find(card => {
                        const text = card.textContent || '';
                        return text.includes(searchTerm);
                    });
                    
                    if (matchingCard) {
                        matchingCard.click();
                        return true;
                    }
                    return false;
                }, term);
                
                if (clicked) {
                    console.log(`Found and clicked category card using term: ${term}`);
                    cardClicked = true;
                    break;
                }
            }
            
            // Approach 2: If not found, try looking for specific emoji or text patterns
            if (!cardClicked) {
                cardClicked = await this.page.evaluate((cat) => {
                    const allElements = Array.from(document.querySelectorAll('*'));
                    const element = allElements.find(el => {
                        const text = el.textContent || '';
                        return text.includes(cat) && 
                               (el.classList.contains('mud-card') || el.classList.contains('immigration-card') ||
                                el.closest('.mud-card') || el.getAttribute('style')?.includes('cursor: pointer'));
                    });
                    
                    if (element) {
                        // Click the card or its parent card
                        const card = element.closest('.mud-card') || element;
                        card.click();
                        return true;
                    }
                    return false;
                }, categoryName);
            }
            
            if (cardClicked) {
                await this.page.waitForTimeout(3000);
                
                // Check if we navigated to Phase 2
                const currentUrl = this.page.url();
                const isPhase2 = currentUrl.includes('phase2') || currentUrl.includes('interview');
                
                console.log(`After clicking card: URL = ${currentUrl}, isPhase2 = ${isPhase2}`);
                return isPhase2;
            } else {
                console.log(`Could not find category card for: ${categoryName}`);
                return false;
            }
            
        } catch (error) {
            console.log(`Error clicking category card: ${error.message}`);
            return false;
        }
    }

    // Execute Phase 2 interview with specific answer pattern
    async executePhase2Interview(answerPattern, maxQuestions = 8) {
        try {
            console.log(`Executing interview with pattern: [${answerPattern.map(i => String.fromCharCode(65 + i)).join(', ')}]`);
            
            let questionCount = 0;
            let interviewComplete = false;
            
            for (let i = 0; i < maxQuestions && i < answerPattern.length && !interviewComplete; i++) {
                questionCount++;
                const answerIndex = answerPattern[i]; // 0=A, 1=B, 2=C
                
                console.log(`Question ${questionCount}: Selecting option ${String.fromCharCode(65 + answerIndex)}`);
                
                // Wait for question to load
                await this.page.waitForTimeout(2000);
                
                // Take screenshot for debugging
                await this.page.screenshot({ path: `question-${questionCount}.png` });
                
                // Check for completion message first
                const completionText = await this.page.evaluate(() => {
                    return document.body.textContent || '';
                });
                
                if (completionText.includes('Complete') || completionText.includes('Perfect') || 
                    completionText.includes('recommend') || completionText.includes('workflow')) {
                    console.log('Interview already completed');
                    interviewComplete = true;
                    break;
                }
                
                // Look for option buttons
                await this.page.waitForSelector('button', { timeout: 10000 });
                
                // Get all buttons that look like option buttons
                const optionButtons = await this.page.$$('button.btn-outline-primary, button.btn-primary, button[class*="outline"], button[class*="primary"]');
                
                if (optionButtons.length === 0) {
                    console.log('No option buttons found');
                    // Try to find any clickable buttons
                    const allButtons = await this.page.$$('button');
                    console.log(`Found ${allButtons.length} total buttons on page`);
                    
                    // Log button details
                    for (let j = 0; j < Math.min(allButtons.length, 5); j++) {
                        const buttonText = await allButtons[j].evaluate(el => el.textContent?.substring(0, 50));
                        const buttonClass = await allButtons[j].evaluate(el => el.className);
                        console.log(`Button ${j + 1}: "${buttonText}" (class: ${buttonClass})`);
                    }
                    
                    return false;
                }
                
                console.log(`Found ${optionButtons.length} option buttons`);
                
                // Click the selected option
                if (answerIndex < optionButtons.length) {
                    const button = optionButtons[answerIndex];
                    await button.click();
                    console.log(`Clicked option ${answerIndex + 1}`);
                    
                    // Wait for selection to register
                    await this.page.waitForTimeout(1000);
                    
                    // Look for and click Next button
                    let nextClicked = false;
                    const nextSelectors = [
                        'button:contains("Next")',
                        '.btn-success',
                        'button[type="submit"]',
                        'button.btn-success'
                    ];
                    
                    for (const selector of nextSelectors) {
                        try {
                            const nextButton = await this.page.$(selector);
                            if (nextButton) {
                                await nextButton.click();
                                console.log(`Clicked Next button (${selector})`);
                                nextClicked = true;
                                break;
                            }
                        } catch (e) {
                            // Continue trying other selectors
                        }
                    }
                    
                    if (!nextClicked) {
                        // Try using JavaScript to find Next button
                        nextClicked = await this.page.evaluate(() => {
                            const buttons = Array.from(document.querySelectorAll('button'));
                            const nextButton = buttons.find(btn => 
                                btn.textContent?.toLowerCase().includes('next') ||
                                btn.classList.contains('btn-success')
                            );
                            
                            if (nextButton) {
                                nextButton.click();
                                return true;
                            }
                            return false;
                        });
                        
                        if (nextClicked) {
                            console.log('Clicked Next button via JavaScript');
                        }
                    }
                    
                    if (!nextClicked) {
                        console.log('Next button not found - checking if interview auto-progressed');
                    }
                    
                    // Wait for next question or completion
                    await this.page.waitForTimeout(3000);
                    
                    // Check for completion again
                    const newCompletionText = await this.page.evaluate(() => {
                        return document.body.textContent || '';
                    });
                    
                    if (newCompletionText.includes('Complete') || newCompletionText.includes('Perfect') || 
                        newCompletionText.includes('recommend') || newCompletionText.includes('Dashboard')) {
                        console.log('Interview completed after answering question');
                        interviewComplete = true;
                        break;
                    }
                    
                } else {
                    console.log(`Answer index ${answerIndex} out of range for ${optionButtons.length} options`);
                    return false;
                }
            }
            
            // Final check for completion
            if (!interviewComplete) {
                const finalText = await this.page.evaluate(() => {
                    return document.body.textContent || '';
                });
                
                interviewComplete = finalText.includes('Complete') || finalText.includes('Perfect') || 
                                   finalText.includes('recommend') || finalText.includes('Dashboard');
            }
            
            console.log(`Interview ${interviewComplete ? 'completed' : 'not completed'} after ${questionCount} questions`);
            return interviewComplete;
            
        } catch (error) {
            console.log(`Interview execution error: ${error.message}`);
            return false;
        }
    }

    // Test a single category with a specific answer pattern
    async testCategoryPattern(category, answerPattern) {
        const testName = `${category} - Pattern [${answerPattern.map(i => String.fromCharCode(65 + i)).join(',')}]`;
        
        try {
            console.log(`\n${'='.repeat(50)}`);
            console.log(`Testing: ${testName}`);
            console.log(`${'='.repeat(50)}`);
            
            // Login user (will try registration if login fails)
            const loggedIn = await this.ensureUserLoggedIn();
            if (!loggedIn) {
                this.testResults.addResult(testName, false, 'Failed to login user');
                return false;
            }
            
            // Click category card
            const categoryStarted = await this.clickCategoryCard(category);
            if (!categoryStarted) {
                this.testResults.addResult(testName, false, 'Failed to start category interview');
                return false;
            }
            
            // Execute interview
            const interviewCompleted = await this.executePhase2Interview(answerPattern);
            
            if (interviewCompleted) {
                this.testResults.addResult(testName, true, `Successfully completed with pattern [${answerPattern.map(i => String.fromCharCode(65 + i)).join(',')}]`);
                return true;
            } else {
                this.testResults.addResult(testName, false, `Interview did not complete with pattern [${answerPattern.map(i => String.fromCharCode(65 + i)).join(',')}]`);
                return false;
            }
            
        } catch (error) {
            this.testResults.addResult(testName, false, 'Unexpected error during test', error);
            return false;
        }
    }

    // Run focused tests
    async runFocusedTests() {
        console.log('Starting focused Phase 2 tests...\n');
        
        // Start with simple test cases - just one category to verify it works
        const testCases = [
            { category: 'Immigrate', pattern: [0] },      // A
            { category: 'Immigrate', pattern: [1] },      // B  
        ];
        
        for (const testCase of testCases) {
            await this.testCategoryPattern(testCase.category, testCase.pattern);
            
            // Small delay between tests
            await this.page.waitForTimeout(2000);
        }
        
        return this.testResults.generateReport();
    }
}

// Main execution function
async function runFocusedPhase2Tests() {
    const tester = new Phase2WorkingTester();
    
    try {
        await tester.init();
        console.log('Browser initialized successfully');
        
        const results = await tester.runFocusedTests();
        
        console.log('\nTest execution completed!');
        console.log(`Results saved to: phase2-working-test-results.json`);
        
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
    runFocusedPhase2Tests()
        .then(results => {
            process.exit(results.summary.failed > 0 ? 1 : 0);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { Phase2WorkingTester, runFocusedPhase2Tests };