using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Law4Hire.GovScraper;

public class VisaWizardTester
{
    private readonly ILogger<VisaWizardTester> _logger;
    private readonly Law4HireDbContext _dbContext;
    private IWebDriver? _driver;
    private WebDriverWait? _wait;
    private readonly List<string> _countries = new();
    
    // Travel purposes as they appear in the dropdown
    private readonly List<string> _travelPurposes = new()
    {
        "Tourism or Visit",
        "Business or Employment", 
        "Study or Exchange",
        "Traveling through the U.S. to another country",
        "Immigrate"
    };

    public VisaWizardTester(ILogger<VisaWizardTester> logger, Law4HireDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task TestVisaWizardAsync(int countryLimit = 5)
    {
        try
        {
            InitializeDriver();
            await LoadCountriesFromDatabaseAsync();
            
            _logger.LogInformation($"🧪 STARTING VISA WIZARD SYSTEMATIC TESTING");
            _logger.LogInformation($"📊 Will test {Math.Min(countryLimit, _countries.Count)} countries");
            _logger.LogInformation($"🎯 Testing all combinations of purposes and questions");
            
            var testedCountries = 0;
            var totalResults = 0;
            
            foreach (var country in _countries.Take(countryLimit))
            {
                testedCountries++;
                _logger.LogInformation($"🌍 [{testedCountries}/{Math.Min(countryLimit, _countries.Count)}] Testing country: {country}");
                
                var countryResults = await TestCountryAsync(country);
                totalResults += countryResults;
                
                _logger.LogInformation($"✅ Country {country} completed: {countryResults} results captured");
            }
            
            _logger.LogInformation($"🎉 TESTING COMPLETED!");
            _logger.LogInformation($"📊 Total results captured: {totalResults}");
        }
        finally
        {
            CleanupDriver();
        }
    }

    public async Task TestSpecificCountriesAsync(List<string> countriesToTest)
    {
        try
        {
            // Clear existing VisaWizard data first
            await ClearExistingVisaWizardDataAsync();
            
            InitializeDriver();
            
            _logger.LogInformation($"🧪 STARTING COMPREHENSIVE COUNTRY TESTING");
            _logger.LogInformation($"📊 Will test {countriesToTest.Count} countries systematically");
            _logger.LogInformation($"🎯 Testing all combinations of purposes and questions");
            _logger.LogInformation($"🔄 Browser will refresh every 10 countries for stability");
            
            var testedCountries = 0;
            var totalResults = 0;
            var failedCountries = new List<string>();
            
            foreach (var country in countriesToTest)
            {
                testedCountries++;
                _logger.LogInformation($"🌍 [{testedCountries}/{countriesToTest.Count}] Testing country: {country}");
                
                // Refresh browser every 3 countries to prevent crashes (more frequent)
                if (testedCountries > 1 && (testedCountries - 1) % 3 == 0)
                {
                    _logger.LogInformation($"🔄 Refreshing browser session after {testedCountries - 1} countries for stability");
                    CleanupDriver();
                    await Task.Delay(3000); // Wait longer for cleanup
                    InitializeDriver();
                }
                
                var countryResults = await TestCountryWithRetryAsync(country);
                totalResults += countryResults;
                
                if (countryResults > 0)
                {
                    _logger.LogInformation($"✅ Country {country} completed: {countryResults} results captured");
                }
                else
                {
                    _logger.LogWarning($"❌ Country {country} failed to capture any results");
                    failedCountries.Add(country);
                }
                
                // Progress checkpoint every 25 countries
                if (testedCountries % 25 == 0)
                {
                    _logger.LogInformation($"📊 PROGRESS CHECKPOINT: {testedCountries}/{countriesToTest.Count} countries processed, {totalResults} total results");
                }
            }
            
            _logger.LogInformation($"🎉 COMPREHENSIVE COUNTRY TESTING COMPLETED!");
            _logger.LogInformation($"📊 Total results captured: {totalResults}");
            _logger.LogInformation($"✅ Successfully processed: {countriesToTest.Count - failedCountries.Count}/{countriesToTest.Count} countries");
            
            if (failedCountries.Count > 0)
            {
                _logger.LogWarning($"⚠️ Failed countries ({failedCountries.Count}): {string.Join(", ", failedCountries.Take(10))}{(failedCountries.Count > 10 ? "..." : "")}");
            }
        }
        finally
        {
            CleanupDriver();
        }
    }

    private async Task ClearExistingVisaWizardDataAsync()
    {
        try
        {
            _logger.LogInformation("🗑️ Clearing existing VisaWizard data...");
            
            var existingCount = await _dbContext.VisaWizards.CountAsync();
            if (existingCount > 0)
            {
                _dbContext.VisaWizards.RemoveRange(_dbContext.VisaWizards);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"✅ Cleared {existingCount} existing VisaWizard records");
            }
            else
            {
                _logger.LogInformation("📝 No existing VisaWizard data to clear");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing existing VisaWizard data");
            throw;
        }
    }

    private async Task<int> TestCountryWithRetryAsync(string country)
    {
        const int maxRetries = 2;
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (attempt > 1)
                {
                    _logger.LogInformation($"  🔄 Retry attempt {attempt} for country: {country}");
                    await Task.Delay(5000); // Wait before retry
                }
                
                var results = await TestCountryAsync(country);
                
                if (results > 0)
                {
                    return results; // Success - return results
                }
                else if (attempt == maxRetries)
                {
                    _logger.LogWarning($"  ⚠️ Country {country} returned 0 results after {maxRetries} attempts");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"  ❌ Attempt {attempt} failed for country '{country}': {ex.Message}");
                
                if (attempt == maxRetries)
                {
                    _logger.LogError($"  💥 Country {country} failed all {maxRetries} attempts");
                    return 0;
                }
            }
        }
        
        return 0;
    }

    private async Task<int> TestCountryAsync(string country)
    {
        var results = 0;
        
        foreach (var purpose in _travelPurposes)
        {
            _logger.LogInformation($"  🎯 Testing purpose: {purpose}");
            
            try
            {
                var purposeResults = await TestCountryPurposeAsync(country, purpose);
                results += purposeResults;
                _logger.LogInformation($"    ✅ Purpose '{purpose}' completed: {purposeResults} results");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"    ⚠️ Error testing purpose '{purpose}' for country '{country}': {ex.Message}");
                // Continue with next purpose instead of failing the entire country
                continue;
            }
        }
        
        return results;
    }

    private async Task<int> TestCountryPurposeAsync(string country, string purpose)
    {
        var results = 0;
        
        // Start fresh wizard session
        if (!await NavigateToWizardAndSelectCountryAsync(country))
        {
            _logger.LogWarning($"    ❌ Could not select country: {country}");
            return 0;
        }
        
        // Select the purpose
        if (!await SelectPurposeAsync(purpose))
        {
            _logger.LogWarning($"    ❌ Could not select purpose: {purpose}");
            return 0;
        }
        
        // Click Find a Visa button
        if (!await ClickFindVisaButtonAsync())
        {
            _logger.LogWarning($"    ❌ Could not click Find a Visa button");
            return 0;
        }
        
        await Task.Delay(1000); // Wait for response
        
        // Check if we got direct results or questions
        if (await HasDirectResultsAsync())
        {
            // Direct results - capture them
            var links = await CaptureLearnMoreLinksAsync();
            await SaveResultAsync(country, purpose, null, null, null, null, links);
            results++;
            _logger.LogInformation($"      📝 Direct result captured for {country} - {purpose}");
        }
        else if (await HasQuestionsAsync())
        {
            // Questions appeared - systematically explore them
            results += await ExploreQuestionsAsync(country, purpose);
        }
        else
        {
            _logger.LogWarning($"      ⚠️ Unexpected state for {country} - {purpose}");
        }
        
        return results;
    }

    private async Task<int> ExploreQuestionsAsync(string country, string purpose)
    {
        var results = 0;
        
        try
        {
            var firstQuestions = await GetAvailableQuestionOptionsAsync();
            
            if (firstQuestions.Count == 0)
            {
                _logger.LogInformation($"        📝 No questions found for {country} - {purpose}");
                return results;
            }
            
            // Systematically explore ALL question combinations
            foreach (var (firstQuestionText, firstAnswerOptions) in firstQuestions)
            {
                _logger.LogInformation($"        📋 Processing Q1: '{firstQuestionText}' with {firstAnswerOptions.Count} answers");
                
                foreach (var firstAnswer in firstAnswerOptions) // Process ALL answers, not just first 3
                {
                    _logger.LogInformation($"        🔍 Testing Q1: '{firstQuestionText.Substring(0, Math.Min(50, firstQuestionText.Length))}...' A1: '{firstAnswer}'");
                    
                    // Reset and navigate to this combination
                    if (!await NavigateToWizardAndSelectCountryAsync(country)) continue;
                    if (!await SelectPurposeAsync(purpose)) continue;
                    if (!await ClickFindVisaButtonAsync()) continue;
                    await Task.Delay(1000);
                    
                    // Select first answer
                    if (!await SelectQuestionAnswerAsync(firstAnswer)) 
                    {
                        _logger.LogWarning($"        ❌ Could not select answer: {firstAnswer}");
                        continue;
                    }
                    
                    await Task.Delay(1000); // Wait for page to update
                    
                    // CRITICAL FIX: Submit the form to reveal second level questions
                    if (!await ClickFindVisaButtonAsync())
                    {
                        _logger.LogWarning($"        ❌ Could not submit form after selecting Q1 answer: {firstAnswer}");
                        continue;
                    }
                    
                    await Task.Delay(2000); // Wait longer for form submission and page update
                    
                    // Check if there are more questions after form submission
                    var hasMoreQuestions = await HasSecondQuestionAsync();
                    
                    if (hasMoreQuestions)
                    {
                        var secondQuestions = await GetAvailableQuestionOptionsAsync();
                        
                        if (secondQuestions.Count > 0)
                        {
                            // Process ALL second-level questions and answers
                            foreach (var (secondQuestionText, secondAnswerOptions) in secondQuestions)
                            {
                                foreach (var secondAnswer in secondAnswerOptions) // Process ALL second answers
                                {
                                    _logger.LogInformation($"          🔍 Testing Q2: '{secondQuestionText.Substring(0, Math.Min(30, secondQuestionText.Length))}...' A2: '{secondAnswer}'");
                                    
                                    // Reset and navigate to this full combination
                                    if (!await NavigateToWizardAndSelectCountryAsync(country)) continue;
                                    if (!await SelectPurposeAsync(purpose)) continue;
                                    if (!await ClickFindVisaButtonAsync()) continue;
                                    await Task.Delay(1000);
                                    if (!await SelectQuestionAnswerAsync(firstAnswer)) continue;
                                    await Task.Delay(1000);
                                    
                                    // CRITICAL FIX: Submit form after first answer to reveal second question
                                    if (!await ClickFindVisaButtonAsync()) continue;
                                    await Task.Delay(1500);
                                    
                                    if (!await SelectQuestionAnswerAsync(secondAnswer)) continue;
                                    await Task.Delay(1000);
                                    
                                    // Get final results after both answers
                                    if (await ClickFindVisaButtonAsync())
                                    {
                                        await Task.Delay(1500);
                                        var links = await CaptureLearnMoreLinksAsync();
                                        await SaveResultAsync(country, purpose, firstQuestionText, firstAnswer, secondQuestionText, secondAnswer, links);
                                        results++;
                                        _logger.LogInformation($"          ✅ Saved 2-level result #{results}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation($"        ⚠️ Expected second question but none found for: {firstAnswer}");
                        }
                    }
                    else
                    {
                        // No second question - this first answer leads directly to results
                        _logger.LogInformation($"        📝 Q1 answer '{firstAnswer}' leads directly to results");
                        
                        if (await ClickFindVisaButtonAsync())
                        {
                            await Task.Delay(1500);
                            var links = await CaptureLearnMoreLinksAsync();
                            await SaveResultAsync(country, purpose, firstQuestionText, firstAnswer, null, null, links);
                            results++;
                            _logger.LogInformation($"        ✅ Saved 1-level result #{results}");
                        }
                    }
                }
            }
            
            _logger.LogInformation($"        🎯 Completed exploration for {country} - {purpose}: {results} total results");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error exploring questions for {country} - {purpose}");
        }
        
        return results;
    }

    private async Task<bool> NavigateToWizardAndSelectCountryAsync(string country)
    {
        try
        {
            _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
            await Task.Delay(2000);
            
            // Find country input field
            var countryInput = await WaitForElementAsync(By.CssSelector("input.autocomplete_input, input[type='text'][placeholder*='country' i], input[type='text'][id*='country' i]"), TimeSpan.FromSeconds(10));
            if (countryInput == null)
            {
                _logger.LogWarning("Could not find country input field");
                return false;
            }
            
            // Clear and type country name
            countryInput.Clear();
            countryInput.SendKeys(country);
            await Task.Delay(1000);
            
            _logger.LogInformation($"      🌍 Searching for country: {country}");
            
            // Look for autocomplete dropdown and select the country
            try
            {
                var autocompleteDropdown = await WaitForElementAsync(
                    By.CssSelector(".ui-autocomplete.ui-menu.ui-widget.ui-widget-content.ui-corner-all"), 
                    TimeSpan.FromSeconds(5)
                );
                
                if (autocompleteDropdown != null)
                {
                    var menuItems = autocompleteDropdown.FindElements(By.CssSelector("li.ui-menu-item[role='menuitem']"));
                    _logger.LogInformation($"      📋 Found {menuItems.Count} autocomplete options");
                    
                    foreach (var menuItem in menuItems)
                    {
                        var anchor = menuItem.FindElement(By.CssSelector("a.ui-corner-all"));
                        var countryText = anchor.Text.Trim();
                        
                        if (countryText.Equals(country, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation($"      ✅ Found and selecting: {countryText}");
                            anchor.Click();
                            await Task.Delay(100);
                            return true;
                        }
                    }
                    
                    _logger.LogWarning($"      ❌ Country '{country}' not found in autocomplete options");
                }
                else
                {
                    _logger.LogWarning($"      ❌ No autocomplete dropdown appeared for: {country}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error selecting country from autocomplete: {country}");
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error navigating to wizard for country: {country}");
            return false;
        }
    }

    private async Task<bool> SelectPurposeAsync(string purpose)
    {
        try
        {
            var purposeSelect = await WaitForElementAsync(By.Id("why_travel_to_us"), TimeSpan.FromSeconds(10));
            if (purposeSelect == null) 
            {
                _logger.LogWarning("      ❌ Could not find why_travel_to_us dropdown");
                return false;
            }
            
            var selectElement = new SelectElement(purposeSelect);
            
            // Log available options for debugging
            var options = selectElement.Options;
            _logger.LogInformation($"      📋 Available purpose options: {string.Join(", ", options.Select(o => o.Text))}");
            
            // Try to find matching option
            var matchingOption = options.FirstOrDefault(o => o.Text.Trim().Equals(purpose, StringComparison.OrdinalIgnoreCase));
            if (matchingOption != null)
            {
                selectElement.SelectByText(matchingOption.Text);
                await Task.Delay(100);
                _logger.LogInformation($"      ✅ Selected purpose: {matchingOption.Text}");
                return true;
            }
            else
            {
                _logger.LogWarning($"      ❌ Could not find purpose option: {purpose}");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error selecting purpose: {purpose}");
            return false;
        }
    }

    private async Task<bool> ClickFindVisaButtonAsync()
    {
        try
        {
            // Handle any existing alerts first
            await HandleAlertsAsync();
            
            // Try multiple selectors for the Find a Visa button
            var selectors = new[]
            {
                By.XPath("//button[contains(text(), 'Find a Visa')]"),
                By.XPath("//input[@type='button' and contains(@value, 'Find a Visa')]"),
                By.XPath("//input[@type='submit' and contains(@value, 'Find a Visa')]"),
                By.XPath("//a[contains(text(), 'Find a Visa')]"),
                By.CssSelector("button[onclick*='submitForm']"),
                By.CssSelector("input[type='submit']"),
                By.CssSelector("button[type='submit']"),
                By.XPath("//*[contains(@class, 'button') and contains(text(), 'Find')]"),
                By.XPath("//*[contains(@onclick, 'submit') or contains(@onclick, 'wizard')]")
            };

            foreach (var selector in selectors)
            {
                try
                {
                    var element = await WaitForElementAsync(selector, TimeSpan.FromSeconds(2));
                    if (element != null && element.Displayed && element.Enabled)
                    {
                        _logger.LogInformation($"      🎯 Found button using selector: {selector}");
                        element.Click();
                        await Task.Delay(500); // Give more time for processing
                        
                        // Handle any alerts that might appear after clicking
                        await HandleAlertsAsync();
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug($"      Selector failed: {selector} - {ex.Message}");
                    // Try to handle alerts even if selector failed
                    await HandleAlertsAsync();
                    continue;
                }
            }

            // If no button found, log available buttons for debugging
            try
            {
                var allButtons = _driver!.FindElements(By.TagName("button"));
                var allInputs = _driver!.FindElements(By.CssSelector("input[type='button'], input[type='submit']"));
                
                _logger.LogWarning($"      🔍 No Find a Visa button found. Available buttons: {allButtons.Count}, inputs: {allInputs.Count}");
                
                foreach (var btn in allButtons.Take(5))
                {
                    _logger.LogInformation($"        Button: '{btn.Text}' | onclick: '{btn.GetAttribute("onclick")}' | class: '{btn.GetAttribute("class")}'");
                }
                
                foreach (var inp in allInputs.Take(5))
                {
                    _logger.LogInformation($"        Input: '{inp.GetAttribute("value")}' | onclick: '{inp.GetAttribute("onclick")}' | class: '{inp.GetAttribute("class")}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"Error logging debug info: {ex.Message}");
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clicking Find a Visa button");
            // Try to handle alerts even if there was an error
            await HandleAlertsAsync();
            return false;
        }
    }

    private async Task<bool> HasDirectResultsAsync()
    {
        try
        {
            var visaOutcomeScreen = await WaitForElementAsync(By.CssSelector(".visaoutcomescreen"), TimeSpan.FromSeconds(2));
            return visaOutcomeScreen != null;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasQuestionsAsync()
    {
        try
        {
            // Look for radio buttons or question elements
            var radioButtons = _driver!.FindElements(By.CssSelector("input[type='radio']"));
            return radioButtons.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> HasSecondQuestionAsync()
    {
        try
        {
            await Task.Delay(500); // Wait for potential second question to appear
            var radioButtons = _driver!.FindElements(By.CssSelector("input[type='radio']"));
            return radioButtons.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<(string questionText, List<string> answerOptions)>> GetAvailableQuestionOptionsAsync()
    {
        var questions = new List<(string, List<string>)>();
        
        try
        {
            // Handle any existing alerts first
            await HandleAlertsAsync();
            
            // Find all radio button groups
            var radioButtons = _driver!.FindElements(By.CssSelector("input[type='radio']"));
            
            if (radioButtons.Count == 0)
            {
                _logger.LogInformation($"      📝 No radio buttons found on current page");
                return questions;
            }
            
            var groupedByName = radioButtons.GroupBy(rb => rb.GetAttribute("name"));
            
            foreach (var group in groupedByName)
            {
                var questionText = "Unknown Question";
                var answerOptions = new List<string>();
                
                // Try to find the question text by looking for nearby text elements
                try
                {
                    var firstRadio = group.First();
                    var radioId = firstRadio.GetAttribute("id");
                    
                    // Try multiple strategies to find the question text
                    var questionCandidates = new[]
                    {
                        // Look for div with id ending in "question" or containing "question" 
                        By.XPath("//div[contains(@id, 'question') and contains(., text())]"),
                        By.XPath("//div[@id='question1']"),
                        By.XPath("//div[@id='question2']"),
                        By.XPath("//div[@id='question3']"),
                        By.XPath("//div[@id='question4']"),
                        By.XPath("//div[@id='question5']"),
                        // Look for preceding text elements
                        By.XPath($"//label[@for='{radioId}']/../preceding-sibling::div[contains(text(), '?') or contains(text(), 'are') or contains(text(), 'will') or contains(text(), 'do')]"),
                        By.XPath($"//label[@for='{radioId}']/../preceding-sibling::p[contains(text(), '?') or contains(text(), 'are') or contains(text(), 'will') or contains(text(), 'do')]"),
                        By.XPath($"//label[@for='{radioId}']/../preceding-sibling::h3"),
                        By.XPath($"//label[@for='{radioId}']/../preceding-sibling::strong"),
                        // Look for parent container with question text
                        By.XPath($"//label[@for='{radioId}']/../../div[contains(text(), '?') or contains(text(), 'are') or contains(text(), 'will')]"),
                        // Fallback to looking for any visible text before the radio group
                        By.XPath("//div[contains(@class, 'question') or contains(@class, 'prompt')]")
                    };
                    
                    foreach (var candidate in questionCandidates)
                    {
                        try
                        {
                            var questionElement = _driver.FindElement(candidate);
                            var text = questionElement.Text.Trim();
                            if (!string.IsNullOrEmpty(text) && text.Length > 10 && 
                                (text.Contains("?") || text.Contains("are") || text.Contains("will") || text.Contains("do"))) 
                            {
                                questionText = text;
                                _logger.LogInformation($"      🎯 Found question text: '{text.Substring(0, Math.Min(60, text.Length))}...'");
                                break;
                            }
                        }
                        catch { continue; }
                    }
                }
                catch
                {
                    // Use the name attribute as fallback
                    questionText = group.Key ?? "Unknown Question";
                }
                
                // Get all answer options for this question
                foreach (var radio in group)
                {
                    try
                    {
                        var radioId = radio.GetAttribute("id");
                        if (!string.IsNullOrEmpty(radioId))
                        {
                            var label = _driver.FindElement(By.XPath($"//label[@for='{radioId}']"));
                            var labelText = label.Text.Trim();
                            if (!string.IsNullOrEmpty(labelText))
                            {
                                answerOptions.Add(labelText);
                            }
                        }
                        else
                        {
                            // Fallback to value attribute
                            var value = radio.GetAttribute("value");
                            if (!string.IsNullOrEmpty(value))
                            {
                                answerOptions.Add(value);
                            }
                        }
                    }
                    catch
                    {
                        // Try value attribute as final fallback
                        try
                        {
                            var value = radio.GetAttribute("value");
                            if (!string.IsNullOrEmpty(value))
                            {
                                answerOptions.Add(value);
                            }
                        }
                        catch { /* Skip this radio button */ }
                    }
                }
                
                if (answerOptions.Count > 0)
                {
                    questions.Add((questionText, answerOptions));
                    _logger.LogInformation($"      📋 Found question: '{questionText}' with {answerOptions.Count} answers: {string.Join(", ", answerOptions.Take(3))}...");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting question options");
            await HandleAlertsAsync();
        }
        
        return questions;
    }

    private async Task<bool> SelectQuestionAnswerAsync(string answer)
    {
        try
        {
            // Handle any existing alerts first
            await HandleAlertsAsync();
            
            // Find radio button by label text or value
            var radioButtons = _driver!.FindElements(By.CssSelector("input[type='radio']"));
            
            foreach (var radio in radioButtons)
            {
                try
                {
                    // Try to find associated label
                    var label = _driver.FindElement(By.XPath($"//label[@for='{radio.GetAttribute("id")}']"));
                    if (label.Text.Trim().Equals(answer, StringComparison.OrdinalIgnoreCase))
                    {
                        radio.Click();
                        await Task.Delay(100);
                        
                        // Handle any alerts that might appear after clicking
                        await HandleAlertsAsync();
                        return true;
                    }
                }
                catch
                {
                    // Try value attribute
                    if (radio.GetAttribute("value")?.Equals(answer, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        radio.Click();
                        await Task.Delay(100);
                        
                        // Handle any alerts that might appear after clicking
                        await HandleAlertsAsync();
                        return true;
                    }
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error selecting answer: {answer}");
            // Try to handle alerts even if there was an error
            await HandleAlertsAsync();
            return false;
        }
    }

    private async Task<string> CaptureLearnMoreLinksAsync()
    {
        try
        {
            var visaOutcomeScreen = await WaitForElementAsync(By.CssSelector(".visaoutcomescreen"), TimeSpan.FromSeconds(5));
            if (visaOutcomeScreen == null) return "";
            
            var links = visaOutcomeScreen.FindElements(By.TagName("a"));
            var learnMoreLinks = new List<string>();
            
            foreach (var link in links)
            {
                var href = link.GetAttribute("href");
                var text = link.Text.Trim();
                
                if (!string.IsNullOrEmpty(href) && (text.Contains("Learn More", StringComparison.OrdinalIgnoreCase) || text.Contains("More Information", StringComparison.OrdinalIgnoreCase)))
                {
                    learnMoreLinks.Add(href);
                }
            }
            
            return string.Join(", ", learnMoreLinks.Distinct());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error capturing Learn More links");
            return "";
        }
    }

    private async Task SaveResultAsync(string country, string purpose, string? firstQuestion, string? firstAnswer, string? secondQuestion, string? secondAnswer, string links)
    {
        try
        {
            var result = new VisaWizard
            {
                Country = country,
                Purpose = purpose,
                Answer1 = firstAnswer,
                Answer2 = secondAnswer,
                HasFollowUp = !string.IsNullOrEmpty(secondQuestion),
                LearnMoreLinks = links,
                SessionId = Guid.NewGuid(),
                StepNumber = 1,
                IsCompleteSession = true,
                CreatedAt = DateTime.UtcNow
            };
            
            _dbContext.VisaWizards.Add(result);
            await _dbContext.SaveChangesAsync();
            
            var q1Display = firstQuestion?.Length > 30 ? firstQuestion.Substring(0, 30) + "..." : firstQuestion ?? "None";
            var q2Display = secondQuestion?.Length > 30 ? secondQuestion.Substring(0, 30) + "..." : secondQuestion ?? "None";
            _logger.LogInformation($"        💾 Saved result: {country} | {purpose} | Q1: {q1Display} | Q2: {q2Display} | Links: {links.Split(',').Length}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving wizard result");
        }
    }

    private async Task LoadCountriesFromDatabaseAsync()
    {
        try
        {
            var countries = await _dbContext.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .Select(c => c.Name)
                .ToListAsync();
            
            _countries.AddRange(countries);
            _logger.LogInformation($"Loaded {_countries.Count} countries from database");
            
            if (_countries.Count == 0)
            {
                _logger.LogWarning("No countries found in database. Run the country scraper first.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading countries from database");
        }
    }

    private void InitializeDriver()
    {
        _logger.LogInformation("Initializing Chrome WebDriver for visa wizard testing...");
        
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--disable-web-security");
        options.AddArgument("--disable-features=VizDisplayCompositor");
        options.AddArgument("--memory-pressure-off");
        options.AddArgument("--max_old_space_size=4096");
        options.AddArgument("--disable-background-timer-throttling");
        options.AddArgument("--disable-backgrounding-occluded-windows");
        options.AddArgument("--disable-renderer-backgrounding");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-plugins");
        options.AddArgument("--disable-images");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        
        // Add unique user data directory to avoid conflicts
        var tempUserDataDir = Path.Combine(Path.GetTempPath(), "ChromeUserDataVisaTester_" + Guid.NewGuid().ToString());
        options.AddArgument($"--user-data-dir={tempUserDataDir}");
        
        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
        
        _logger.LogInformation("Chrome WebDriver initialized successfully with enhanced stability options");
    }

    private void CleanupDriver()
    {
        try
        {
            _driver?.Quit();
            _driver?.Dispose();
            _logger.LogInformation("WebDriver cleaned up successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during WebDriver cleanup");
        }
    }

    private async Task<IWebElement?> WaitForElementAsync(By by, TimeSpan timeout)
    {
        try
        {
            if (_driver == null) return null;
            var wait = new WebDriverWait(_driver, timeout);
            return await Task.Run(() => wait.Until(driver => driver.FindElement(by)));
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    private async Task HandleAlertsAsync()
    {
        try
        {
            if (_driver == null) return;
            
            // Try to switch to alert and handle it
            var alert = _driver.SwitchTo().Alert();
            var alertText = alert.Text;
            _logger.LogInformation($"      📢 Handling alert: {alertText}");
            
            // Accept the alert (click OK)
            alert.Accept();
            await Task.Delay(500); // Wait after dismissing alert
        }
        catch (NoAlertPresentException)
        {
            // No alert present - this is normal
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Error handling alert: {ex.Message}");
        }
    }
}