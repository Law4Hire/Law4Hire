// Test the improved Phase 2 interview with multiple choice answers
const testUserId = "123e4567-e89b-12d3-a456-426614174000"; 
const baseUrl = "http://localhost:5237";

console.log("Testing improved Phase 2 interview with multiple choice answers...");
console.log("Test User ID:", testUserId);

async function testPhase2Interview() {
    try {
        // Step 1: Start Phase 2 interview (initial question)
        console.log("\n=== Step 1: Starting Phase 2 Interview ===");
        const payload1 = {
            UserId: testUserId,
            Category: "Immigrate", 
            Instructions: "Please help me find the right visa type based on my specific situation."
        };
        
        const response1 = await fetch(`${baseUrl}/api/VisaInterview/phase2/step`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept-Language': 'en-US'
            },
            body: JSON.stringify(payload1)
        });
        
        if (!response1.ok) {
            throw new Error(`HTTP ${response1.status}: ${await response1.text()}`);
        }
        
        const result1 = await response1.json();
        console.log("Step 1 Response:", JSON.stringify(result1, null, 2));
        
        // Step 2: Answer with option A (family relationships)
        console.log("\n=== Step 2: Answering with Option A (Family) ===");
        const payload2 = {
            UserId: testUserId,
            Category: "Immigrate",
            Instructions: "Please help me find the right visa type based on my specific situation.",
            Answer: "A"  // Family relationships
        };
        
        const response2 = await fetch(`${baseUrl}/api/VisaInterview/phase2/step`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept-Language': 'en-US'
            },
            body: JSON.stringify(payload2)
        });
        
        if (!response2.ok) {
            throw new Error(`HTTP ${response2.status}: ${await response2.text()}`);
        }
        
        const result2 = await response2.json();
        console.log("Step 2 Response:", JSON.stringify(result2, null, 2));
        
        // Step 3: Answer another question to further narrow down
        console.log("\n=== Step 3: Further Narrowing ===");
        const payload3 = {
            UserId: testUserId,
            Category: "Immigrate",
            Instructions: "Please help me find the right visa type based on my specific situation.",
            Answer: "A"  // Continue with option A
        };
        
        const response3 = await fetch(`${baseUrl}/api/VisaInterview/phase2/step`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept-Language': 'en-US'
            },
            body: JSON.stringify(payload3)
        });
        
        if (!response3.ok) {
            throw new Error(`HTTP ${response3.status}: ${await response3.text()}`);
        }
        
        const result3 = await response3.json();
        console.log("Step 3 Response:", JSON.stringify(result3, null, 2));
        
        console.log("\n=== Test completed successfully! ===");
        
    } catch (error) {
        console.error("Test failed:", error.message);
        if (error.stack) {
            console.error("Stack trace:", error.stack);
        }
    }
}

// Run the test
testPhase2Interview();