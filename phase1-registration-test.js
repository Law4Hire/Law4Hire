const puppeteer = require('puppeteer');
const fs = require('fs');

// Configuration
const BASE_URL = 'http://localhost:5161';

// Test user data - realistic but fake data for testing
const generateTestUser = () => {
    const timestamp = Date.now();
    const randomNum = Math.floor(Math.random() * 1000);
    
    // Generate realistic birth date (22-70 years old as requested)
    const today = new Date();
    const minAge = 22;
    const maxAge = 70;
    const birthYear = today.getFullYear() - minAge - Math.floor(Math.random() * (maxAge - minAge + 1));
    const birthMonth = Math.floor(Math.random() * 12) + 1;
    const birthDay = Math.floor(Math.random() * 28) + 1; // Use 28 to avoid month issues
    const birthDate = `${birthYear}-${birthMonth.toString().padStart(2, '0')}-${birthDay.toString().padStart(2, '0')}`;
    
    const countries = ['India', 'China', 'Mexico', 'Philippines', 'Canada', 'United Kingdom', 'Germany', 'France', 'Brazil', 'Nigeria'];
    const educationLevels = ["Bachelor's Degree", "Master's Degree", "High School", "Associate's Degree", "Doctorate"];
    const maritalStatuses = ['Single', 'Married', 'Divorced'];
    
    return {
        email: `phase1test${timestamp}_${randomNum}@example.com`,
        firstName: `TestUser${randomNum}`,
        lastName: `Phase1${timestamp}`,
        middleName: `M${randomNum}`,
        dateOfBirth: birthDate,
        maritalStatus: maritalStatuses[Math.floor(Math.random() * maritalStatuses.length)],
        citizenshipCountry: countries[Math.floor(Math.random() * countries.length)],
        hasRelativesInUS: Math.random() > 0.5 ? 'Yes' : 'No',
        hasJobOffer: Math.random() > 0.7 ? 'Yes' : 'No',
        educationLevel: educationLevels[Math.floor(Math.random() * educationLevels.length)],
        fearOfPersecution: Math.random() > 0.9 ? 'Yes' : 'No',
        hasPastVisaDenials: Math.random() > 0.8 ? 'Yes' : 'No',
        hasStatusViolations: Math.random() > 0.9 ? 'Yes' : 'No',
        address1: `${Math.floor(Math.random() * 9999) + 1} Test Street`,
        address2: Math.random() > 0.7 ? `Apt ${Math.floor(Math.random() * 999) + 1}` : '',
        city: 'Test City',
        country: countries[Math.floor(Math.random() * countries.length)],
        state: 'Test State',
        postalCode: `${Math.floor(Math.random() * 90000) + 10000}`,
        phoneNumber: `555-${Math.floor(Math.random() * 900) + 100}-${Math.floor(Math.random() * 9000) + 1000}`,
        password: 'SecureTest123!'
    };
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

        fs.writeFileSync('phase1-registration-test-results.json', JSON.stringify(report, null, 2));
        
        console.log('\n' + '='.repeat(60));
        console.log('PHASE 1 REGISTRATION TEST RESULTS');
        console.log('='.repeat(60));
        console.log(`Total Tests: ${this.totalTests}`);
        console.log(`Passed: ${this.passedTests}`);
        console.log(`Failed: ${this.failedTests}`);
        console.log(`Success Rate: ${report.summary.successRate}`);
        console.log('='.repeat(60));
        
        return report;
    }
}

class Phase1RegistrationTester {
    constructor() {
        this.browser = null;
        this.page = null;
        this.testResults = new TestResults();
    }

    async init() {
        console.log('Launching browser for Phase 1 registration test...');
        this.browser = await puppeteer.launch({ 
            headless: false, // Show browser for debugging
            defaultViewport: { width: 1280, height: 720 },
            args: ['--no-sandbox', '--disable-setuid-sandbox'],
            slowMo: 200 // Slow down for visibility
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

    // Fill text input field with wait and validation
    async fillField(selector, value, fieldName) {
        try {
            await this.page.waitForSelector(selector, { timeout: 5000 });
            await this.page.click(selector);
            await this.page.evaluate((sel) => document.querySelector(sel).value = '', selector);
            await this.page.type(selector, value);
            console.log(`✅ Filled ${fieldName}: ${value}`);
            return true;
        } catch (error) {
            console.log(`❌ Failed to fill ${fieldName}: ${error.message}`);
            return false;
        }
    }

    // Select dropdown option
    async selectDropdown(selector, value, fieldName) {
        try {
            await this.page.waitForSelector(selector, { timeout: 5000 });
            await this.page.select(selector, value);
            console.log(`✅ Selected ${fieldName}: ${value}`);
            return true;
        } catch (error) {
            console.log(`❌ Failed to select ${fieldName}: ${error.message}`);
            return false;
        }
    }

    // Select radio button option
    async selectRadio(optionText, fieldName) {
        try {
            const radioSelected = await this.page.evaluate((text) => {
                const labels = Array.from(document.querySelectorAll('label.form-check-label'));
                const label = labels.find(l => l.textContent.trim() === text);
                if (label) {
                    const radio = label.previousElementSibling || label.closest('.form-check').querySelector('input[type="radio"]');
                    if (radio) {
                        radio.click();
                        return true;
                    }
                }
                return false;
            }, optionText);

            if (radioSelected) {
                console.log(`✅ Selected ${fieldName}: ${optionText}`);
                return true;
            } else {
                console.log(`❌ Failed to find radio option for ${fieldName}: ${optionText}`);
                return false;
            }
        } catch (error) {
            console.log(`❌ Error selecting radio ${fieldName}: ${error.message}`);
            return false;
        }
    }

    // Click Next button and wait for navigation
    async clickNext() {
        try {
            await this.page.waitForSelector('button[type="submit"], .btn-primary', { timeout: 5000 });
            await this.page.click('button[type="submit"], .btn-primary');
            await this.page.waitForTimeout(2000); // Wait for form processing
            console.log('✅ Clicked Next button');
            return true;
        } catch (error) {
            console.log(`❌ Failed to click Next button: ${error.message}`);
            return false;
        }
    }

    // Complete Phase 1 registration process
    async completePhase1Registration(category = 'Immigrate') {
        const testUser = generateTestUser();
        const testName = `Phase 1 Registration - ${category} - ${testUser.email}`;
        
        try {
            console.log(`\n${'='.repeat(60)}`);
            console.log(`Starting Phase 1 Registration Test`);
            console.log(`Category: ${category}`);
            console.log(`Email: ${testUser.email}`);
            console.log(`${'='.repeat(60)}`);
            
            // Step 1: Go to home page
            console.log('Step 1: Navigating to home page...');
            await this.page.goto(`${BASE_URL}/`, { waitUntil: 'networkidle0' });
            await this.page.waitForTimeout(2000);

            // Step 2: Click category card to start registration
            console.log(`Step 2: Clicking ${category} category card...`);
            const categoryClicked = await this.page.evaluate((cat) => {
                // Look for immigration cards with the category text
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
                this.testResults.addResult(testName, false, 'Failed to click category card');
                return false;
            }

            await this.page.waitForTimeout(3000);

            // Step 3: Fill Email
            console.log('Step 3: Filling email...');
            const emailFilled = await this.fillField('input[type="email"], input[name="email"]', testUser.email, 'Email');
            if (!emailFilled) {
                this.testResults.addResult(testName, false, 'Failed to fill email field');
                return false;
            }
            await this.clickNext();

            // Step 4: Fill First Name
            console.log('Step 4: Filling first name...');
            const firstNameFilled = await this.fillField('input[type="text"]', testUser.firstName, 'First Name');
            if (!firstNameFilled) {
                this.testResults.addResult(testName, false, 'Failed to fill first name field');
                return false;
            }
            await this.clickNext();

            // Step 5: Fill Last Name
            console.log('Step 5: Filling last name...');
            const lastNameFilled = await this.fillField('input[type="text"]', testUser.lastName, 'Last Name');
            if (!lastNameFilled) {
                this.testResults.addResult(testName, false, 'Failed to fill last name field');
                return false;
            }
            await this.clickNext();

            // Step 6: Fill Middle Name (optional)
            console.log('Step 6: Filling middle name (optional)...');
            await this.fillField('input[type="text"]', testUser.middleName, 'Middle Name');
            await this.clickNext();

            // Step 7: Fill Date of Birth
            console.log('Step 7: Filling date of birth...');
            const dobFilled = await this.fillField('input[type="date"]', testUser.dateOfBirth, 'Date of Birth');
            if (!dobFilled) {
                this.testResults.addResult(testName, false, 'Failed to fill date of birth field');
                return false;
            }
            await this.clickNext();

            // Step 8: Select Marital Status
            console.log('Step 8: Selecting marital status...');
            const maritalSelected = await this.selectDropdown('select', testUser.maritalStatus, 'Marital Status');
            if (!maritalSelected) {
                this.testResults.addResult(testName, false, 'Failed to select marital status');
                return false;
            }
            await this.clickNext();

            // Step 9: Select Citizenship Country (SearchableSelect - try typing)
            console.log('Step 9: Selecting citizenship country...');
            try {
                // For searchable select, we need to type and then select
                await this.page.waitForSelector('input', { timeout: 5000 });
                const countryInput = await this.page.$('input:not([type="hidden"])');
                if (countryInput) {
                    await countryInput.type(testUser.citizenshipCountry);
                    await this.page.waitForTimeout(1000);
                    
                    // Look for dropdown options and click the first match
                    const optionClicked = await this.page.evaluate((country) => {
                        const options = Array.from(document.querySelectorAll('li, option, .option, [role="option"]'));
                        const option = options.find(opt => opt.textContent?.includes(country));
                        if (option) {
                            option.click();
                            return true;
                        }
                        return false;
                    }, testUser.citizenshipCountry);
                    
                    if (optionClicked) {
                        console.log(`✅ Selected citizenship country: ${testUser.citizenshipCountry}`);
                    } else {
                        console.log(`⚠️ Could not find dropdown option, continuing with typed value`);
                    }
                }
            } catch (error) {
                console.log(`⚠️ Citizenship country field issue: ${error.message}`);
            }
            await this.clickNext();

            // Step 10: Select Has Relatives in US
            console.log('Step 10: Selecting relatives in US...');
            const relativesSelected = await this.selectRadio(testUser.hasRelativesInUS, 'Has Relatives in US');
            if (!relativesSelected) {
                this.testResults.addResult(testName, false, 'Failed to select relatives in US');
                return false;
            }
            await this.clickNext();

            // Step 11: Select Has Job Offer
            console.log('Step 11: Selecting job offer...');
            const jobOfferSelected = await this.selectRadio(testUser.hasJobOffer, 'Has Job Offer');
            if (!jobOfferSelected) {
                this.testResults.addResult(testName, false, 'Failed to select job offer');
                return false;
            }
            await this.clickNext();

            // Step 12: Select Education Level
            console.log('Step 12: Selecting education level...');
            const educationSelected = await this.selectDropdown('select', testUser.educationLevel, 'Education Level');
            if (!educationSelected) {
                this.testResults.addResult(testName, false, 'Failed to select education level');
                return false;
            }
            await this.clickNext();

            // Step 13: Select Fear of Persecution
            console.log('Step 13: Selecting fear of persecution...');
            await this.selectRadio(testUser.fearOfPersecution, 'Fear of Persecution');
            await this.clickNext();

            // Step 14: Select Past Visa Denials
            console.log('Step 14: Selecting past visa denials...');
            await this.selectRadio(testUser.hasPastVisaDenials, 'Past Visa Denials');
            await this.clickNext();

            // Step 15: Select Status Violations
            console.log('Step 15: Selecting status violations...');
            await this.selectRadio(testUser.hasStatusViolations, 'Status Violations');
            await this.clickNext();

            // Step 16: Fill Address1
            console.log('Step 16: Filling address line 1...');
            await this.fillField('input[type="text"]', testUser.address1, 'Address 1');
            await this.clickNext();

            // Step 17: Fill Address2 (optional)
            console.log('Step 17: Filling address line 2 (optional)...');
            await this.fillField('input[type="text"]', testUser.address2, 'Address 2');
            await this.clickNext();

            // Step 18: Fill City
            console.log('Step 18: Filling city...');
            await this.fillField('input[type="text"]', testUser.city, 'City');
            await this.clickNext();

            // Step 19: Select Country (for address)
            console.log('Step 19: Selecting address country...');
            try {
                await this.page.waitForSelector('input', { timeout: 5000 });
                const addressCountryInput = await this.page.$('input:not([type="hidden"])');
                if (addressCountryInput) {
                    await addressCountryInput.type(testUser.country);
                    await this.page.waitForTimeout(1000);
                    
                    // Click dropdown option if available
                    const optionClicked = await this.page.evaluate((country) => {
                        const options = Array.from(document.querySelectorAll('li, option, .option, [role="option"]'));
                        const option = options.find(opt => opt.textContent?.includes(country));
                        if (option) {
                            option.click();
                            return true;
                        }
                        return false;
                    }, testUser.country);
                    
                    if (optionClicked) {
                        console.log(`✅ Selected address country: ${testUser.country}`);
                    } else {
                        console.log(`⚠️ Could not find dropdown option, continuing with typed value`);
                    }
                }
            } catch (error) {
                console.log(`⚠️ Address country field issue: ${error.message}`);
            }
            await this.clickNext();

            // Step 20: Fill State (if required)
            console.log('Step 20: Filling state...');
            await this.fillField('input[type="text"]', testUser.state, 'State');
            await this.clickNext();

            // Step 21: Fill Postal Code
            console.log('Step 21: Filling postal code...');
            await this.fillField('input[type="text"]', testUser.postalCode, 'Postal Code');
            await this.clickNext();

            // Step 22: Fill Phone Number
            console.log('Step 22: Filling phone number...');
            await this.fillField('input[type="tel"], input[type="text"]', testUser.phoneNumber, 'Phone Number');
            await this.clickNext();

            // Step 23: Fill Password
            console.log('Step 23: Filling password...');
            const passwordFilled = await this.fillField('input[type="password"]', testUser.password, 'Password');
            if (!passwordFilled) {
                this.testResults.addResult(testName, false, 'Failed to fill password field');
                return false;
            }
            
            // Submit final form
            console.log('Step 24: Submitting final registration...');
            await this.clickNext();
            
            // Wait for redirect or completion
            await this.page.waitForTimeout(5000);
            
            // Check if we've been redirected to Phase 2 or dashboard
            const currentUrl = this.page.url();
            console.log(`Final URL: ${currentUrl}`);
            
            const success = currentUrl.includes('/phase2') || currentUrl.includes('/interview') || 
                          currentUrl.includes('/dashboard') || !currentUrl.includes(BASE_URL + '/');
            
            if (success) {
                // Save successful test user for Phase 2 testing
                const userData = {
                    email: testUser.email,
                    password: testUser.password,
                    firstName: testUser.firstName,
                    lastName: testUser.lastName,
                    category: category,
                    registrationDate: new Date().toISOString()
                };
                
                // Append to test users file
                let testUsers = [];
                try {
                    const existingData = fs.readFileSync('phase1-test-users.json', 'utf8');
                    testUsers = JSON.parse(existingData);
                } catch (e) {
                    // File doesn't exist or is invalid, start with empty array
                }
                
                testUsers.push(userData);
                fs.writeFileSync('phase1-test-users.json', JSON.stringify(testUsers, null, 2));
                
                this.testResults.addResult(testName, true, `Registration completed successfully. Redirected to: ${currentUrl}`);
                console.log(`✅ Registration successful! User saved for Phase 2 testing.`);
                return true;
            } else {
                this.testResults.addResult(testName, false, `Registration did not complete properly. Final URL: ${currentUrl}`);
                return false;
            }
            
        } catch (error) {
            this.testResults.addResult(testName, false, 'Unexpected error during registration', error);
            console.log(`❌ Registration failed with error: ${error.message}`);
            return false;
        }
    }

    // Run Phase 1 tests for multiple categories
    async runPhase1Tests() {
        console.log('Starting Phase 1 Registration Tests...\n');
        
        // Test different categories
        const categories = ['Immigrate', 'Visit', 'Work', 'Study'];
        
        for (const category of categories) {
            await this.completePhase1Registration(category);
            
            // Small delay between tests
            await this.page.waitForTimeout(2000);
            
            // Clear any alerts or dialogs
            try {
                await this.page.evaluate(() => {
                    // Clear any alerts
                    if (typeof window.alert === 'function') {
                        window.alert = () => {};
                    }
                });
            } catch (e) {
                // Ignore errors
            }
        }
        
        return this.testResults.generateReport();
    }
}

// Main execution function
async function runPhase1Tests() {
    const tester = new Phase1RegistrationTester();
    
    try {
        await tester.init();
        console.log('Browser initialized successfully');
        
        const results = await tester.runPhase1Tests();
        
        console.log('\nPhase 1 test execution completed!');
        console.log(`Results saved to: phase1-registration-test-results.json`);
        console.log(`Test users saved to: phase1-test-users.json`);
        
        if (results.summary.failed > 0) {
            console.log('\n⚠️  FAILED TESTS:');
            results.results.filter(r => !r.passed).forEach(result => {
                console.log(`   ${result.testName}: ${result.details}`);
            });
        }
        
        console.log('\n📝 Test users have been created and saved to phase1-test-users.json');
        console.log('   You can now use these users for Phase 2 testing!');
        
        return results;
        
    } catch (error) {
        console.error('❌ Phase 1 test execution failed:', error);
        throw error;
    } finally {
        await tester.cleanup();
        console.log('Browser closed');
    }
}

// Run tests if this file is executed directly
if (require.main === module) {
    runPhase1Tests()
        .then(results => {
            process.exit(results.summary.failed > 0 ? 1 : 0);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { Phase1RegistrationTester, runPhase1Tests };