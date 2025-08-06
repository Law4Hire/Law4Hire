// Direct API test to check what's happening with Terry's user
const fetch = (...args) => import('node-fetch').then(({default: fetch}) => fetch(...args));

async function testAPIDirectly() {
    try {
        console.log('🔍 Testing direct API calls...\n');
        
        // Test with a simpler payload to see what happens
        const testPayload = {
            UserId: "some-test-guid-here",
            Category: "Immigrate"
        };
        
        console.log('Testing basic API connectivity...');
        
        // Try the web app's API endpoint (which should be running)
        try {
            const response = await fetch('http://localhost:5161/api/visainterview/phase2/step', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(testPayload)
            });
            
            console.log(`Web API Response Status: ${response.status} ${response.statusText}`);
            const responseText = await response.text();
            console.log(`Web API Response: ${responseText}`);
            
        } catch (error) {
            console.log(`Web API Error: ${error.message}`);
        }
        
    } catch (error) {
        console.error('Test failed:', error);
    }
}

testAPIDirectly();