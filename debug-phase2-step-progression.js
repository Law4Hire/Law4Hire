// Comprehensive debug test for Phase 2 step progression
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
                        data: responseData ? JSON.parse(responseData) : null,
                        rawData: responseData
                    };
                    resolve(response);
                } catch (e) {
                    resolve({
                        statusCode: res.statusCode,
                        data: null,
                        rawData: responseData,
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

async function debugPhase2StepProgression() {
    console.log('🔍 DEBUG: Phase 2 Step Progression Analysis\n');
    console.log('=' .repeat(80));
    
    try {
        // Step 1: Initial request
        console.log('\n=== STEP 1: Initial Request ===');
        const step1Request = {
            UserId: TEST_USER_ID,
            Category: 'Immigrate',
            Instructions: 'Please help me find the right visa type based on my specific situation.'
        };
        
        console.log('REQUEST:', JSON.stringify(step1Request, null, 2));
        const step1Response = await makeRequest('/api/VisaInterview/phase2/step', step1Request);
        
        console.log(`\nRESPONSE STATUS: ${step1Response.statusCode}`);
        console.log('RESPONSE DATA:', JSON.stringify(step1Response.data, null, 2));
        
        if (step1Response.statusCode !== 200) {
            console.log('❌ Step 1 failed');
            return false;
        }
        
        // Extract key information from Step 1
        const step1Question = step1Response.data.question;
        const step1Options = step1Response.data.options;
        const step1Step = step1Response.data.step;
        
        console.log(`\nStep 1 Analysis:`);
        console.log(`  Step Number: ${step1Step}`);
        console.log(`  Question: "${step1Question}"`);
        console.log(`  Options Count: ${step1Options?.length || 0}`);
        if (step1Options && step1Options.length > 0) {
            step1Options.forEach(opt => {
                console.log(`    ${opt.Key}: ${opt.Text}`);
            });
        }
        
        // Step 2: Submit answer "A" 
        console.log('\n=== STEP 2: Submit Answer A ===');
        const step2Request = {
            UserId: TEST_USER_ID,
            Category: 'Immigrate',
            Instructions: 'Please help me find the right visa type based on my specific situation.',
            Answer: 'A'
        };
        
        console.log('REQUEST:', JSON.stringify(step2Request, null, 2));
        const step2Response = await makeRequest('/api/VisaInterview/phase2/step', step2Request);
        
        console.log(`\nRESPONSE STATUS: ${step2Response.statusCode}`);
        console.log('RESPONSE DATA:', JSON.stringify(step2Response.data, null, 2));
        
        if (step2Response.statusCode !== 200) {
            console.log('❌ Step 2 failed');
            return false;
        }
        
        // Extract key information from Step 2
        const step2Question = step2Response.data.question;
        const step2Options = step2Response.data.options;
        const step2Step = step2Response.data.step;
        
        console.log(`\nStep 2 Analysis:`);
        console.log(`  Step Number: ${step2Step}`);
        console.log(`  Question: "${step2Question}"`);
        console.log(`  Options Count: ${step2Options?.length || 0}`);
        if (step2Options && step2Options.length > 0) {
            step2Options.forEach(opt => {
                console.log(`    ${opt.Key}: ${opt.Text}`);
            });
        }
        
        // Compare Step 1 and Step 2
        console.log('\n=== COMPARISON ANALYSIS ===');
        
        const stepChanged = step1Step !== step2Step;
        const questionChanged = step1Question !== step2Question;
        const optionsChanged = JSON.stringify(step1Options) !== JSON.stringify(step2Options);
        
        console.log(`Step Number Changed: ${stepChanged} (${step1Step} → ${step2Step})`);
        console.log(`Question Changed: ${questionChanged}`);
        console.log(`Options Changed: ${optionsChanged}`);
        
        if (stepChanged && step2Step > step1Step) {
            console.log('🎉 SUCCESS: Step progression working!');
            return true;
        } else if (!questionChanged && !optionsChanged) {
            console.log('❌ PROBLEM: Exact same response returned - no progression');
            console.log('This indicates the API is not processing the answer or filtering the visa list');
            return false;
        } else if (questionChanged || optionsChanged) {
            console.log('⚠️  PARTIAL: Some changes detected but step number didn\'t advance');
            console.log('This might indicate partial processing');
            return false;
        } else {
            console.log('❌ UNKNOWN: Unexpected response pattern');
            return false;
        }
        
    } catch (error) {
        console.log('❌ Debug test failed:', error.message);
        return false;
    }
}

// Run the debug test
debugPhase2StepProgression().then(success => {
    console.log('\n' + '=' .repeat(80));
    if (success) {
        console.log('✅ DEBUG TEST CONCLUSION: Step progression is working correctly!');
        process.exit(0);
    } else {
        console.log('❌ DEBUG TEST CONCLUSION: Step progression has issues that need fixing!');
        process.exit(1);
    }
}).catch(error => {
    console.error('❌ Debug test execution failed:', error);
    process.exit(1);
});