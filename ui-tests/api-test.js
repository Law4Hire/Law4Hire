// Simple Node.js script to test the API directly
const https = require('https');

// Disable SSL verification for localhost testing
process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

async function testPhase2API() {
    console.log('🔧 Testing Phase2 API endpoint directly...');
    
    try {
        // Test 1: Register a user
        console.log('📝 Step 1: Registering test user...');
        const testUser = {
            firstName: 'APITest',
            lastName: 'User',
            email: `apitest${Date.now()}@testing.com`,
            password: 'SecureTest123!',
            category: 'Immigrate',
            phoneNumber: '+1234567890',
            city: 'Test City',
            state: 'CA',
            country: 'United States',
            dateOfBirth: '1990-01-01T00:00:00Z',
            immigrationGoal: 'Immigrate'
        };

        const registerResponse = await fetch('https://localhost:7280/api/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(testUser)
        });

        console.log('Register response status:', registerResponse.status);
        if (registerResponse.status !== 200) {
            const errorText = await registerResponse.text();
            console.error('❌ Registration failed:', errorText);
            return;
        }
        
        const registerData = await registerResponse.json();
        console.log('✅ Registration successful:', registerData);
        
        const userId = registerData.userId || registerData.id;
        if (!userId) {
            console.error('❌ No userId returned from registration');
            return;
        }

        // Test 2: Test Phase2 step endpoint
        console.log('🎯 Step 2: Testing Phase2 interview...');
        const phase2Response = await fetch('https://localhost:7280/api/VisaInterview/phase2/step', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                userId: userId,
                category: 'Immigrate',
                answer: null // First request - should return initial question
            })
        });

        console.log('Phase2 response status:', phase2Response.status);
        
        if (phase2Response.status !== 200) {
            console.error('❌ Phase2 API error status:', phase2Response.status);
            const errorText = await phase2Response.text();
            console.error('❌ Phase2 API error response:', errorText);
            
            // Check if it's a server error
            if (phase2Response.status >= 500) {
                console.error('🚨 Server error - check API logs');
            } else if (phase2Response.status === 400) {
                console.error('🚨 Bad Request - likely the bug we\'re looking for');
            }
            return;
        }

        const phase2Data = await phase2Response.json();
        console.log('✅ Phase2 response data:', phase2Data);
        
        if (phase2Data.question && phase2Data.question.length > 10) {
            console.log('🎉 SUCCESS: Phase2 interview working correctly!');
            console.log('Question received:', phase2Data.question);
        } else {
            console.error('❌ FAILURE: No valid question received');
            console.error('Response:', phase2Data);
        }
        
    } catch (error) {
        console.error('❌ Test failed with error:', error.message);
        console.error('Stack:', error.stack);
    }
}

// Run the test
testPhase2API();