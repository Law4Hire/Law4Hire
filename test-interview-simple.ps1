# Simple PowerShell test for Phase 2 interview
$baseUrl = "http://localhost:5237"
$testUserId = "123e4567-e89b-12d3-a456-426614174000"

Write-Host "Testing Phase 2 Interview..." -ForegroundColor Green

# Check if API is running
try {
    $healthCheck = Invoke-RestMethod -Uri "$baseUrl/api/health" -Method GET -ErrorAction Stop
    Write-Host "API is running" -ForegroundColor Green
} catch {
    Write-Host "API is not running. Please start the API project first." -ForegroundColor Red
    Start-Process -FilePath "cmd" -ArgumentList "/c", "dotnet run --project Law4Hire.API --urls http://localhost:5237" -WindowStyle Minimized
    Start-Sleep 10
}

# Test Phase 2 Step 1
$payload1 = @{
    UserId = $testUserId
    Category = "Immigrate"  
    Instructions = "Please help me find the right visa type based on my specific situation."
} | ConvertTo-Json

try {
    $headers = @{
        'Content-Type' = 'application/json'
        'Accept-Language' = 'en-US'
    }
    
    Write-Host "`nStep 1: Starting interview..." -ForegroundColor Yellow
    $response1 = Invoke-RestMethod -Uri "$baseUrl/api/VisaInterview/phase2/step" -Method POST -Body $payload1 -Headers $headers
    Write-Host "Response: $($response1 | ConvertTo-Json -Depth 3)" -ForegroundColor Cyan
    
    # Test Phase 2 Step 2 with option A
    $payload2 = @{
        UserId = $testUserId
        Category = "Immigrate"
        Instructions = "Please help me find the right visa type based on my specific situation."
        Answer = "A"
    } | ConvertTo-Json
    
    Write-Host "`nStep 2: Answering with option A..." -ForegroundColor Yellow  
    $response2 = Invoke-RestMethod -Uri "$baseUrl/api/VisaInterview/phase2/step" -Method POST -Body $payload2 -Headers $headers
    Write-Host "Response: $($response2 | ConvertTo-Json -Depth 3)" -ForegroundColor Cyan
    
    Write-Host "`nTest completed successfully!" -ForegroundColor Green
    
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = [System.IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
        $responseText = $reader.ReadToEnd()
        Write-Host "Response body: $responseText" -ForegroundColor Red
    }
}