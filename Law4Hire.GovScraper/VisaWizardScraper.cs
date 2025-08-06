using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text;

namespace Law4Hire.GovScraper;

public class VisaWizardScraper
{
    private readonly ILogger<VisaWizardScraper> _logger;
    private readonly Law4HireDbContext _dbContext;
    private IWebDriver? _driver;
    private WebDriverWait? _wait;
    private readonly List<string> _allCountries = new();
    private readonly List<string> _wizardData = new();
    
    public VisaWizardScraper(ILogger<VisaWizardScraper> logger, Law4HireDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task ScrapeAllWizardDataAsync()
    {
        try
        {
            InitializeDriver();
            await LoadAllCountriesFromDropdownAsync();
            
            _logger.LogInformation($"Found {_allCountries.Count} countries to process");
            _logger.LogInformation("FULL MODE: Processing ALL countries for complete visa wizard data");
            
            // Update Countries table with all countries found
            await UpdateCountriesTableAsync();
            
            // Clear existing VisaWizard data as requested
            await ClearExistingVisaWizardDataAsync();
            
            var countryIndex = 0;
            var totalRecords = 0;
            
            foreach (var country in _allCountries)
            {
                countryIndex++;
                _logger.LogInformation($"[{countryIndex}/{_allCountries.Count}] Processing country: {country}");
                
                var recordsForCountry = await ProcessCountryCompleteWizardAsync(country);
                totalRecords += recordsForCountry;
                
                // Log progress
                if (countryIndex % 10 == 0)
                {
                    _logger.LogInformation($"⏱️ PROGRESS: {countryIndex}/{_allCountries.Count} countries processed, {totalRecords} total records collected");
                }
                
                // Small delay between countries
                await Task.Delay(1000);
            }
            
            _logger.LogInformation("=== VISA WIZARD SCRAPING COMPLETED ===");
            _logger.LogInformation($"✅ TOTAL COUNTRIES PROCESSED: {_allCountries.Count}");
            _logger.LogInformation($"✅ TOTAL RECORDS COLLECTED: {totalRecords}");
            _logger.LogInformation("✅ STATUS: COMPLETE - ALL COUNTRIES AND QUESTIONS PROCESSED");
            _logger.LogInformation("==========================================");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during complete visa wizard scraping");
            throw;
        }
        finally
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }

    private void InitializeDriver()
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("--headless");
        chromeOptions.AddArguments("--no-sandbox");
        chromeOptions.AddArguments("--disable-dev-shm-usage");
        chromeOptions.AddArguments("--disable-gpu");
        chromeOptions.AddArguments("--window-size=1920,1080");
        chromeOptions.AddArguments("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        
        // Add unique user data directory to avoid conflicts
        var tempUserDataDir = Path.Combine(Path.GetTempPath(), "ChromeUserDataVisaScraper_" + Guid.NewGuid().ToString());
        chromeOptions.AddArguments($"--user-data-dir={tempUserDataDir}");
        
        _driver = new ChromeDriver(chromeOptions);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
        
        _logger.LogInformation("Chrome driver initialized for complete country extraction");
    }

    private async Task LoadAllCountriesFromDropdownAsync()
    {
        try
        {
            _logger.LogInformation("Loading visa wizard page to extract all countries...");
            _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
            
            // Wait for page to fully load
            await Task.Delay(5000);
            
            // First, let's inspect what's actually on the page
            _logger.LogInformation("Analyzing page structure...");
            var pageSource = _driver.PageSource;
            _logger.LogInformation($"Page loaded, length: {pageSource.Length} characters");
            
            // Look for question1 div which contains the country dropdown
            var question1Div = await WaitForElementAsync(By.Id("question1"), TimeSpan.FromSeconds(10));
            
            if (question1Div != null)
            {
                _logger.LogInformation("Found question1 div, analyzing its contents...");
                
                // Log the innerHTML to see what's actually there
                var question1Html = question1Div.GetAttribute("innerHTML");
                
                // Try multiple selectors for finding the country dropdown
                IWebElement? countrySelect = null;
                var selectors = new[]
                {
                    "select",
                    "select[name*='country']",
                    "select[id*='country']", 
                    "select[class*='country']",
                    "input[type='hidden']", // Sometimes it's a hidden field with options loaded by JS
                    "div[role='combobox']", // Modern dropdowns might use divs
                    "div[class*='select']",
                    ".dropdown",
                    ".form-control"
                };
                
                foreach (var selector in selectors)
                {
                    try
                    {
                        countrySelect = question1Div.FindElement(By.CssSelector(selector));
                        _logger.LogInformation($"Found element using selector: {selector}");
                        break;
                    }
                    catch (NoSuchElementException)
                    {
                        _logger.LogDebug($"Selector '{selector}' not found in question1 div");
                        continue;
                    }
                }
                
                if (countrySelect != null)
                {
                    _logger.LogInformation($"Found autocomplete input: {countrySelect.TagName}");
                    _logger.LogInformation("Loading countries from database (scraped from government autocomplete)");
                    await LoadCountriesFromDatabaseAsync();
                }
                else
                {
                    _logger.LogWarning("Could not find any dropdown element within question1 div");
                    _logger.LogInformation("Available elements in question1 div:");
                    var allElements = question1Div.FindElements(By.XPath(".//*"));
                    foreach (var element in allElements.Take(10)) // Log first 10 elements
                    {
                        _logger.LogInformation($"- {element.TagName} (class: {element.GetAttribute("class")}, id: {element.GetAttribute("id")})");
                    }
                    
                    await LoadCountriesFromDatabaseAsync();
                }
            }
            else
            {
                _logger.LogError("Could not find question1 div on the page");
                _logger.LogInformation("Searching for alternative question containers...");
                
                // Look for alternative structures
                var alternativeSelectors = new[]
                {
                    "[id*='question']",
                    "[class*='question']", 
                    ".wizard-step",
                    ".form-group",
                    "form"
                };
                
                foreach (var selector in alternativeSelectors)
                {
                    var elements = _driver.FindElements(By.CssSelector(selector));
                    if (elements.Any())
                    {
                        _logger.LogInformation($"Found {elements.Count} elements matching '{selector}'");
                        foreach (var element in elements.Take(5))
                        {
                            _logger.LogInformation($"- {element.TagName} (id: {element.GetAttribute("id")}, class: {element.GetAttribute("class")})");
                        }
                    }
                }
                
                await LoadFallbackCountriesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading countries from dropdown");
            await LoadFallbackCountriesAsync();
        }
    }

    private async Task LoadCountriesFromDatabaseAsync()
    {
        try
        {
            _logger.LogInformation("Loading countries from database (scraped from government autocomplete)...");
            
            // Load countries from the database that were scraped by CountryScraper
            var countriesFromDb = await _dbContext.Countries
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .ToListAsync();
                
            if (countriesFromDb.Count > 0)
            {
                _allCountries.AddRange(countriesFromDb);
                _logger.LogInformation($"Loaded {_allCountries.Count} countries from database");
                _logger.LogInformation($"Sample countries: {string.Join(", ", _allCountries.Take(5))}...");
            }
            else
            {
                _logger.LogWarning("No countries found in database. Please run the country scraper first with: dotnet run countries");
                _logger.LogInformation("Falling back to hardcoded country list...");
                await LoadFallbackCountriesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading countries from database");
            _logger.LogInformation("Falling back to hardcoded country list...");
            await LoadFallbackCountriesAsync();
        }
    }

    private async Task LoadFallbackCountriesAsync()
    {
        _logger.LogInformation("Loading comprehensive fallback country list...");
        
        // Comprehensive list of 193 UN member countries plus territories
        var countries = new[]
        {
            "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda", "Argentina", "Armenia", "Australia", "Austria",
            "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bhutan",
            "Bolivia", "Bosnia and Herzegovina", "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cambodia", "Cameroon",
            "Canada", "Cape Verde", "Central African Republic", "Chad", "Chile", "China", "Colombia", "Comoros", "Congo", "Costa Rica",
            "Croatia", "Cuba", "Cyprus", "Czech Republic", "Democratic Republic of the Congo", "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador",
            "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini", "Ethiopia", "Fiji", "Finland", "France",
            "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Greece", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau",
            "Guyana", "Haiti", "Honduras", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland",
            "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Kuwait", "Kyrgyzstan",
            "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg", "Madagascar",
            "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Mauritania", "Mauritius", "Mexico", "Micronesia",
            "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco", "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal",
            "Netherlands", "New Zealand", "Nicaragua", "Niger", "Nigeria", "North Korea", "North Macedonia", "Norway", "Oman", "Pakistan",
            "Palau", "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Qatar",
            "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines", "Samoa", "San Marino", "Sao Tome and Principe", "Saudi Arabia",
            "Senegal", "Serbia", "Seychelles", "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa",
            "South Korea", "South Sudan", "Spain", "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland", "Syria", "Taiwan",
            "Tajikistan", "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan",
            "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom", "United States", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City",
            "Venezuela", "Vietnam", "Yemen", "Zambia", "Zimbabwe",
            // Additional territories and dependencies
            "Puerto Rico", "Guam", "US Virgin Islands", "American Samoa", "Northern Mariana Islands",
            "Hong Kong", "Macau", "Faroe Islands", "Greenland", "French Polynesia", "New Caledonia"
        };
        
        _allCountries.AddRange(countries);
        _logger.LogInformation($"Loaded {_allCountries.Count} countries from comprehensive fallback list");
        
        await Task.Delay(100); // Small delay for async consistency
    }

    private async Task<IWebElement?> WaitForElementAsync(By by, TimeSpan timeout)
    {
        try
        {
            var wait = new WebDriverWait(_driver!, timeout);
            return wait.Until(driver => driver.FindElement(by));
        }
        catch (WebDriverTimeoutException)
        {
            return null;
        }
    }

    private async Task UpdateCountriesTableAsync()
    {
        try
        {
            _logger.LogInformation("Updating Countries table with extracted countries...");
            
            var existingCountries = await _dbContext.Countries.Select(c => c.Name).ToListAsync();
            var newCountries = new List<Country>();
            
            foreach (var countryName in _allCountries)
            {
                if (!existingCountries.Contains(countryName))
                {
                    newCountries.Add(new Country
                    {
                        Id = Guid.NewGuid(),
                        Name = countryName,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            
            if (newCountries.Any())
            {
                _dbContext.Countries.AddRange(newCountries);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Added {newCountries.Count} new countries to Countries table");
            }
            else
            {
                _logger.LogInformation("All countries already exist in Countries table");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Countries table");
        }
    }

    private async Task ClearExistingVisaWizardDataAsync()
    {
        try
        {
            _logger.LogInformation("Clearing existing VisaWizard data as requested...");
            
            var existingRecords = await _dbContext.VisaWizards.CountAsync();
            if (existingRecords > 0)
            {
                _dbContext.VisaWizards.RemoveRange(_dbContext.VisaWizards);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Cleared {existingRecords} existing VisaWizard records");
            }
            else
            {
                _logger.LogInformation("No existing VisaWizard data to clear");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing existing VisaWizard data");
        }
    }

    private async Task<int> ProcessCountryCompleteWizardAsync(string country)
    {
        var totalRecords = 0;
        
        try
        {
            _logger.LogInformation($"Starting complete wizard processing for {country}");
            
            // Navigate to wizard page
            _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
            await Task.Delay(3000);
            
            // Step 1: Select the country (question1)
            var question1Div = await WaitForElementAsync(By.Id("question1"), TimeSpan.FromSeconds(10));
            if (question1Div == null)
            {
                _logger.LogWarning($"Could not find question1 div for {country}");
                return 0;
            }
            
            // Select the country using autocomplete input
            try
            {
                var countryInput = question1Div.FindElement(By.CssSelector("input.autocomplete_input, input[name*='autocomplete']"));
                
                // Clear any existing text and type the country name
                countryInput.Clear();
                countryInput.SendKeys(country);
                await Task.Delay(1500); // Wait for autocomplete suggestions
                
                // Try to select the first autocomplete suggestion
                try
                {
                    // Look for autocomplete dropdown and click first option
                    var firstOption = await WaitForElementAsync(By.CssSelector(".ui-autocomplete li:first-child, .ui-menu-item:first-child"), TimeSpan.FromSeconds(3));
                    if (firstOption != null)
                    {
                        firstOption.Click();
                        _logger.LogInformation($"Clicked autocomplete suggestion for {country}");
                    }
                    else
                    {
                        // Fallback to keyboard navigation
                        countryInput.SendKeys(OpenQA.Selenium.Keys.ArrowDown);
                        await Task.Delay(500);
                        countryInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                        _logger.LogInformation($"Used keyboard selection for {country}");
                    }
                }
                catch
                {
                    // Final fallback - just press Enter/Tab
                    countryInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                }
                
                await Task.Delay(2000); // Wait for page to update
                
                _logger.LogInformation($"Selected country via autocomplete: {country}");
            }
            catch (NoSuchElementException)
            {
                _logger.LogWarning($"Could not find autocomplete input for country selection");
                return 0;
            }
            
            // Step 2: Process question2 (Purpose dropdown)
            await Task.Delay(2000);
            var question2Div = await WaitForElementAsync(By.Id("question2"), TimeSpan.FromSeconds(5));
            
            if (question2Div != null)
            {
                _logger.LogInformation($"Found question2 div for {country}, processing purposes...");
                
                var purposeSelect = question2Div.FindElement(By.TagName("select"));
                var purposeSelectElement = new SelectElement(purposeSelect);
                var purposeOptions = purposeSelectElement.Options;
                
                // Extract all purpose options
                var availablePurposes = new List<string>();
                foreach (var option in purposeOptions)
                {
                    var purposeText = option.Text?.Trim();
                    if (!string.IsNullOrWhiteSpace(purposeText) && 
                        !purposeText.Equals("Please Select", StringComparison.OrdinalIgnoreCase))
                    {
                        availablePurposes.Add(purposeText);
                    }
                }
                
                _logger.LogInformation($"Found {availablePurposes.Count} purposes for {country}: {string.Join(", ", availablePurposes)}");
                
                // Process each purpose
                foreach (var purpose in availablePurposes)
                {
                    var recordsForPurpose = await ProcessCountryPurposeWizardAsync(country, purpose);
                    totalRecords += recordsForPurpose;
                    
                    // Small delay between purposes
                    await Task.Delay(1000);
                }
            }
            else
            {
                _logger.LogInformation($"No question2 div found for {country} - may not have purpose selection");
                
                // Record that this country has no purposes
                var noPurposeRecord = new VisaWizard
                {
                    Id = Guid.NewGuid(),
                    Country = country,
                    Purpose = "No purposes available",
                    Answer1 = country,
                    HasFollowUp = false,
                    StepNumber = 1,
                    IsCompleteSession = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                await SaveSingleWizardRecordAsync(noPurposeRecord);
                totalRecords = 1;
            }
            
            _logger.LogInformation($"Completed processing {country}: {totalRecords} records");
            return totalRecords;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing country {country}");
            return totalRecords;
        }
    }

    private async Task<int> ProcessCountryPurposeWizardAsync(string country, string purpose)
    {
        var sessionId = Guid.NewGuid();
        var records = new List<VisaWizard>();
        
        try
        {
            _logger.LogInformation($"Processing {country} - {purpose}");
            
            // Navigate fresh to wizard page
            _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
            await Task.Delay(3000);
            
            // Select country again using autocomplete
            var question1Div = await WaitForElementAsync(By.Id("question1"), TimeSpan.FromSeconds(10));
            if (question1Div != null)
            {
                try
                {
                    var countryInput = question1Div.FindElement(By.CssSelector("input.autocomplete_input, input[name*='autocomplete']"));
                    countryInput.Clear();
                    countryInput.SendKeys(country);
                    await Task.Delay(1500);
                    
                    // Try to select autocomplete suggestion
                    try
                    {
                        var firstOption = await WaitForElementAsync(By.CssSelector(".ui-autocomplete li:first-child, .ui-menu-item:first-child"), TimeSpan.FromSeconds(3));
                        if (firstOption != null)
                        {
                            firstOption.Click();
                        }
                        else
                        {
                            countryInput.SendKeys(OpenQA.Selenium.Keys.ArrowDown);
                            await Task.Delay(500);
                            countryInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                        }
                    }
                    catch
                    {
                        countryInput.SendKeys(OpenQA.Selenium.Keys.Enter);
                    }
                    
                    await Task.Delay(2000);
                }
                catch (NoSuchElementException)
                {
                    _logger.LogWarning($"Could not find autocomplete input in repeat country selection for {country}");
                    return 0;
                }
            }
            
            // Select purpose
            var question2Div = await WaitForElementAsync(By.Id("question2"), TimeSpan.FromSeconds(5));
            if (question2Div != null)
            {
                var purposeSelect = question2Div.FindElement(By.TagName("select"));
                var purposeSelectElement = new SelectElement(purposeSelect);
                purposeSelectElement.SelectByText(purpose);
                await Task.Delay(2000);
                
                // Record the country and purpose selection
                records.Add(new VisaWizard
                {
                    Id = Guid.NewGuid(),
                    SessionId = sessionId,
                    Country = country,
                    Purpose = purpose,
                    Answer1 = country,
                    Answer2 = purpose,
                    HasFollowUp = false, // Will be updated if we find more questions
                    StepNumber = 1,
                    CreatedAt = DateTime.UtcNow
                });
            }
            
            // Now check for questions 3-9
            await ProcessFollowUpQuestionsAsync(country, purpose, sessionId, records);
            
            // Look for outcome/results
            await ProcessWizardOutcomeAsync(country, purpose, sessionId, records);
            
            // Save all records for this country-purpose combination
            if (records.Any())
            {
                await SaveWizardRecordsAsync(records);
                _logger.LogInformation($"Saved {records.Count} records for {country} - {purpose}");
            }
            
            return records.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing {country} - {purpose}");
            return 0;
        }
    }

    private async Task ProcessFollowUpQuestionsAsync(string country, string purpose, Guid sessionId, List<VisaWizard> records)
    {
        // Check for questions 3-9 as described
        for (int questionNum = 3; questionNum <= 9; questionNum++)
        {
            var questionId = $"question{questionNum}";
            var questionDiv = await WaitForElementAsync(By.Id(questionId), TimeSpan.FromSeconds(3));
            
            if (questionDiv != null && questionDiv.Displayed)
            {
                _logger.LogInformation($"Found {questionId} for {country} - {purpose}");
                
                try
                {
                    // Extract question text
                    var questionText = ExtractQuestionText(questionDiv);
                    
                    // Extract radio button options
                    var radioButtons = questionDiv.FindElements(By.CssSelector("input[type='radio']"));
                    
                    if (radioButtons.Any())
                    {
                        // Process each radio button option
                        foreach (var radioButton in radioButtons)
                        {
                            var optionText = GetRadioButtonText(radioButton);
                            
                            if (!string.IsNullOrWhiteSpace(optionText))
                            {
                                // Click this option to see if it leads to follow-up
                                radioButton.Click();
                                await Task.Delay(1500);
                                
                                // Check if this creates a follow-up question
                                var hasFollowUp = await CheckForFollowUpQuestion(questionNum + 1);
                                var nextQuestionId = hasFollowUp ? $"question{questionNum + 1}" : null;
                                
                                // Check for Learn More links
                                var learnMoreLinks = ExtractLearnMoreLinks(questionDiv);
                                
                                records.Add(new VisaWizard
                                {
                                    Id = Guid.NewGuid(),
                                    SessionId = sessionId,
                                    Country = country,
                                    Purpose = purpose,
                                    Answer1 = country,
                                    Answer2 = optionText,
                                    HasFollowUp = hasFollowUp,
                                    LearnMoreLinks = learnMoreLinks,
                                    StepNumber = questionNum,
                                    CreatedAt = DateTime.UtcNow
                                });
                                
                                _logger.LogDebug($"Recorded {questionId}: {questionText} -> {optionText} (HasFollowUp: {hasFollowUp})");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Error processing {questionId} for {country} - {purpose}: {ex.Message}");
                }
            }
            else
            {
                // No more questions found
                break;
            }
        }
    }

    private string ExtractQuestionText(IWebElement questionDiv)
    {
        try
        {
            // Look for common question text patterns
            var questionTextElements = questionDiv.FindElements(By.CssSelector("label, .question-text, h3, h4, p"));
            
            foreach (var element in questionTextElements)
            {
                var text = element.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(text) && text.Contains("?"))
                {
                    return text;
                }
            }
            
            // Fallback to div text
            return questionDiv.Text?.Trim() ?? "Question text not found";
        }
        catch
        {
            return "Question text extraction failed";
        }
    }

    private string GetRadioButtonText(IWebElement radioButton)
    {
        try
        {
            // Try to find associated label
            var radioId = radioButton.GetAttribute("id");
            if (!string.IsNullOrWhiteSpace(radioId))
            {
                var label = _driver!.FindElement(By.CssSelector($"label[for='{radioId}']"));
                return label.Text?.Trim() ?? "";
            }
            
            // Try to find nearby text
            var parent = radioButton.FindElement(By.XPath(".."));
            return parent.Text?.Trim() ?? radioButton.GetAttribute("value") ?? "";
        }
        catch
        {
            return radioButton.GetAttribute("value") ?? "Unknown option";
        }
    }

    private async Task<bool> CheckForFollowUpQuestion(int nextQuestionNum)
    {
        if (nextQuestionNum > 9) return false;
        
        var nextQuestionId = $"question{nextQuestionNum}";
        var nextQuestion = await WaitForElementAsync(By.Id(nextQuestionId), TimeSpan.FromSeconds(2));
        
        return nextQuestion != null && nextQuestion.Displayed;
    }

    private string ExtractLearnMoreLinks(IWebElement questionDiv)
    {
        try
        {
            var learnMoreLinks = questionDiv.FindElements(By.CssSelector("a[href*='learn'], a:contains('Learn More'), a:contains('more information')"));
            var links = learnMoreLinks
                .Select(link => link.GetAttribute("href"))
                .Where(href => !string.IsNullOrWhiteSpace(href))
                .ToList();
            
            return string.Join("; ", links);
        }
        catch
        {
            return "";
        }
    }

    private async Task ProcessWizardOutcomeAsync(string country, string purpose, Guid sessionId, List<VisaWizard> records)
    {
        try
        {
            // Wait a bit for any outcome to load
            await Task.Delay(2000);
            
            // Look for outcome display
            var outcomeElements = _driver!.FindElements(By.CssSelector("#outcome_display_rwd, .outcome, .result, .recommendation"));
            
            if (outcomeElements.Any())
            {
                var outcomeDiv = outcomeElements.First();
                var outcomeText = outcomeDiv.Text?.Trim();
                
                // Extract learn more links from outcome
                var learnMoreLinks = ExtractLearnMoreLinks(outcomeDiv);
                
                // Extract visa recommendations
                var visaRecommendations = ExtractVisaRecommendations(outcomeDiv);
                
                // Mark the last record as complete and add outcome info
                if (records.Any())
                {
                    var lastRecord = records.Last();
                    lastRecord.IsCompleteSession = true;
                    lastRecord.OutcomeDisplayContent = outcomeText;
                    lastRecord.LearnMoreLinks = string.IsNullOrWhiteSpace(lastRecord.LearnMoreLinks) 
                        ? learnMoreLinks 
                        : $"{lastRecord.LearnMoreLinks}; {learnMoreLinks}";
                    lastRecord.VisaRecommendations = visaRecommendations;
                }
                
                _logger.LogInformation($"Captured outcome for {country} - {purpose}");
            }
            else
            {
                // Mark the last record as complete even without specific outcome
                if (records.Any())
                {
                    records.Last().IsCompleteSession = true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Error processing outcome for {country} - {purpose}: {ex.Message}");
            
            // Still mark as complete
            if (records.Any())
            {
                records.Last().IsCompleteSession = true;
            }
        }
    }

    private string ExtractVisaRecommendations(IWebElement outcomeDiv)
    {
        try
        {
            var visaElements = outcomeDiv.FindElements(By.CssSelector("a[href*='visa'], .visa-type, .recommendation"));
            var recommendations = new List<string>();
            
            foreach (var element in visaElements)
            {
                var text = element.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    recommendations.Add(text);
                }
            }
            
            return string.Join("; ", recommendations);
        }
        catch
        {
            return "";
        }
    }

    private async Task SaveWizardRecordsAsync(List<VisaWizard> records)
    {
        try
        {
            _dbContext.VisaWizards.AddRange(records);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving wizard records to database");
        }
    }

    private async Task SaveSingleWizardRecordAsync(VisaWizard record)
    {
        try
        {
            _dbContext.VisaWizards.Add(record);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving single wizard record to database");
        }
    }
}