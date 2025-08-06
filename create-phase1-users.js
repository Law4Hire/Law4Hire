// Simple script to create Phase 1 test users via API
const fetch = require('node-fetch');

const API_URL = 'http://localhost:5237';

// Generate test users with realistic data
const generateTestUsers = () => {
    const countries = ['India', 'China', 'Mexico', 'Philippines', 'Canada'];
    const categories = ['Immigrate', 'Visit', 'Work', 'Study'];
    
    return categories.map((category, index) => {
        const timestamp = Date.now() + index;
        const randomNum = Math.floor(Math.random() * 1000);
        
        // Generate realistic birth date (22-70 years old)
        const today = new Date();
        const age = 22 + Math.floor(Math.random() * 48); // 22-70 years old
        const birthYear = today.getFullYear() - age;
        const birthMonth = Math.floor(Math.random() * 12) + 1;
        const birthDay = Math.floor(Math.random() * 28) + 1;
        const birthDate = new Date(birthYear, birthMonth - 1, birthDay);
        
        return {
            email: `phase1test${category.toLowerCase()}${timestamp}@example.com`,
            password: 'SecureTest123!',
            firstName: `Test${category}`,
            lastName: `User${randomNum}`,
            middleName: `M${randomNum}`,
            dateOfBirth: birthDate.toISOString().split('T')[0],
            phoneNumber: `555-${Math.floor(Math.random() * 900) + 100}-${Math.floor(Math.random() * 9000) + 1000}`,
            maritalStatus: 'Single',
            address1: `${Math.floor(Math.random() * 9999) + 1} Test Street`,
            address2: '',
            city: 'Test City',
            state: 'CA',
            postalCode: `${Math.floor(Math.random() * 90000) + 10000}`,
            country: 'United States',
            citizenshipCountryId: null,
            hasRelativesInUS: false,
            hasJobOffer: false,
            educationLevel: "Bachelor's Degree",
            fearOfPersecution: false,
            hasPastVisaDenials: false,
            hasStatusViolations: false,
            immigrationGoal: category
        };
    });
};

async function createTestUser(userData) {
    try {
        console.log(`Creating test user: ${userData.email} (${userData.immigrationGoal})`);
        
        const response = await fetch(`${API_URL}/api/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(userData)
        });
        
        if (response.ok) {
            console.log(`✅ Created: ${userData.email}`);
            return { ...userData, created: true };
        } else {
            const error = await response.text();
            console.log(`❌ Failed to create ${userData.email}: ${response.status} ${error}`);
            return { ...userData, created: false, error };
        }
        
    } catch (error) {
        console.log(`❌ Error creating ${userData.email}: ${error.message}`);
        return { ...userData, created: false, error: error.message };
    }
}

async function createAllTestUsers() {
    console.log('Creating Phase 1 test users via API...\n');
    
    const testUsers = generateTestUsers();
    const results = [];
    
    for (const userData of testUsers) {
        const result = await createTestUser(userData);
        results.push(result);
        
        // Small delay between requests
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
    
    // Save successful users for Phase 2 testing
    const successfulUsers = results.filter(u => u.created);
    const fs = require('fs');
    
    if (successfulUsers.length > 0) {
        const userData = successfulUsers.map(u => ({
            email: u.email,
            password: u.password,
            firstName: u.firstName,
            lastName: u.lastName,
            category: u.immigrationGoal,
            registrationDate: new Date().toISOString()
        }));
        
        fs.writeFileSync('phase1-test-users.json', JSON.stringify(userData, null, 2));
        
        console.log('\n' + '='.repeat(60));
        console.log('PHASE 1 USER CREATION COMPLETE');
        console.log('='.repeat(60));
        console.log(`Total users attempted: ${results.length}`);
        console.log(`Successfully created: ${successfulUsers.length}`);
        console.log(`Failed: ${results.length - successfulUsers.length}`);
        console.log('\n📝 Test users saved to: phase1-test-users.json');
        console.log('   You can now run Phase 2 tests with these users!');
        console.log('='.repeat(60));
        
        // Display created users
        console.log('\n✅ Created test users:');
        successfulUsers.forEach(user => {
            console.log(`   ${user.immigrationGoal}: ${user.email} / ${user.password}`);
        });
        
        return true;
    } else {
        console.log('\n❌ No users were created successfully');
        return false;
    }
}

async function testLogin(email, password) {
    try {
        const response = await fetch(`${API_URL}/api/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ email, password })
        });
        
        if (response.ok) {
            console.log(`✅ Login test successful for: ${email}`);
            return true;
        } else {
            console.log(`❌ Login test failed for: ${email}`);
            return false;
        }
        
    } catch (error) {
        console.log(`❌ Login test error for ${email}: ${error.message}`);
        return false;
    }
}

// Run if executed directly
if (require.main === module) {
    createAllTestUsers()
        .then(success => {
            if (success) {
                console.log('\n🚀 Ready to run Phase 2 tests!');
                console.log('   Command: node phase2-from-phase1-test.js');
            }
            process.exit(success ? 0 : 1);
        })
        .catch(error => {
            console.error('Fatal error:', error);
            process.exit(1);
        });
}

module.exports = { createAllTestUsers, generateTestUsers };