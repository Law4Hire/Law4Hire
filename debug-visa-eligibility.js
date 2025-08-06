// Debug script to test visa eligibility for different users
const fetch = (...args) => import('node-fetch').then(({default: fetch}) => fetch(...args));

// Disable SSL verification for localhost testing
process.env["NODE_TLS_REJECT_UNAUTHORIZED"] = 0;

async function testVisaEligibility() {
    try {
        console.log('🔍 Testing visa eligibility for different users...\n');
        
        // Test data
        const users = [
            {
                name: 'Working US Citizen',
                userId: 'c679b8c2-2891-414a-f130-08ddd36fc637', // testuser@example.com
                country: 'United States'
            },
            {
                name: 'Failing Albanian Citizen (Terry)',
                userId: 'terry-user-id-here', // Terry@testing.com - need to find actual ID
                country: 'Albania'
            }
        ];
        
        for (const user of users) {
            console.log(`\n📋 Testing: ${user.name} (${user.country})`);
            console.log(`User ID: ${user.userId}`);
            
            // Test the Phase2 API call that's failing
            try {
                const payload = {
                    UserId: user.userId,
                    Category: 'Immigrate',
                    Instructions: 'Please help me find the right visa type based on my specific situation.'
                };
                
                console.log('Payload:', JSON.stringify(payload, null, 2));
                
                const response = await fetch('https://localhost:7280/api/VisaInterview/phase2/step', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(payload)
                });
                
                console.log(`Response Status: ${response.status} ${response.statusText}`);
                
                if (response.ok) {
                    const result = await response.json();
                    console.log('✅ SUCCESS - Response:', JSON.stringify(result, null, 2));
                } else {
                    const errorText = await response.text();
                    console.log(`❌ FAILED - Error: ${errorText}`);
                }
                
            } catch (error) {
                console.log(`❌ ERROR: ${error.message}`);
            }
            
            console.log('─'.repeat(60));
        }
        
    } catch (error) {
        console.error('Test failed:', error);
    }
}

testVisaEligibility();