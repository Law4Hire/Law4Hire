// Simple test to verify Phase 2 step progression works
const API_URL = 'http://localhost:5237';

// Test user ID (you may need to adjust this)
const TEST_USER_ID = '12345678-1234-1234-1234-123456789abc';

async function testStepProgression() {
    console.log('Testing Phase 2 step progression...');
    
    try {
        // Step 1: Send initial request to start Phase 2
        console.log('\n=== STEP 1: Initial Request ===');
        const step1Response = await fetch(`${API_URL}/api/VisaInterview/phase2/step`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept-Language': 'en-US'
            },
            body: JSON.stringify({
                UserId: TEST_USER_ID,
                Category: 'Immigrate',
                Instructions: 'Please help me find the right visa type based on my specific situation.'
            })
        });
        
        if (!step1Response.ok) {
            console.log('❌ Step 1 failed:', step1Response.status, await step1Response.text());
            return false;
        }
        
        const step1Data = await step1Response.json();
        console.log('✅ Step 1 response:', {
            question: step1Data.question?.substring(0, 100) + '...',
            step: step1Data.step,
            isComplete: step1Data.isComplete,
            optionsCount: step1Data.options?.length || 0
        });
        
        if (step1Data.step !== 1) {
            console.log('❌ Expected Step 1, got:', step1Data.step);
            return false;
        }
        
        // Step 2: Answer the first question
        console.log('\n=== STEP 2: Answer First Question ===');
        const step2Response = await fetch(`${API_URL}/api/VisaInterview/phase2/step`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept-Language': 'en-US'
            },
            body: JSON.stringify({
                UserId: TEST_USER_ID,
                Category: 'Immigrate',
                Instructions: 'Please help me find the right visa type based on my specific situation.',
                Answer: 'A'  // Select first option
            })
        });
        
        if (!step2Response.ok) {
            console.log('❌ Step 2 failed:', step2Response.status, await step2Response.text());
            return false;
        }
        
        const step2Data = await step2Response.json();
        console.log('✅ Step 2 response:', {
            question: step2Data.question?.substring(0, 100) + '...',
            step: step2Data.step,
            isComplete: step2Data.isComplete,
            optionsCount: step2Data.options?.length || 0
        });
        
        if (step2Data.step !== 2) {
            console.log('❌ Expected Step 2, got:', step2Data.step);
            console.log('Full step2Data:', JSON.stringify(step2Data, null, 2));
            return false;
        }
        
        console.log('\n🎉 SUCCESS: Step progression is working correctly!');
        console.log('   - Step 1 completed successfully');
        console.log('   - Step 2 reached successfully');
        return true;
        
    } catch (error) {
        console.log('❌ Test failed with error:', error.message);
        return false;
    }
}

// Run the test
testStepProgression().then(success => {
    if (success) {
        console.log('\n✅ All tests passed!');
        process.exit(0);
    } else {
        console.log('\n❌ Tests failed!');
        process.exit(1);
    }
}).catch(error => {
    console.error('❌ Test execution failed:', error);
    process.exit(1);
});