const puppeteer = require('puppeteer');

// Simple manual test to verify Phase 2 button interface
async function manualPhase2Test() {
    console.log('Starting manual Phase 2 interface test...\n');
    
    const browser = await puppeteer.launch({ 
        headless: false, 
        defaultViewport: { width: 1280, height: 720 },
        slowMo: 500 // Slow down for visibility
    });
    
    const page = await browser.newPage();
    
    // Enable console logging
    page.on('console', msg => {
        console.log(`Browser: ${msg.text()}`);
    });
    
    try {
        console.log('1. Navigating to home page...');
        await page.goto('http://localhost:5161/', { waitUntil: 'networkidle0' });
        await page.waitForTimeout(3000);
        
        console.log('2. Taking screenshot of home page...');
        await page.screenshot({ path: 'home-page-screenshot.png', fullPage: true });
        
        console.log('3. Looking for page elements...');
        
        // Get page title and main content
        const title = await page.title();
        console.log(`Page title: ${title}`);
        
        // Get all clickable elements
        const clickableElements = await page.evaluate(() => {
            const elements = Array.from(document.querySelectorAll('a, button, .card, [onclick], [role="button"]'));
            return elements.map(el => ({
                tag: el.tagName,
                text: el.textContent?.substring(0, 100) || '',
                href: el.href || '',
                classes: el.className || '',
                id: el.id || ''
            })).filter(el => el.text.trim().length > 0);
        });
        
        console.log('\nClickable elements found:');
        clickableElements.forEach((el, i) => {
            console.log(`${i + 1}. ${el.tag}: "${el.text.trim()}" (classes: ${el.classes})`);
        });
        
        console.log('\n4. Looking for category buttons...');
        const categories = ['Immigrate', 'Work', 'Study', 'Family', 'Visit', 'Investment', 'Asylum'];
        
        for (const category of categories) {
            const found = await page.evaluate((cat) => {
                const elements = Array.from(document.querySelectorAll('*'));
                const element = elements.find(el => 
                    el.textContent?.includes(cat) && 
                    (el.tagName === 'BUTTON' || el.tagName === 'A' || el.classList.contains('card'))
                );
                return element ? {
                    tag: element.tagName,
                    text: element.textContent?.substring(0, 100),
                    classes: element.className
                } : null;
            }, category);
            
            if (found) {
                console.log(`✅ Found ${category}: ${found.tag} with classes "${found.classes}"`);
            } else {
                console.log(`❌ Not found: ${category}`);
            }
        }
        
        console.log('\n5. Looking for login/register links...');
        const authElements = await page.evaluate(() => {
            const patterns = ['login', 'register', 'sign up', 'sign in', 'auth'];
            const results = [];
            
            for (const pattern of patterns) {
                const elements = Array.from(document.querySelectorAll('a, button'));
                const matches = elements.filter(el => 
                    el.textContent?.toLowerCase().includes(pattern) ||
                    el.href?.toLowerCase().includes(pattern)
                );
                
                matches.forEach(match => {
                    results.push({
                        pattern,
                        tag: match.tagName,
                        text: match.textContent?.substring(0, 50),
                        href: match.href || ''
                    });
                });
            }
            
            return results;
        });
        
        console.log('Authentication elements:');
        authElements.forEach(el => {
            console.log(`- ${el.pattern}: ${el.tag} "${el.text}" (${el.href})`);
        });
        
        console.log('\n6. Manual test instructions:');
        console.log('- Browser window is open for manual inspection');
        console.log('- Screenshot saved as "home-page-screenshot.png"');
        console.log('- Try clicking on category buttons manually');
        console.log('- Check if Phase 2 interview loads correctly');
        console.log('- Verify that A/B/C buttons appear and work');
        console.log('\nPress any key to close browser...');
        
        // Keep browser open for manual inspection
        await new Promise(resolve => {
            process.stdin.once('data', resolve);
        });
        
    } catch (error) {
        console.error('Error during manual test:', error);
    } finally {
        await browser.close();
        console.log('Browser closed');
    }
}

// Run manual test
if (require.main === module) {
    manualPhase2Test().catch(console.error);
}

module.exports = { manualPhase2Test };