const puppeteer = require('puppeteer');

async function verifyStepProgression() {
    console.log('🔍 Verifying Phase 2 Step Progression...');
    
    const browser = await puppeteer.launch({ 
        headless: false,
        defaultViewport: { width: 1280, height: 720 }
    });
    
    const page = await browser.newPage();
    
    try {
        // Open the test HTML file
        await page.goto(`file://${__dirname}/test-step-progression.html`);
        
        // Wait for page to load
        await page.waitForTimeout(2000);
        
        // Click the test button
        await page.click('button');
        
        // Wait for the test to complete (give it time to make API calls)
        await page.waitForTimeout(10000);
        
        // Check the results
        const results = await page.evaluate(() => {
            const resultsDiv = document.getElementById('results');
            return resultsDiv.innerHTML;
        });
        
        console.log('Test Results:');
        console.log(results);
        
        // Check if we successfully reached Step 2
        const step2Success = results.includes('Step 2 reached successfully') || 
                            results.includes('step": 2');
        
        if (step2Success) {
            console.log('✅ SUCCESS: Step progression is working!');
            return true;
        } else {
            console.log('❌ FAILED: Step progression is not working');
            return false;
        }
        
    } catch (error) {
        console.log('❌ Error during verification:', error.message);
        return false;
    } finally {
        await browser.close();
    }
}

// Run verification
verifyStepProgression().then(success => {
    process.exit(success ? 0 : 1);
}).catch(error => {
    console.error('❌ Verification failed:', error);
    process.exit(1);
});