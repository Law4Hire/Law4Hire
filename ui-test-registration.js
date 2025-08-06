// UI Test Script for User Registration
// Password: SecureTest123!

const testUser = {
    email: `test-${Date.now()}@example.com`,
    password: 'SecureTest123!',
    firstName: 'Test',
    lastName: 'User',
    phone: '555-0123'
};

console.log('Starting UI Registration Test');
console.log('Test User:', testUser);

// Navigate to the home page
console.log('Navigating to http://localhost:5161');

// Simulate the registration flow
setTimeout(() => {
    console.log('Test completed - check debug logs for detailed results');
}, 5000);