const puppeteer = require('puppeteer');
const fs = require('fs');

// Configuration
const BASE_URL = 'http://localhost:5161';
const API_URL = 'http://localhost:5237';
const TEST_PASSWORD = 'SecureTest123!';

// Test data
const COUNTRIES = [
    'United States', 'Canada', 'United Kingdom', 'Germany', 'France', 'Italy', 'Spain', 
    'Australia', 'Japan', 'South Korea', 'China', 'India', 'Brazil', 'Mexico', 
    'Argentina', 'South Africa', 'Nigeria', 'Egypt', 'Russia', 'Poland'
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
    const day = Math.floor(Math.random() * 28) + 1; // Use 28 to avoid month issues
    return `${birthYear}-${month.toString().padStart(2, '0')}-${day.toString().padStart(2, '0')}`;
}

// Generate random country
function getRandomCountry() {
    return COUNTRIES[Math.floor(Math.random() * COUNTRIES.length)];
}

// Generate test user data
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

// Test result tracking
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

        fs.writeFileSync('phase2-test-results.json', JSON.stringify(report, null, 2));
        
        console.log('\n' + '='.repeat(60));
        console.log('PHASE 2 INTERVIEW TEST RESULTS');
        console.log('='.repeat(60));
        console.log(`Total Tests: ${this.totalTests}`);
        console.log(`Passed: ${this.passedTests}`);
        console.log(`Failed: ${this.failedTests}`);
        console.log(`Success Rate: ${report.summary.successRate}`);
        console.log('='.repeat(60));
        
        return report;
    }
}

// Main test class
class Phase2InterviewTester {
    constructor() {
        this.browser = null;
        this.page = null;
        this.testResults = new TestResults();
    }

    async init() {
        this.browser = await puppeteer.launch({ 
            headless: false, // Set to true for CI/automated testing
            defaultViewport: { width: 1280, height: 720 },
            args: ['--no-sandbox', '--disable-setuid-sandbox']
        });
        this.page = await this.browser.newPage();
        
        // Set up request/response logging
        this.page.on('response', response => {
            if (response.url().includes('/api/')) {
                console.log(`API Response: ${response.status()} ${response.url()}`);
            }
        });
        
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

    // Register a new test user
    async registerUser(userData) {
        try {
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            
            // Wait for and click register/signup
            await this.page.waitForSelector('a[href*="register"], button:contains("Sign Up"), .register-link', { timeout: 10000 });
            
            // Try multiple selectors for registration
            const registerSelectors = [
                'a[href*="register"]',
                'button:contains("Sign Up")',
                'a:contains("Register")',
                'a:contains("Sign Up")',
                '.register-link'
            ];
            
            let clicked = false;
            for (const selector of registerSelectors) {
                try {
                    const element = await this.page.$(selector);
                    if (element) {
                        await element.click();
                        clicked = true;
                        break;
                    }
                } catch (e) {
                    // Continue trying other selectors
                }
            }
            
            if (!clicked) {
                // Try clicking any link that might lead to registration
                await this.page.evaluate(() => {
                    const links = Array.from(document.querySelectorAll('a, button'));
                    const registerLink = links.find(link => 
                        link.textContent.toLowerCase().includes('register') ||
                        link.textContent.toLowerCase().includes('sign up') ||
                        link.href?.includes('register')
                    );
                    if (registerLink) registerLink.click();
                });
            }
            
            // Wait for registration form
            await this.page.waitForSelector('input[type="email"], input[name="email"]', { timeout: 10000 });
            
            // Fill registration form
            await this.page.type('input[type="email"], input[name="email"]', userData.email);
            await this.page.type('input[type="password"], input[name="password"]', userData.password);
            
            // Fill other fields if they exist
            const firstNameInput = await this.page.$('input[name="firstName"], input[name="firstname"]');
            if (firstNameInput) await firstNameInput.type(userData.firstName);
            
            const lastNameInput = await this.page.$('input[name="lastName"], input[name="lastname"]');
            if (lastNameInput) await lastNameInput.type(userData.lastName);
            
            const phoneInput = await this.page.$('input[name="phone"], input[name="phoneNumber"]');
            if (phoneInput) await phoneInput.type(userData.phone);
            
            // Submit registration
            await this.page.click('button[type="submit"], input[type="submit"], button:contains("Register")');
            
            // Wait for success or navigation
            await this.page.waitForTimeout(3000);
            
            const currentUrl = this.page.url();
            if (currentUrl.includes('login') || currentUrl.includes('dashboard') || currentUrl.includes('home')) {
                return true;
            }
            
            return false;
        } catch (error) {
            console.log(`Registration failed for ${userData.email}: ${error.message}`);
            return false;
        }
    }

    // Login with user credentials
    async loginUser(email, password) {
        try {
            // Go to login page
            await this.page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle0' });
            
            // Fill login form
            await this.page.waitForSelector('input[type="email"], input[name="email"]', { timeout: 5000 });
            await this.page.type('input[type="email"], input[name="email"]', email);
            await this.page.type('input[type="password"], input[name="password"]', password);
            
            // Submit login
            await this.page.click('button[type="submit"], input[type="submit"], button:contains("Login")');
            
            // Wait for navigation
            await this.page.waitForTimeout(3000);
            
            const currentUrl = this.page.url();
            return currentUrl.includes('dashboard') || currentUrl.includes('home') || !currentUrl.includes('login');
        } catch (error) {
            console.log(`Login failed for ${email}: ${error.message}`);
            return false;
        }
    }

    // Navigate to category selection and start Phase 2
    async startPhase2Interview(category) {
        try {
            // Go to home page
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            
            // Look for category button/card
            const categorySelectors = [
                `button:contains("${category}")`,
                `a:contains("${category}")`,
                `.category-${category.toLowerCase()}`,
                `[data-category="${category}"]`,
                `.card:contains("${category}")`
            ];
            
            let categoryClicked = false;
            for (const selector of categorySelectors) {
                try {
                    const element = await this.page.$(selector);
                    if (element) {
                        await element.click();
                        categoryClicked = true;
                        break;
                    }
                } catch (e) {
                    // Continue trying
                }
            }
            
            if (!categoryClicked) {
                // Try JavaScript click on category
                await this.page.evaluate((cat) => {
                    const elements = Array.from(document.querySelectorAll('*'));
                    const categoryElement = elements.find(el => 
                        el.textContent?.includes(cat) && 
                        (el.tagName === 'BUTTON' || el.tagName === 'A' || el.classList.contains('card'))
                    );
                    if (categoryElement) categoryElement.click();
                }, category);
            }
            
            // Wait for navigation to Phase 2 interview
            await this.page.waitForTimeout(2000);
            
            // Check if we're on Phase 2 interview page
            const currentUrl = this.page.url();
            return currentUrl.includes('phase2') || currentUrl.includes('interview');
        } catch (error) {
            console.log(`Failed to start Phase 2 for category ${category}: ${error.message}`);
            return false;
        }
    }

    // Execute a single Phase 2 interview path
    async executeInterviewPath(testName, category, answerPattern) {
        const userData = generateTestUser(Date.now());
        
        try {
            // Register user
            const registered = await this.registerUser(userData);
            if (!registered) {
                this.testResults.addResult(testName, false, `Failed to register user ${userData.email}`);
                return false;
            }
            
            // Login user
            const loggedIn = await this.loginUser(userData.email, userData.password);
            if (!loggedIn) {
                this.testResults.addResult(testName, false, `Failed to login user ${userData.email}`);
                return false;
            }
            
            // Start Phase 2 interview
            const interviewStarted = await this.startPhase2Interview(category);
            if (!interviewStarted) {
                this.testResults.addResult(testName, false, `Failed to start Phase 2 interview for category ${category}`);
                return false;
            }
            
            // Execute interview with answer pattern
            const interviewCompleted = await this.executeInterviewAnswers(answerPattern);
            if (!interviewCompleted) {
                this.testResults.addResult(testName, false, `Interview failed with pattern ${answerPattern.join('')}`);
                return false;
            }
            
            this.testResults.addResult(testName, true, `Completed successfully: ${category} with pattern ${answerPattern.join('')}`);
            return true;
            
        } catch (error) {
            this.testResults.addResult(testName, false, `Unexpected error in ${testName}`, error);
            return false;
        }
    }

    // Execute interview answers based on pattern
    async executeInterviewAnswers(answerPattern) {
        try {
            let questionCount = 0;
            
            for (const answerIndex of answerPattern) {
                questionCount++;
                
                // Wait for question to load
                await this.page.waitForSelector('.question-box, .card', { timeout: 10000 });
                
                // Wait for buttons to appear
                await this.page.waitForSelector('button.btn-outline-primary, button.btn-primary', { timeout: 5000 });
                
                // Get all option buttons
                const optionButtons = await this.page.$$('button.btn-outline-primary, button.btn-primary');
                
                if (optionButtons.length === 0) {
                    console.log(`No option buttons found on question ${questionCount}`);
                    return false;
                }
                
                // Click the button based on answer index (0=A, 1=B, 2=C)
                if (answerIndex < optionButtons.length) {
                    await optionButtons[answerIndex].click();
                    
                    // Wait for selection to register
                    await this.page.waitForTimeout(500);
                    
                    // Click Next button
                    const nextButton = await this.page.$('button:contains("Next"), .btn-success');
                    if (nextButton) {
                        await nextButton.click();
                        
                        // Wait for next question or completion
                        await this.page.waitForTimeout(2000);
                        
                        // Check if interview is complete
                        const completionText = await this.page.$eval('body', el => el.textContent);
                        if (completionText.includes('Complete') || completionText.includes('Perfect')) {
                            return true; // Interview completed successfully
                        }
                    } else {
                        console.log(`Next button not found after selecting option ${answerIndex}`);
                        return false;
                    }
                } else {
                    console.log(`Answer index ${answerIndex} out of range for ${optionButtons.length} options`);
                    return false;
                }
            }
            
            return true;
        } catch (error) {
            console.log(`Error executing interview answers: ${error.message}`);
            return false;
        }
    }

    // Generate all possible answer patterns for testing
    generateAnswerPatterns(maxQuestions = 5) {
        const patterns = [];
        const options = [0, 1, 2]; // A=0, B=1, C=2
        
        // Generate all possible paths
        function generatePath(currentPath, depth) {
            if (depth >= maxQuestions) {
                patterns.push([...currentPath]);
                return;
            }
            
            for (const option of options) {
                currentPath.push(option);
                generatePath(currentPath, depth + 1);
                currentPath.pop();
            }
        }
        
        // Start with shorter paths and build up
        for (let pathLength = 1; pathLength <= maxQuestions; pathLength++) {
            generatePath([], pathLength);
        }
        
        return patterns.slice(0, 50); // Limit to first 50 patterns for practical testing
    }

    // Run comprehensive test suite
    async runComprehensiveTests() {
        console.log('Starting comprehensive Phase 2 interview tests...\n');
        
        const answerPatterns = this.generateAnswerPatterns(4); // Test up to 4 questions deep
        
        let testNumber = 1;
        
        // Test each category with multiple answer patterns
        for (const category of CATEGORIES) {
            console.log(`\nTesting category: ${category}`);
            
            // Test first few patterns for each category
            const patternsToTest = answerPatterns.slice(0, 10); // Test first 10 patterns per category
            
            for (const pattern of patternsToTest) {
                const testName = `Test${testNumber}: ${category} - Pattern ${pattern.join('')}`;
                await this.executeInterviewPath(testName, category, pattern);
                testNumber++;
                
                // Small delay between tests
                await this.page.waitForTimeout(1000);
            }
        }
        
        return this.testResults.generateReport();
    }
}

// Main execution
async function runTests() {
    const tester = new Phase2InterviewTester();
    
    try {
        await tester.init();
        const results = await tester.runComprehensiveTests();
        
        if (results.summary.failed > 0) {
            console.log('\nFailed tests details:');
            results.results.filter(r => !r.passed).forEach(result => {
                console.log(`- ${result.testName}: ${result.details}`);
                if (result.error) console.log(`  Error: ${result.error}`);
            });
        }
        
    } catch (error) {
        console.error('Test execution failed:', error);
    } finally {
        await tester.cleanup();
    }
}

// Check if this file is being run directly
if (require.main === module) {
    runTests().catch(console.error);
}

module.exports = { Phase2InterviewTester, runTests };