// Create a test user via API for Phase 2 testing
const fetch = require('node-fetch');

const API_URL = 'http://localhost:5237';
const TEST_USER = {
    email: 'phase2testuser@example.com',
    password: 'SecureTest123!',
    firstName: 'Phase2',
    lastName: 'TestUser',
    phoneNumber: '555-0199'
};

async function createTestUser() {
    try {
        console.log('Creating test user via API...');
        
        const response = await fetch(`${API_URL}/api/auth/register`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(TEST_USER)
        });
        
        if (response.ok) {
            const result = await response.text();
            console.log('✅ Test user created successfully');
            console.log('User details:', TEST_USER);
            return true;
        } else {
            const error = await response.text();
            console.log('❌ Failed to create test user:', response.status, error);
            return false;
        }
        
    } catch (error) {
        console.log('❌ Error creating test user:', error.message);
        return false;
    }
}

async function testLogin() {
    try {
        console.log('Testing login...');
        
        const response = await fetch(`${API_URL}/api/auth/login`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: TEST_USER.email,
                password: TEST_USER.password
            })
        });
        
        if (response.ok) {
            const result = await response.json();
            console.log('✅ Login test successful');
            console.log('Token received:', result.token ? 'Yes' : 'No');
            return true;
        } else {
            const error = await response.text();
            console.log('❌ Login test failed:', response.status, error);
            return false;
        }
        
    } catch (error) {
        console.log('❌ Error testing login:', error.message);
        return false;
    }
}

async function main() {
    console.log('Setting up test user for Phase 2 testing...\n');
    
    // Try to create user
    const created = await createTestUser();
    
    // Test login regardless of creation result (user might already exist)
    await testLogin();
    
    console.log('\n' + '='.repeat(50));
    console.log('Test user setup complete!');
    console.log('Email:', TEST_USER.email);
    console.log('Password:', TEST_USER.password);
    console.log('Use this user for Phase 2 UI testing');
    console.log('='.repeat(50));
}

if (require.main === module) {
    main().catch(console.error);
}

module.exports = { createTestUser, testLogin, TEST_USER };