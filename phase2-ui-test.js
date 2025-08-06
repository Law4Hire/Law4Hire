const puppeteer = require('puppeteer');
const fs = require('fs');

// Configuration
const BASE_URL = 'http://localhost:5161';
const TEST_PASSWORD = 'SecureTest123!';

// Test data
const COUNTRIES = [
    'United States', 'Canada', 'United Kingdom', 'Germany', 'France', 'Italy', 'Spain', 
    'Australia', 'Japan', 'South Korea', 'China', 'India', 'Brazil', 'Mexico'
];

const CATEGORIES = ['Immigrate', 'Work', 'Study', 'Family', 'Visit', 'Investment', 'Asylum'];

// Generate random birthdate (22-70 years old)
function generateRandomBirthdate() {
    const today = new Date();
    const minAge = 22;
    const maxAge = 70;
    const age = Math.floor(Math.random() * (maxAge - minAge + 1)) + minAge;
    const birthYear = today.getFullYear() - age;
    const month = Math.floor(Math.random() * 12) + 1;
    const day = Math.floor(Math.random() * 28) + 1;
    return `${birthYear}-${month.toString().padStart(2, '0')}-${day.toString().padStart(2, '0')}`;
}

function getRandomCountry() {
    return COUNTRIES[Math.floor(Math.random() * COUNTRIES.length)];
}

// Generate test user
function generateTestUser(testNumber) {
    return {
        email: `phase2test${testNumber}@example.com`,
        password: TEST_PASSWORD,
        firstName: `Test${testNumber}`,
        lastName: 'User',
        phone: `555-${testNumber.toString().padStart(4, '0')}`,
        birthdate: generateRandomBirthdate(),
        country: getRandomCountry()
    };
}

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

        fs.writeFileSync('phase2-ui-test-results.json', JSON.stringify(report, null, 2));
        
        console.log('\n' + '='.repeat(60));
        console.log('PHASE 2 UI INTERVIEW TEST RESULTS');
        console.log('='.repeat(60));
        console.log(`Total Tests: ${this.totalTests}`);
        console.log(`Passed: ${this.passedTests}`);
        console.log(`Failed: ${this.failedTests}`);
        console.log(`Success Rate: ${report.summary.successRate}`);
        console.log('='.repeat(60));
        
        return report;
    }
}

// Main Phase 2 Tester
class Phase2UITester {
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
            slowMo: 100 // Slow down for visibility
        });
        this.page = await this.browser.newPage();
        
        // Enable console logging
        this.page.on('console', msg => {
            console.log(`Browser: ${msg.text()}`);
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

    // Check if user exists and login, or create new user
    async setupTestUser(userData) {
        try {
            console.log(`Setting up test user: ${userData.email}`);
            
            // Try to login first
            await this.page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle0', timeout: 10000 });
            
            // Check if login form exists
            const emailInput = await this.page.$('input[type="email"], input[name="email"]');
            if (emailInput) {
                await emailInput.type(userData.email);
                
                const passwordInput = await this.page.$('input[type="password"], input[name="password"]');
                if (passwordInput) {
                    await passwordInput.type(userData.password);
                    
                    // Click login button
                    await this.page.click('button[type="submit"], .btn-primary');
                    await this.page.waitForTimeout(3000);
                    
                    // Check if login successful
                    const url = this.page.url();
                    if (!url.includes('/login')) {
                        console.log('Login successful');
                        return true;
                    }
                }
            }
            
            // If login failed, try registration
            console.log('Login failed, attempting registration...');
            return await this.registerNewUser(userData);
            
        } catch (error) {
            console.log(`Setup user failed: ${error.message}`);
            return false;
        }
    }

    async registerNewUser(userData) {
        try {
            // Go to home page and look for registration
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            
            // Look for registration/signup button/link
            await this.page.waitForTimeout(2000);
            
            // Try multiple ways to find registration
            const registrationFound = await this.page.evaluate(() => {
                // Look for common registration patterns
                const patterns = [
                    'register', 'sign up', 'signup', 'create account', 'join'
                ];
                
                for (const pattern of patterns) {
                    const elements = Array.from(document.querySelectorAll('a, button'));
                    const element = elements.find(el => 
                        el.textContent.toLowerCase().includes(pattern) ||
                        el.href?.toLowerCase().includes(pattern)
                    );
                    
                    if (element) {
                        element.click();
                        return true;
                    }
                }
                return false;
            });
            
            if (!registrationFound) {
                console.log('Could not find registration link');
                return false;
            }
            
            // Wait for registration form
            await this.page.waitForTimeout(2000);
            
            // Fill registration form
            const emailInput = await this.page.$('input[type="email"], input[name="email"]');
            if (emailInput) {
                await emailInput.type(userData.email);
            }
            
            const passwordInput = await this.page.$('input[type="password"], input[name="password"]');
            if (passwordInput) {
                await passwordInput.type(userData.password);
            }
            
            // Fill additional fields if present
            try {
                const firstNameInput = await this.page.$('input[name*="first"], input[name*="First"]');
                if (firstNameInput) await firstNameInput.type(userData.firstName);
                
                const lastNameInput = await this.page.$('input[name*="last"], input[name*="Last"]');
                if (lastNameInput) await lastNameInput.type(userData.lastName);
                
                const phoneInput = await this.page.$('input[name*="phone"], input[name*="Phone"]');
                if (phoneInput) await phoneInput.type(userData.phone);
            } catch (e) {
                console.log('Some optional fields not found');
            }
            
            // Submit registration
            await this.page.click('button[type="submit"], .btn-primary');
            await this.page.waitForTimeout(3000);
            
            // Check if registration successful
            const url = this.page.url();
            return !url.includes('/register');
            
        } catch (error) {
            console.log(`Registration failed: ${error.message}`);
            return false;
        }
    }

    // Navigate to home and start category selection
    async startCategoryInterview(category) {
        try {
            console.log(`Starting interview for category: ${category}`);
            
            // Go to home page
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(2000);
            
            // Look for category button/card
            const categoryClicked = await this.page.evaluate((cat) => {
                // Look for category elements
                const elements = Array.from(document.querySelectorAll('*'));
                const categoryElement = elements.find(el => {
                    const text = el.textContent || '';
                    return (
                        text.includes(cat) &&
                        (el.tagName === 'BUTTON' || el.tagName === 'A' || 
                         el.classList.contains('card') || el.classList.contains('btn') ||
                         el.getAttribute('data-category') === cat)
                    );
                });
                
                if (categoryElement) {
                    categoryElement.click();
                    return true;
                }
                return false;
            }, category);
            
            if (!categoryClicked) {
                console.log(`Could not find category button for: ${category}`);
                return false;
            }
            
            // Wait for navigation
            await this.page.waitForTimeout(3000);
            
            // Check if we're on interview page
            const url = this.page.url();
            return url.includes('interview') || url.includes('phase2');
            
        } catch (error) {
            console.log(`Failed to start category interview: ${error.message}`);
            return false;
        }
    }

    // Execute Phase 2 interview with specific answer pattern
    async executePhase2Interview(answerPattern, maxQuestions = 10) {
        try {
            console.log(`Executing interview with pattern: [${answerPattern.join(', ')}]`);
            
            let questionNumber = 0;
            
            for (let i = 0; i < maxQuestions && i < answerPattern.length; i++) {
                questionNumber++;
                const answerIndex = answerPattern[i]; // 0=A, 1=B, 2=C
                
                console.log(`Question ${questionNumber}: Selecting option ${String.fromCharCode(65 + answerIndex)}`);
                
                // Wait for question to load
                await this.page.waitForSelector('.question-box, .card-body', { timeout: 10000 });
                await this.page.waitForTimeout(1000);
                
                // Check for completion message first
                const isComplete = await this.page.evaluate(() => {
                    const text = document.body.textContent || '';
                    return text.includes('Complete') || text.includes('Perfect') || 
                           text.includes('recommend') || text.includes('workflow');
                });
                
                if (isComplete) {
                    console.log('Interview completed successfully');
                    return true;
                }
                
                // Look for option buttons
                const optionButtons = await this.page.$$('button.btn-outline-primary, button.btn-primary');
                
                if (optionButtons.length === 0) {
                    console.log('No option buttons found');
                    return false;
                }
                
                console.log(`Found ${optionButtons.length} option buttons`);
                
                // Click the selected option
                if (answerIndex < optionButtons.length) {
                    await optionButtons[answerIndex].click();
                    console.log(`Clicked option ${answerIndex + 1}`);
                    
                    // Wait for selection to register
                    await this.page.waitForTimeout(1000);
                    
                    // Look for and click Next button
                    const nextButton = await this.page.$('button:contains("Next"), .btn-success, button[type="submit"]');
                    if (nextButton) {
                        await nextButton.click();
                        console.log('Clicked Next button');
                        
                        // Wait for next question or completion
                        await this.page.waitForTimeout(3000);
                        
                        // Check again for completion
                        const completedNow = await this.page.evaluate(() => {
                            const text = document.body.textContent || '';
                            return text.includes('Complete') || text.includes('Perfect') || 
                                   text.includes('recommend') || text.includes('Dashboard');
                        });
                        
                        if (completedNow) {
                            console.log('Interview completed after clicking Next');
                            return true;
                        }
                        
                    } else {
                        console.log('Next button not found');
                        // Continue to next iteration in case it auto-progressed
                    }
                } else {
                    console.log(`Answer index ${answerIndex} out of range for ${optionButtons.length} options`);
                    return false;
                }
            }
            
            // Check final completion state
            const finalCheck = await this.page.evaluate(() => {
                const text = document.body.textContent || '';
                return text.includes('Complete') || text.includes('Perfect') || 
                       text.includes('recommend') || text.includes('Dashboard');
            });
            
            return finalCheck;
            
        } catch (error) {
            console.log(`Interview execution error: ${error.message}`);
            return false;
        }
    }

    // Generate all possible answer patterns
    generateTestPatterns() {
        const patterns = [];
        
        // Test different path lengths and combinations
        const basicPatterns = [
            [0], [1], [2],                    // Single answers
            [0, 0], [0, 1], [0, 2],          // Two questions - A then A/B/C
            [1, 0], [1, 1], [1, 2],          // Two questions - B then A/B/C  
            [2, 0], [2, 1], [2, 2],          // Two questions - C then A/B/C
            [0, 0, 0], [0, 0, 1], [0, 0, 2], // Three questions starting with A
            [1, 1, 0], [1, 1, 1], [1, 1, 2], // Three questions starting with B
            [2, 2, 0], [2, 2, 1], [2, 2, 2], // Three questions starting with C
        ];
        
        return basicPatterns;
    }

    // Run comprehensive test suite
    async runAllTests() {
        console.log('Starting comprehensive Phase 2 UI tests...\n');
        
        const testPatterns = this.generateTestPatterns();
        let testNumber = 1;
        
        // Test a subset of categories for thorough testing
        const testCategories = ['Immigrate', 'Work', 'Family']; // Focus on main categories
        
        for (const category of testCategories) {
            console.log(`\n${'='.repeat(50)}`);
            console.log(`TESTING CATEGORY: ${category}`);
            console.log(`${'='.repeat(50)}`);
            
            // Test first several patterns for each category
            for (let i = 0; i < Math.min(testPatterns.length, 8); i++) {
                const pattern = testPatterns[i];
                const testName = `Test${testNumber}: ${category} - Pattern [${pattern.join(',')}]`;
                
                console.log(`\nRunning ${testName}...`);
                
                try {
                    // Generate unique user for this test
                    const userData = generateTestUser(testNumber);
                    
                    // Setup user (login or register)
                    const userReady = await this.setupTestUser(userData);
                    if (!userReady) {
                        this.testResults.addResult(testName, false, 'Failed to setup test user');
                        testNumber++;
                        continue;
                    }
                    
                    // Start category interview
                    const interviewStarted = await this.startCategoryInterview(category);
                    if (!interviewStarted) {
                        this.testResults.addResult(testName, false, 'Failed to start category interview');
                        testNumber++;
                        continue;
                    }
                    
                    // Execute interview with pattern
                    const interviewCompleted = await this.executePhase2Interview(pattern);
                    
                    if (interviewCompleted) {
                        this.testResults.addResult(testName, true, `Successfully completed with pattern [${pattern.join(',')}]`);
                    } else {
                        this.testResults.addResult(testName, false, `Interview did not complete with pattern [${pattern.join(',')}]`);
                    }
                    
                } catch (error) {
                    this.testResults.addResult(testName, false, 'Unexpected error during test execution', error);
                }
                
                testNumber++;
                
                // Small delay between tests
                await this.page.waitForTimeout(2000);
            }
        }
        
        return this.testResults.generateReport();
    }
}

// Main execution function
async function runPhase2Tests() {
    const tester = new Phase2UITester();
    
    try {
        await tester.init();
        console.log('Browser initialized successfully');
        
        const results = await tester.runAllTests();
        
        console.log('\nTest execution completed!');
        console.log(`Results saved to: phase2-ui-test-results.json`);
        
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
    runPhase2Tests()
        .then(results => {
            process.exit(results.summary.failed > 0 ? 1 : 0);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { Phase2UITester, runPhase2Tests };