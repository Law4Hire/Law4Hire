// Direct API test for Phase 2 step progression
const https = require('https');

const API_URL = 'https://localhost:7280';
const TEST_USER_ID = 'eaf13c98-3aef-4eb8-ed49-08ddd43150ea'; // jimmy@testing.com

// Disable SSL verification for localhost testing
process.env['NODE_TLS_REJECT_UNAUTHORIZED'] = 0;

async function makeRequest(path, data) {
    return new Promise((resolve, reject) => {
        const postData = JSON.stringify(data);
        
        const options = {
            hostname: 'localhost',
            port: 7280,
            path: path,
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept-Language': 'en-US',
                'Content-Length': Buffer.byteLength(postData)
            },
            rejectUnauthorized: false
        };

        const req = https.request(options, (res) => {
            let responseData = '';
            
            res.on('data', (chunk) => {
                responseData += chunk;
            });
            
            res.on('end', () => {
                try {
                    const response = {
                        statusCode: res.statusCode,
                        data: responseData ? JSON.parse(responseData) : null
                    };
                    resolve(response);
                } catch (e) {
                    resolve({
                        statusCode: res.statusCode,
                        data: responseData,
                        error: 'Failed to parse JSON'
                    });
                }
            });
        });

        req.on('error', (e) => {
            reject(e);
        });

        req.write(postData);
        req.end();
    });
}

async function testPhase2StepProgression() {
    console.log('🔍 Testing Phase 2 API Step Progression...\n');
    
    try {
        // Step 1: Initial request - should get first question
        console.log('=== STEP 1: Initial Request ===');
        const step1Request = {
            UserId: TEST_USER_ID,
            Category: 'Immigrate',
            Instructions: 'Please help me find the right visa type based on my specific situation.'
        };
        
        console.log('Sending:', JSON.stringify(step1Request, null, 2));
        const step1Response = await makeRequest('/api/VisaInterview/phase2/step', step1Request);
        
        console.log(`Status: ${step1Response.statusCode}`);
        if (step1Response.statusCode !== 200) {
            console.log('❌ Step 1 failed:', step1Response.data);
            return false;
        }
        
        console.log('✅ Step 1 Response:');
        console.log(`  Question: "${step1Response.data.question?.substring(0, 100)}..."`);
        console.log(`  Step: ${step1Response.data.step}`);
        console.log(`  Options: ${step1Response.data.options?.length || 0}`);
        console.log(`  Is Complete: ${step1Response.data.isComplete}`);
        
        if (step1Response.data.options && step1Response.data.options.length > 0) {
            console.log('  Option A:', step1Response.data.options[0].text);
        }
        
        // Step 2: Answer with option A
        console.log('\n=== STEP 2: Submit Answer A ===');
        const step2Request = {
            UserId: TEST_USER_ID,
            Category: 'Immigrate',
            Instructions: 'Please help me find the right visa type based on my specific situation.',
            Answer: 'A'
        };
        
        console.log('Sending:', JSON.stringify(step2Request, null, 2));
        const step2Response = await makeRequest('/api/VisaInterview/phase2/step', step2Request);
        
        console.log(`Status: ${step2Response.statusCode}`);
        if (step2Response.statusCode !== 200) {
            console.log('❌ Step 2 failed:', step2Response.data);
            return false;
        }
        
        console.log('✅ Step 2 Response:');
        console.log(`  Question: "${step2Response.data.question?.substring(0, 100)}..."`);
        console.log(`  Step: ${step2Response.data.step}`);
        console.log(`  Options: ${step2Response.data.options?.length || 0}`);
        console.log(`  Is Complete: ${step2Response.data.isComplete}`);
        
        // Check if we progressed to Step 2
        if (step2Response.data.step === 2) {
            console.log('\n🎉 SUCCESS: Progressed to Step 2!');
            return true;
        } else if (step2Response.data.step === 1) {
            console.log('\n❌ FAILED: Still at Step 1 - no progression');
            console.log('This suggests the API is returning the same question/list');
            return false;
        } else if (step2Response.data.isComplete) {
            console.log('\n🎉 SUCCESS: Interview completed (fast progression)');
            return true;
        } else {
            console.log(`\n❌ FAILED: Unexpected step ${step2Response.data.step}`);
            return false;
        }
        
    } catch (error) {
        console.log('❌ API Test failed:', error.message);
        return false;
    }
}

// Run the test
testPhase2StepProgression().then(success => {
    if (success) {
        console.log('\n✅ API Step Progression Test PASSED!');
        process.exit(0);
    } else {
        console.log('\n❌ API Step Progression Test FAILED!');
        process.exit(1);
    }
}).catch(error => {
    console.error('❌ Test execution failed:', error);
    process.exit(1);
});