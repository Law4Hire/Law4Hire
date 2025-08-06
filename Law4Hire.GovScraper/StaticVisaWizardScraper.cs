using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Law4Hire.GovScraper;

public class StaticVisaWizardScraper
{
    private readonly ILogger<StaticVisaWizardScraper> _logger;
    private readonly Law4HireDbContext _dbContext;
    private IWebDriver? _driver;

    // Static arrays of all possible options based on website analysis
    private readonly List<string> _purposes = new()
    {
        "Tourism or Visit",
        "Business or Employment", 
        "Study or Exchange",
        "Traveling through the U.S. to another country",
        "Immigrate"
    };

    // Business/Employment options (Question 3)
    private readonly Dictionary<string, List<string>> _businessEmploymentOptions = new()
    {
        ["doing_in_us_business_employment"] = new()
        {
            "travel_for_temporary_business",
            "travel_for_temporary_employment", 
            "travel_as_crew_member",
            "travel_for_govt",
            "travel_for_exchange",
            "travel_for_permanent_employment"
        },
        ["i_am_temporary_business"] = new()
        {
            "investment_or_trade"
        }
    };

    // Kind of work options (Question 4)
    private readonly List<string> _kindOfWorkOptions = new()
    {
        "travel_for_employer_home_country",
        "transfer_to_us_office",
        "work_for_us_emloyer",
        "accepting_offer_us_employer"
    };

    // Temporary business options (Question 5)
    private readonly List<string> _temporaryBusinessOptions = new()
    {
        "meeting_for_shortterm_business",
        "member_of_media_in_us"
    };

    // Employment subcategories (Question 6)
    private readonly List<string> _employmentSubcategories = new()
    {
        "profession_with_specialized_skills",
        "receiving_training",
        "seasonal_temporary_worker",
        "extraordinary_ability",
        "nonprofit_religious_organization"
    };

    // Study/Exchange options (Question 7)
    private readonly List<string> _studyExchangeOptions = new()
    {
        "courses_for_academic_credit",
        "participate_in_exchange_program",
        "no_credit_course",
        "prospective_student"
    };

    // Exchange program types (Question 8)
    private readonly List<string> _exchangeProgramTypes = new()
    {
        "study_as_student",
        "participant_summer_work_program",
        "conduct_research_as_scholar",
        "receive_training",
        "governemnt_international_visitor"
    };

    // Immigration situations (Question 9)
    private readonly List<string> _immigrationSituations = new()
    {
        "spouse_immediate_family_us_citizen",
        "immigrate_based_on_employment",
        "marry_us_citizen",
        "learn_about_dv_lottery"
    };

    public StaticVisaWizardScraper(ILogger<StaticVisaWizardScraper> logger, Law4HireDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task TestStaticApproachAsync()
    {
        try
        {
            await ClearExistingDataAsync();
            
            // Test with just first 3 countries
            var testCountries = new List<string> { "Afghanistan", "Albania", "Algeria" };

            _logger.LogInformation($"🧪 TESTING STATIC VISA WIZARD SCRAPING");
            _logger.LogInformation($"📊 Processing {testCountries.Count} test countries");
            _logger.LogInformation($"🎯 Using deterministic static approach - NO GUESSING");
            _logger.LogInformation("=" + new string('=', 60));

            InitializeDriver();

            var totalResults = 0;
            var countryCount = 0;

            foreach (var country in testCountries)
            {
                countryCount++;
                _logger.LogInformation($"🌍 [{countryCount}/{testCountries.Count}] Testing: {country}");
                
                var countryResults = await ProcessCountryStaticAsync(country);
                totalResults += countryResults;
                
                _logger.LogInformation($"✅ {country}: {countryResults} combinations captured");
            }

            _logger.LogInformation($"🎉 STATIC TEST COMPLETED!");
            _logger.LogInformation($"📊 Total results: {totalResults}");
            _logger.LogInformation($"✅ All {testCountries.Count} test countries processed systematically");
        }
        finally
        {
            CleanupDriver();
        }
    }

    public async Task ScrapeAllCountriesStaticAsync()
    {
        try
        {
            await ClearExistingDataAsync();
            
            // Get all countries from database
            var countries = await _dbContext.Countries
                .Where(c => c.IsActive)
                .OrderBy(c => c.SortOrder)
                .Select(c => c.Name)
                .ToListAsync();

            _logger.LogInformation($"🚀 STARTING STATIC VISA WIZARD SCRAPING");
            _logger.LogInformation($"📊 Processing {countries.Count} countries");
            _logger.LogInformation($"🎯 Using deterministic static approach - NO GUESSING");
            _logger.LogInformation("=" + new string('=', 60));

            InitializeDriver();

            var totalResults = 0;
            var countryCount = 0;

            foreach (var country in countries)
            {
                countryCount++;
                _logger.LogInformation($"🌍 [{countryCount}/{countries.Count}] Processing: {country}");
                
                var countryResults = await ProcessCountryStaticAsync(country);
                totalResults += countryResults;
                
                _logger.LogInformation($"✅ {country}: {countryResults} combinations captured");
                
                // Refresh browser every 3 countries for stability (more frequent)
                if (countryCount % 3 == 0)
                {
                    _logger.LogInformation("🔄 Refreshing browser for stability...");
                    CleanupDriver();
                    await Task.Delay(3000); // Longer delay for cleanup
                    InitializeDriver();
                    await Task.Delay(1000); // Wait for driver to be ready
                }
            }

            _logger.LogInformation($"🎉 STATIC SCRAPING COMPLETED!");
            _logger.LogInformation($"📊 Total results: {totalResults}");
            _logger.LogInformation($"✅ All {countries.Count} countries processed systematically");
        }
        finally
        {
            CleanupDriver();
        }
    }

    private async Task<int> ProcessCountryStaticAsync(string country)
    {
        var totalResults = 0;
        const int maxRetries = 2;

        foreach (var purpose in _purposes)
        {
            _logger.LogInformation($"  🎯 Purpose: {purpose}");
            
            var attempt = 0;
            var purposeResults = 0;
            
            while (attempt < maxRetries && purposeResults == 0)
            {
                attempt++;
                try
                {
                    if (attempt > 1)
                    {
                        _logger.LogInformation($"    🔄 Retry attempt {attempt} for {country} - {purpose}");
                        await Task.Delay(2000);
                    }
                    
                    purposeResults = await ProcessCountryPurposeStaticAsync(country, purpose);
                    totalResults += purposeResults;
                    _logger.LogInformation($"    ✅ {purposeResults} combinations for {purpose}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"    ❌ Attempt {attempt} failed for {country} - {purpose}: {ex.Message}");
                    
                    if (attempt == maxRetries)
                    {
                        _logger.LogError($"    💥 All {maxRetries} attempts failed for {country} - {purpose}");
                    }
                    else
                    {
                        // Wait before retry and refresh page
                        await Task.Delay(3000);
                        try
                        {
                            _driver?.Navigate().Refresh();
                            await Task.Delay(2000);
                        }
                        catch
                        {
                            // If refresh fails, continue to retry
                        }
                    }
                }
            }
        }

        return totalResults;
    }

    private async Task<int> ProcessCountryPurposeStaticAsync(string country, string purpose)
    {
        var results = new List<VisaWizard>();

        // Navigate and select country/purpose
        if (!await NavigateAndSelectBasicsAsync(country, purpose))
        {
            return 0;
        }

        // Process based on purpose
        switch (purpose)
        {
            case "Tourism or Visit":
                await ProcessTourismVisitAsync(country, purpose, results);
                break;
                
            case "Business or Employment":
                await ProcessBusinessEmploymentAsync(country, purpose, results);
                break;
                
            case "Study or Exchange":
                await ProcessStudyExchangeAsync(country, purpose, results);
                break;
                
            case "Traveling through the U.S. to another country":
                await ProcessTransitAsync(country, purpose, results);
                break;
                
            case "Immigrate":
                await ProcessImmigrationAsync(country, purpose, results);
                break;
        }

        // Save all results
        if (results.Any())
        {
            await SaveResultsAsync(results);
        }

        return results.Count;
    }

    private async Task ProcessTourismVisitAsync(string country, string purpose, List<VisaWizard> results)
    {
        // Tourism/Visit typically goes directly to results - no follow-up questions
        await ClickFindVisaAsync();
        await Task.Delay(2000);
        
        var links = await CaptureLearnMoreLinksAsync();
        var visaRecommendations = await CaptureVisaRecommendationsAsync();
        
        results.Add(new VisaWizard
        {
            Country = country,
            Purpose = purpose,
            Answer1 = null,  // No follow-up question, so no Answer1
            Answer2 = null,  // No follow-up question, so no Answer2
            HasFollowUp = false,
            LearnMoreLinks = links,
            VisaRecommendations = visaRecommendations,
            IsCompleteSession = true,
            StepNumber = 1,
            SessionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });
    }

    private async Task ProcessBusinessEmploymentAsync(string country, string purpose, List<VisaWizard> results)
    {
        await ClickFindVisaAsync();
        await Task.Delay(2000);

        // Process all business/employment subcategories
        foreach (var groupName in _businessEmploymentOptions.Keys)
        {
            foreach (var optionValue in _businessEmploymentOptions[groupName])
            {
                await ProcessBusinessSubcategoryAsync(country, purpose, groupName, optionValue, results);
            }
        }
    }

    private async Task ProcessBusinessSubcategoryAsync(string country, string purpose, string groupName, string optionValue, List<VisaWizard> results)
    {
        try
        {
            // Fresh navigation
            if (!await NavigateAndSelectBasicsAsync(country, purpose)) return;
            await ClickFindVisaAsync();
            await Task.Delay(1500);

            // Select the business subcategory
            if (await SelectRadioOptionAsync(optionValue))
            {
                // Check what happens next based on the option
                if (optionValue == "travel_for_temporary_employment")
                {
                    // This leads to kind_of_work question (Question 4)
                    await ProcessKindOfWorkOptionsAsync(country, purpose, optionValue, results);
                }
                else if (optionValue == "travel_for_temporary_business")
                {
                    // This leads to temporary business question (Question 5)
                    await ProcessTemporaryBusinessOptionsAsync(country, purpose, optionValue, results);
                }
                else
                {
                    // Other options likely go directly to results
                    await ClickFindVisaAsync();
                    await Task.Delay(2000);
                    
                    var links = await CaptureLearnMoreLinksAsync();
                    var visaRecommendations = await CaptureVisaRecommendationsAsync();
                    
                    results.Add(new VisaWizard
                    {
                        Country = country,
                        Purpose = purpose,
                        Answer1 = optionValue,  // The selected radio button value
                        Answer2 = null,         // No second follow-up
                        HasFollowUp = false,
                        LearnMoreLinks = links,
                        VisaRecommendations = visaRecommendations,
                        IsCompleteSession = true,
                        StepNumber = 2,
                        SessionId = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error processing business subcategory {optionValue}");
        }
    }

    private async Task ProcessKindOfWorkOptionsAsync(string country, string purpose, string parentOption, List<VisaWizard> results)
    {
        foreach (var workOption in _kindOfWorkOptions)
        {
            try
            {
                // Fresh navigation for each work option
                if (!await NavigateAndSelectBasicsAsync(country, purpose)) continue;
                await ClickFindVisaAsync();
                await Task.Delay(1000);
                
                if (!await SelectRadioOptionAsync(parentOption)) continue;
                await ClickFindVisaAsync();
                await Task.Delay(1000);
                
                if (await SelectRadioOptionAsync(workOption))
                {
                    await ClickFindVisaAsync();
                    await Task.Delay(2000);
                    
                    var links = await CaptureLearnMoreLinksAsync();
                    var visaRecommendations = await CaptureVisaRecommendationsAsync();
                    
                    results.Add(new VisaWizard
                    {
                        Country = country,
                        Purpose = purpose,
                        Answer1 = parentOption,  // First follow-up answer value
                        Answer2 = workOption,    // Second follow-up answer value
                        HasFollowUp = true,
                        LearnMoreLinks = links,
                        VisaRecommendations = visaRecommendations,
                        IsCompleteSession = true,
                        StepNumber = 3,
                        SessionId = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error processing work option {workOption}");
            }
        }
    }

    private async Task ProcessTemporaryBusinessOptionsAsync(string country, string purpose, string parentOption, List<VisaWizard> results)
    {
        foreach (var businessOption in _temporaryBusinessOptions)
        {
            try
            {
                // Fresh navigation for each business option
                if (!await NavigateAndSelectBasicsAsync(country, purpose)) continue;
                await ClickFindVisaAsync();
                await Task.Delay(1000);
                
                if (!await SelectRadioOptionAsync(parentOption)) continue;
                await ClickFindVisaAsync();
                await Task.Delay(1000);
                
                if (await SelectRadioOptionAsync(businessOption))
                {
                    await ClickFindVisaAsync();
                    await Task.Delay(2000);
                    
                    var links = await CaptureLearnMoreLinksAsync();
                    var visaRecommendations = await CaptureVisaRecommendationsAsync();
                    
                    results.Add(new VisaWizard
                    {
                        Country = country,
                        Purpose = purpose,
                        Answer1 = parentOption,     // First follow-up answer value
                        Answer2 = businessOption,   // Second follow-up answer value  
                        HasFollowUp = true,
                        LearnMoreLinks = links,
                        VisaRecommendations = visaRecommendations,
                        IsCompleteSession = true,
                        StepNumber = 3,
                        SessionId = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error processing business option {businessOption}");
            }
        }
    }

    private async Task ProcessStudyExchangeAsync(string country, string purpose, List<VisaWizard> results)
    {
        await ClickFindVisaAsync();
        await Task.Delay(2000);

        // Process all study/exchange options
        foreach (var studyOption in _studyExchangeOptions)
        {
            if (studyOption == "participate_in_exchange_program")
            {
                // This leads to exchange program types
                await ProcessExchangeProgramTypesAsync(country, purpose, studyOption, results);
            }
            else
            {
                await ProcessSingleStudyOptionAsync(country, purpose, studyOption, results);
            }
        }
    }

    private async Task ProcessSingleStudyOptionAsync(string country, string purpose, string studyOption, List<VisaWizard> results)
    {
        try
        {
            if (!await NavigateAndSelectBasicsAsync(country, purpose)) return;
            await ClickFindVisaAsync();
            await Task.Delay(1000);
            
            if (await SelectRadioOptionAsync(studyOption))
            {
                await ClickFindVisaAsync();
                await Task.Delay(2000);
                
                var links = await CaptureLearnMoreLinksAsync();
                var visaRecommendations = await CaptureVisaRecommendationsAsync();
                
                results.Add(new VisaWizard
                {
                    Country = country,
                    Purpose = purpose,
                    Answer1 = studyOption,  // The selected radio button value
                    Answer2 = null,         // No second follow-up
                    HasFollowUp = false,
                    LearnMoreLinks = links,
                    VisaRecommendations = visaRecommendations,
                    IsCompleteSession = true,
                    StepNumber = 2,
                    SessionId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error processing study option {studyOption}");
        }
    }

    private async Task ProcessExchangeProgramTypesAsync(string country, string purpose, string parentOption, List<VisaWizard> results)
    {
        foreach (var programType in _exchangeProgramTypes)
        {
            try
            {
                if (!await NavigateAndSelectBasicsAsync(country, purpose)) continue;
                await ClickFindVisaAsync();
                await Task.Delay(1000);
                
                if (!await SelectRadioOptionAsync(parentOption)) continue;
                await ClickFindVisaAsync();
                await Task.Delay(1000);
                
                if (await SelectRadioOptionAsync(programType))
                {
                    await ClickFindVisaAsync();
                    await Task.Delay(2000);
                    
                    var links = await CaptureLearnMoreLinksAsync();
                    var visaRecommendations = await CaptureVisaRecommendationsAsync();
                    
                    results.Add(new VisaWizard
                    {
                        Country = country,
                        Purpose = purpose,
                        Answer1 = parentOption,  // First follow-up answer value
                        Answer2 = programType,   // Second follow-up answer value
                        HasFollowUp = true,
                        LearnMoreLinks = links,
                        VisaRecommendations = visaRecommendations,
                        IsCompleteSession = true,
                        StepNumber = 3,
                        SessionId = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Error processing program type {programType}");
            }
        }
    }

    private async Task ProcessTransitAsync(string country, string purpose, List<VisaWizard> results)
    {
        // Transit typically goes directly to results - no follow-up questions
        await ClickFindVisaAsync();
        await Task.Delay(2000);
        
        var links = await CaptureLearnMoreLinksAsync();
        var visaRecommendations = await CaptureVisaRecommendationsAsync();
        
        results.Add(new VisaWizard
        {
            Country = country,
            Purpose = purpose,
            Answer1 = null,  // No follow-up question, so no Answer1
            Answer2 = null,  // No follow-up question, so no Answer2
            HasFollowUp = false,
            LearnMoreLinks = links,
            VisaRecommendations = visaRecommendations,
            IsCompleteSession = true,
            StepNumber = 1,
            SessionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        });
    }

    private async Task ProcessImmigrationAsync(string country, string purpose, List<VisaWizard> results)
    {
        await ClickFindVisaAsync();
        await Task.Delay(2000);

        // Process all immigration situations
        foreach (var situation in _immigrationSituations)
        {
            await ProcessImmigrationSituationAsync(country, purpose, situation, results);
        }
    }

    private async Task ProcessImmigrationSituationAsync(string country, string purpose, string situation, List<VisaWizard> results)
    {
        try
        {
            if (!await NavigateAndSelectBasicsAsync(country, purpose)) return;
            await ClickFindVisaAsync();
            await Task.Delay(1000);
            
            if (await SelectRadioOptionAsync(situation))
            {
                await ClickFindVisaAsync();
                await Task.Delay(2000);
                
                var links = await CaptureLearnMoreLinksAsync();
                var visaRecommendations = await CaptureVisaRecommendationsAsync();
                
                results.Add(new VisaWizard
                {
                    Country = country,
                    Purpose = purpose,
                    Answer1 = situation,  // The selected radio button value
                    Answer2 = null,       // No second follow-up
                    HasFollowUp = false,
                    LearnMoreLinks = links,
                    VisaRecommendations = visaRecommendations,
                    IsCompleteSession = true,
                    StepNumber = 2,
                    SessionId = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error processing immigration situation {situation}");
        }
    }


    private async Task<bool> NavigateAndSelectBasicsAsync(string country, string purpose)
    {
        try
        {
            // Navigate with timeout handling
            try
            {
                _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
                await Task.Delay(3000); // Longer wait for page load
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning($"Page load timeout for {country}, retrying...");
                await Task.Delay(2000);
                _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
                await Task.Delay(3000);
            }
            
            // Select country with retry
            var countryAttempts = 0;
            while (countryAttempts < 3)
            {
                countryAttempts++;
                if (await SelectCountryAsync(country)) break;
                
                if (countryAttempts < 3)
                {
                    _logger.LogWarning($"Country selection attempt {countryAttempts} failed for {country}, retrying...");
                    await Task.Delay(1500);
                }
                else
                {
                    _logger.LogError($"All country selection attempts failed for {country}");
                    return false;
                }
            }
            
            await Task.Delay(1500);
            
            // Select purpose with retry
            var purposeAttempts = 0;
            while (purposeAttempts < 3)
            {
                purposeAttempts++;
                if (await SelectPurposeAsync(purpose)) break;
                
                if (purposeAttempts < 3)
                {
                    _logger.LogWarning($"Purpose selection attempt {purposeAttempts} failed for {purpose}, retrying...");
                    await Task.Delay(1500);
                }
                else
                {
                    _logger.LogError($"All purpose selection attempts failed for {purpose}");
                    return false;
                }
            }
            
            await Task.Delay(1500);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error navigating and selecting basics for {country} - {purpose}");
            return false;
        }
    }

    private async Task<bool> SelectCountryAsync(string country)
    {
        try
        {
            // Find country input field
            var countryInput = _driver!.FindElement(By.CssSelector("input.autocomplete_input, input[type='text'][placeholder*='country' i], input[type='text'][id*='country' i]"));
            
            // Clear and type country name
            countryInput.Clear();
            countryInput.SendKeys(country);
            await Task.Delay(1000);
            
            _logger.LogInformation($"      🌍 Searching for country: {country}");
            
            // Look for autocomplete dropdown and select the country
            try
            {
                var autocompleteDropdown = _driver.FindElement(By.CssSelector(".ui-autocomplete.ui-menu.ui-widget.ui-widget-content.ui-corner-all"));
                
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
                // Fallback to keyboard navigation
                countryInput.SendKeys(Keys.Enter);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Could not select country {country}");
            return false;
        }
    }

    private async Task<bool> SelectPurposeAsync(string purpose)
    {
        try
        {
            var purposeSelect = _driver!.FindElement(By.Id("why_travel_to_us"));
            var selectElement = new SelectElement(purposeSelect);
            selectElement.SelectByText(purpose);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Could not select purpose {purpose}");
            return false;
        }
    }

    private async Task<bool> SelectRadioOptionAsync(string value)
    {
        try
        {
            // Wait for radio buttons to be available
            await Task.Delay(1000);
            
            // Find the radio button with enhanced error handling
            IWebElement? radioButton = null;
            
            // Try multiple strategies to find the radio button
            var selectors = new[]
            {
                By.CssSelector($"input[type='radio'][value='{value}']"),
                By.CssSelector($"input[value='{value}']"),
                By.XPath($"//input[@type='radio' and @value='{value}']"),
                By.XPath($"//input[@value='{value}']")
            };
            
            foreach (var selector in selectors)
            {
                try
                {
                    radioButton = _driver!.FindElement(selector);
                    if (radioButton.Displayed && radioButton.Enabled)
                    {
                        break;
                    }
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }
            
            if (radioButton == null)
            {
                _logger.LogWarning($"Could not find radio button with value: {value}");
                return false;
            }
            
            // Use JavaScript click as fallback for stubborn elements
            try
            {
                radioButton.Click();
            }
            catch (ElementNotInteractableException)
            {
                _logger.LogInformation($"Using JavaScript click for radio button: {value}");
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", radioButton);
            }
            
            await Task.Delay(750);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Could not select radio option {value}");
            return false;
        }
    }

    private async Task<bool> ClickFindVisaAsync()
    {
        try
        {
            // Handle any alerts first
            await HandleAlertsAsync();
            
            var findVisaButton = _driver!.FindElement(By.XPath("//a[contains(text(), 'Find a Visa')]"));
            findVisaButton.Click();
            await Task.Delay(500);
            
            await HandleAlertsAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Could not click Find a Visa button: {ex.Message}");
            return false;
        }
    }

    private async Task HandleAlertsAsync()
    {
        try
        {
            var alert = _driver!.SwitchTo().Alert();
            alert.Accept();
            await Task.Delay(500);
        }
        catch (NoAlertPresentException)
        {
            // No alert present - normal
        }
        catch (Exception ex)
        {
            _logger.LogDebug($"Alert handling error: {ex.Message}");
        }
    }

    private async Task<string> CaptureLearnMoreLinksAsync()
    {
        try
        {
            var visaOutcomeScreen = _driver!.FindElements(By.CssSelector(".visaoutcomescreen")).FirstOrDefault();
            if (visaOutcomeScreen == null) return "";
            
            var links = visaOutcomeScreen.FindElements(By.TagName("a"));
            var learnMoreLinks = links
                .Where(link => !string.IsNullOrEmpty(link.GetAttribute("href")) && 
                              (link.Text.Contains("Learn More", StringComparison.OrdinalIgnoreCase) || 
                               link.Text.Contains("More Information", StringComparison.OrdinalIgnoreCase)))
                .Select(link => link.GetAttribute("href"))
                .Distinct()
                .ToList();
            
            return string.Join(", ", learnMoreLinks);
        }
        catch
        {
            return "";
        }
    }

    private async Task<string> CaptureVisaRecommendationsAsync()
    {
        try
        {
            var visaOutcomeScreen = _driver!.FindElements(By.CssSelector(".visaoutcomescreen")).FirstOrDefault();
            if (visaOutcomeScreen == null) return "";
            
            var visaElements = visaOutcomeScreen.FindElements(By.CssSelector("a[href*='visa'], .visa-type, .recommendation"));
            var recommendations = visaElements
                .Select(element => element.Text?.Trim())
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .Distinct()
                .ToList();
            
            return string.Join("; ", recommendations);
        }
        catch
        {
            return "";
        }
    }

    private async Task SaveResultsAsync(List<VisaWizard> results)
    {
        try
        {
            _dbContext.VisaWizards.AddRange(results);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving results to database");
        }
    }

    private async Task ClearExistingDataAsync()
    {
        try
        {
            var existingCount = await _dbContext.VisaWizards.CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation($"🗑️ Clearing {existingCount} existing records...");
                _dbContext.VisaWizards.RemoveRange(_dbContext.VisaWizards);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("✅ Database cleared");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing existing data");
        }
    }

    private void InitializeDriver()
    {
        var options = new ChromeOptions();
        
        // Conservative headless mode settings
        options.AddArgument("--headless=new"); // Use new headless mode
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1280,720"); // Smaller window size
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-plugins");
        options.AddArgument("--disable-images");
        options.AddArgument("--disable-javascript-harmony");
        options.AddArgument("--disable-background-timer-throttling");
        options.AddArgument("--disable-backgrounding-occluded-windows");
        options.AddArgument("--disable-renderer-backgrounding");
        options.AddArgument("--disable-features=TranslateUI");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-sync");
        options.AddArgument("--metrics-recording-only");
        options.AddArgument("--no-first-run");
        options.AddArgument("--safebrowsing-disable-auto-update");
        options.AddArgument("--disable-ipc-flooding-protection");
        options.AddArgument("--disable-hang-monitor");
        options.AddArgument("--disable-client-side-phishing-detection");
        options.AddArgument("--disable-component-update");
        options.AddArgument("--disable-domain-reliability");
        options.AddArgument("--single-process"); // Try single process mode
        
        // Memory and performance settings
        options.AddArgument("--memory-pressure-off");
        options.AddArgument("--max_old_space_size=4096"); // Reduced memory
        
        // Reduce logging
        options.AddArgument("--log-level=3");
        options.AddArgument("--silent");
        
        // Add unique user data directory to avoid conflicts
        var tempUserDataDir = Path.Combine(Path.GetTempPath(), "ChromeUserDataStaticScraper_" + Guid.NewGuid().ToString());
        options.AddArgument($"--user-data-dir={tempUserDataDir}");
        
        try
        {
            _driver = new ChromeDriver(options);
            
            // Set more conservative timeouts
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(20);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            
            _logger.LogInformation("Chrome WebDriver initialized with ultra-stable configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Chrome WebDriver");
            throw;
        }
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
}