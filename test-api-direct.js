// Test the integrated API endpoint directly
const fetch = (...args) => import('node-fetch').then(({default: fetch}) => fetch(...args));

async function testIntegratedAPI() {
    try {
        console.log('🔍 Testing integrated API endpoint...\n');
        
        // Test with Terry's user ID - first we need to find it, but let's use a test user
        const testPayload = {
            UserId: "c679b8c2-2891-414a-f130-08ddd36fc637", // testuser@example.com (US citizen)
            Category: "Immigrate"
        };
        
        console.log('Testing integrated Web API endpoint...');
        console.log('Payload:', JSON.stringify(testPayload, null, 2));
        
        try {
            const response = await fetch('http://localhost:5161/api/VisaInterview/phase2/step', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(testPayload)
            });
            
            console.log(`Response Status: ${response.status} ${response.statusText}`);
            
            if (response.ok) {
                const result = await response.json();
                console.log('✅ SUCCESS - Response:', JSON.stringify(result, null, 2));
            } else {
                const errorText = await response.text();
                console.log(`❌ FAILED - Error: ${errorText}`);
                console.log('Response headers:', [...response.headers.entries()]);
            }
            
        } catch (error) {
            console.log(`❌ Network Error: ${error.message}`);
        }
        
    } catch (error) {
        console.error('Test failed:', error);
    }
}

testIntegratedAPI();