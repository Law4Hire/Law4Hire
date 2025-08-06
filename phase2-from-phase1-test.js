const puppeteer = require('puppeteer');
const fs = require('fs');

// Configuration
const BASE_URL = 'http://localhost:5161';

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

        fs.writeFileSync('phase2-from-phase1-test-results.json', JSON.stringify(report, null, 2));
        
        console.log('\n' + '='.repeat(60));
        console.log('PHASE 2 (FROM PHASE 1) TEST RESULTS');
        console.log('='.repeat(60));
        console.log(`Total Tests: ${this.totalTests}`);
        console.log(`Passed: ${this.passedTests}`);
        console.log(`Failed: ${this.failedTests}`);
        console.log(`Success Rate: ${report.summary.successRate}`);
        console.log('='.repeat(60));
        
        return report;
    }
}

class Phase2FromPhase1Tester {
    constructor() {
        this.browser = null;
        this.page = null;
        this.testResults = new TestResults();
    }

    async init() {
        console.log('Launching browser for Phase 2 testing...');
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

    // Login with existing Phase 1 user
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

    // Start new interview from logged-in state
    async startNewInterview(category) {
        try {
            console.log(`Starting new interview for category: ${category}`);
            
            // Go to home page
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(2000);
            
            // Click category card
            const categoryClicked = await this.page.evaluate((cat) => {
                const cards = Array.from(document.querySelectorAll('.immigration-card, .mud-card'));
                const card = cards.find(c => {
                    const text = c.textContent || '';
                    return text.includes(cat) || 
                           (cat === 'Immigrate' && (text.includes('🏠') || text.includes('ImmigrateGreenCard'))) ||
                           (cat === 'Visit' && (text.includes('✈️') || text.includes('VisitUSA'))) ||
                           (cat === 'Work' && (text.includes('💼') || text.includes('WorkUSA'))) ||
                           (cat === 'Study' && (text.includes('📚') || text.includes('StudyUSA')));
                });
                
                if (card) {
                    card.click();
                    return true;
                }
                return false;
            }, category);

            if (!categoryClicked) {
                console.log(`Could not find category card for: ${category}`);
                return false;
            }

            await this.page.waitForTimeout(3000);
            
            // Check if we navigated to Phase 2
            const currentUrl = this.page.url();
            const isPhase2 = currentUrl.includes('phase2') || currentUrl.includes('interview');
            
            console.log(`After clicking card: URL = ${currentUrl}, isPhase2 = ${isPhase2}`);
            return isPhase2;
            
        } catch (error) {
            console.log(`Error starting interview: ${error.message}`);
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
                
                // Look for option buttons with the new button-based UI
                await this.page.waitForSelector('button', { timeout: 10000 });
                
                // Get buttons that look like A/B/C option buttons
                const optionButtons = await this.page.$$('button.btn.w-100, button[class*="btn-outline"], button[class*="btn-primary"]');
                
                if (optionButtons.length === 0) {
                    console.log('No option buttons found');
                    return false;
                }
                
                console.log(`Found ${optionButtons.length} option buttons`);
                
                // Look for buttons with A, B, C badges/labels
                const correctButton = await this.page.evaluate((targetIndex) => {
                    const buttons = Array.from(document.querySelectorAll('button.btn.w-100'));
                    const targetLetter = String.fromCharCode(65 + targetIndex); // A, B, C
                    
                    const button = buttons.find(btn => {
                        const text = btn.textContent || '';
                        return text.includes(targetLetter) && (
                            text.includes('badge') || 
                            btn.querySelector('.badge') || 
                            text.match(/^[ABC][\s\)]/) ||
                            btn.querySelector(`[class*="badge"]:contains("${targetLetter}")`)
                        );
                    });
                    
                    if (button) {
                        button.click();
                        return true;
                    }
                    return false;
                }, answerIndex);
                
                if (!correctButton && answerIndex < optionButtons.length) {
                    // Fallback to clicking by index if badge method doesn't work
                    const button = optionButtons[answerIndex];
                    await button.click();
                    console.log(`Clicked option ${answerIndex + 1} (fallback method)`);
                } else if (correctButton) {
                    console.log(`Clicked option ${String.fromCharCode(65 + answerIndex)} (badge method)`);
                } else {
                    console.log(`Answer index ${answerIndex} out of range for ${optionButtons.length} options`);
                    return false;
                }
                
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

    // Test a specific user with different answer patterns
    async testUserWithPatterns(userData) {
        const patterns = [
            [0], // A
            [1], // B
            [2], // C
            [0, 0], // A, A
            [0, 1], // A, B
            [1, 0], // B, A
            [1, 1], // B, B
        ];
        
        let successCount = 0;
        
        for (const pattern of patterns) {
            const testName = `${userData.category} - ${userData.email} - Pattern [${pattern.map(i => String.fromCharCode(65 + i)).join(',')}]`;
            
            try {
                console.log(`\n${'='.repeat(50)}`);
                console.log(`Testing: ${testName}`);
                console.log(`${'='.repeat(50)}`);
                
                // Login user
                const loggedIn = await this.loginUser(userData.email, userData.password);
                if (!loggedIn) {
                    this.testResults.addResult(testName, false, 'Failed to login user');
                    continue;
                }
                
                // Start new interview
                const interviewStarted = await this.startNewInterview(userData.category);
                if (!interviewStarted) {
                    this.testResults.addResult(testName, false, 'Failed to start interview');
                    continue;
                }
                
                // Execute interview
                const interviewCompleted = await this.executePhase2Interview(pattern);
                
                if (interviewCompleted) {
                    this.testResults.addResult(testName, true, `Successfully completed with pattern [${pattern.map(i => String.fromCharCode(65 + i)).join(',')}]`);
                    successCount++;
                } else {
                    this.testResults.addResult(testName, false, `Interview did not complete with pattern [${pattern.map(i => String.fromCharCode(65 + i)).join(',')}]`);
                }
                
            } catch (error) {
                this.testResults.addResult(testName, false, 'Unexpected error during test', error);
            }
            
            // Small delay between patterns
            await this.page.waitForTimeout(2000);
        }
        
        return successCount;
    }

    // Load Phase 1 test users and run Phase 2 tests
    async runPhase2Tests() {
        console.log('Starting Phase 2 tests using Phase 1 created users...\n');
        
        // Load test users created by Phase 1 tests
        let testUsers = [];
        try {
            const userData = fs.readFileSync('phase1-test-users.json', 'utf8');
            testUsers = JSON.parse(userData);
        } catch (error) {
            console.log('❌ Could not load phase1-test-users.json file');
            console.log('   Please run the Phase 1 tests first to create test users');
            console.log('   Command: node phase1-registration-test.js');
            return { summary: { failed: 1 } };
        }
        
        if (testUsers.length === 0) {
            console.log('❌ No test users found in phase1-test-users.json');
            console.log('   Please run the Phase 1 tests first to create test users');
            return { summary: { failed: 1 } };
        }
        
        console.log(`Found ${testUsers.length} test users from Phase 1 tests`);
        
        // Test a subset of users (to avoid very long test runs)
        const usersToTest = testUsers.slice(0, Math.min(3, testUsers.length));
        
        for (const userData of usersToTest) {
            console.log(`\n📧 Testing user: ${userData.email} (${userData.category})`);
            await this.testUserWithPatterns(userData);
        }
        
        return this.testResults.generateReport();
    }
}

// Main execution function
async function runPhase2FromPhase1Tests() {
    const tester = new Phase2FromPhase1Tester();
    
    try {
        await tester.init();
        console.log('Browser initialized successfully');
        
        const results = await tester.runPhase2FromPhase1Tests();
        
        console.log('\nPhase 2 test execution completed!');
        console.log(`Results saved to: phase2-from-phase1-test-results.json`);
        
        if (results.summary.failed > 0) {
            console.log('\n⚠️  FAILED TESTS:');
            results.results.filter(r => !r.passed).forEach(result => {
                console.log(`   ${result.testName}: ${result.details}`);
            });
        }
        
        return results;
        
    } catch (error) {
        console.error('❌ Phase 2 test execution failed:', error);
        throw error;
    } finally {
        await tester.cleanup();
        console.log('Browser closed');
    }
}

// Run tests if this file is executed directly
if (require.main === module) {
    (async () => {
        const tester = new Phase2FromPhase1Tester();
        
        try {
            await tester.init();
            const results = await tester.runPhase2Tests();
            
            await tester.cleanup();
            
            process.exit(results.summary.failed > 0 ? 1 : 0);
        } catch (error) {
            console.error('Fatal error:', error);
            await tester.cleanup();
            process.exit(1);
        }
    })();
}

module.exports = { Phase2FromPhase1Tester, runPhase2FromPhase1Tests };