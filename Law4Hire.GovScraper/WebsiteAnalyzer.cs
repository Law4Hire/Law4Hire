using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text;

namespace Law4Hire.GovScraper;

public class WebsiteAnalyzer
{
    private readonly ILogger<WebsiteAnalyzer> _logger;
    private IWebDriver? _driver;

    public WebsiteAnalyzer(ILogger<WebsiteAnalyzer> logger)
    {
        _logger = logger;
    }

    public async Task AnalyzeVisaWizardStructureAsync()
    {
        try
        {
            InitializeDriver();
            
            _logger.LogInformation("🔍 ANALYZING VISA WIZARD WEBSITE STRUCTURE");
            _logger.LogInformation("🌐 Target: travel.state.gov visa wizard");
            _logger.LogInformation("📋 Goal: Extract all possible question options");
            _logger.LogInformation("=" + new string('=', 50));

            // Navigate to the page
            _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
            await Task.Delay(5000);

            // Analyze the page structure
            await AnalyzePageStructureAsync();
            
            // Try with a sample country to see all questions
            await AnalyzeWithSampleCountryAsync("United Kingdom");
            
        }
        finally
        {
            CleanupDriver();
        }
    }

    private async Task AnalyzePageStructureAsync()
    {
        _logger.LogInformation("📊 ANALYZING INITIAL PAGE STRUCTURE");
        
        // Check for all question divs
        for (int i = 1; i <= 9; i++)
        {
            var questionId = $"question{i}";
            var questionDiv = _driver!.FindElements(By.Id(questionId));
            
            if (questionDiv.Any())
            {
                _logger.LogInformation($"✅ Found {questionId} div");
                await AnalyzeQuestionDiv(i, questionDiv.First());
            }
            else
            {
                _logger.LogInformation($"❌ No {questionId} div found initially");
            }
        }
    }

    private async Task AnalyzeQuestionDiv(int questionNumber, IWebElement questionDiv)
    {
        try
        {
            _logger.LogInformation($"🔍 ANALYZING QUESTION {questionNumber}:");
            
            // Get the question text
            var questionText = ExtractQuestionText(questionDiv);
            _logger.LogInformation($"   📝 Question: {questionText}");
            
            // Check for select dropdown
            var selectElements = questionDiv.FindElements(By.TagName("select"));
            if (selectElements.Any())
            {
                _logger.LogInformation($"   📋 Found SELECT dropdown with options:");
                var selectElement = new SelectElement(selectElements.First());
                foreach (var option in selectElement.Options)
                {
                    var optionText = option.Text.Trim();
                    var optionValue = option.GetAttribute("value");
                    if (!string.IsNullOrEmpty(optionText) && !optionText.Equals("Please Select", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation($"     - \"{optionText}\" (value: {optionValue})");
                    }
                }
            }
            
            // Check for radio buttons
            var radioButtons = questionDiv.FindElements(By.CssSelector("input[type='radio']"));
            if (radioButtons.Any())
            {
                _logger.LogInformation($"   🔘 Found {radioButtons.Count} RADIO BUTTONS:");
                var radioGroups = radioButtons.GroupBy(r => r.GetAttribute("name"));
                
                foreach (var group in radioGroups)
                {
                    _logger.LogInformation($"     Group: {group.Key}");
                    foreach (var radio in group)
                    {
                        var radioId = radio.GetAttribute("id");
                        var radioValue = radio.GetAttribute("value");
                        string labelText = "Unknown";
                        
                        try
                        {
                            if (!string.IsNullOrEmpty(radioId))
                            {
                                var label = _driver!.FindElement(By.XPath($"//label[@for='{radioId}']"));
                                labelText = label.Text.Trim();
                            }
                        }
                        catch { }
                        
                        _logger.LogInformation($"       - \"{labelText}\" (value: {radioValue})");
                    }
                }
            }
            
            // Check for any other input types
            var otherInputs = questionDiv.FindElements(By.TagName("input"));
            var nonRadioInputs = otherInputs.Where(i => i.GetAttribute("type") != "radio").ToList();
            if (nonRadioInputs.Any())
            {
                _logger.LogInformation($"   📝 Found {nonRadioInputs.Count} OTHER INPUT ELEMENTS:");
                foreach (var input in nonRadioInputs)
                {
                    var inputType = input.GetAttribute("type");
                    var inputName = input.GetAttribute("name");
                    var inputId = input.GetAttribute("id");
                    _logger.LogInformation($"     - Type: {inputType}, Name: {inputName}, ID: {inputId}");
                }
            }
            
            _logger.LogInformation("");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"   ⚠️ Error analyzing question {questionNumber}: {ex.Message}");
        }
    }

    private async Task AnalyzeWithSampleCountryAsync(string country)
    {
        _logger.LogInformation($"🌍 ANALYZING WITH SAMPLE COUNTRY: {country}");
        
        try
        {
            // Fresh page load
            _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
            await Task.Delay(3000);
            
            // Select country
            await SelectCountryAsync(country);
            await Task.Delay(2000);
            
            // Now analyze what questions appeared
            _logger.LogInformation("📊 QUESTIONS AFTER COUNTRY SELECTION:");
            await AnalyzePageStructureAsync();
            
            // Try each purpose to see what questions they trigger
            await AnalyzePurposeOptionsAsync();
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error analyzing with sample country {country}");
        }
    }

    private async Task AnalyzePurposeOptionsAsync()
    {
        var purposes = new[]
        {
            "Tourism or Visit",
            "Business or Employment", 
            "Study or Exchange",
            "Traveling through the U.S. to another country",
            "Immigrate"
        };

        foreach (var purpose in purposes)
        {
            try
            {
                _logger.LogInformation($"🎯 TESTING PURPOSE: {purpose}");
                
                // Fresh start
                _driver!.Navigate().GoToUrl("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
                await Task.Delay(2000);
                
                await SelectCountryAsync("United Kingdom");
                await Task.Delay(1000);
                
                await SelectPurposeAsync(purpose);
                await Task.Delay(1000);
                
                // Click find visa to see what questions appear
                await ClickFindVisaAsync();
                await Task.Delay(3000);
                
                // Analyze questions that appeared
                _logger.LogInformation($"   Questions triggered by {purpose}:");
                for (int i = 3; i <= 9; i++)
                {
                    var questionDiv = _driver.FindElements(By.Id($"question{i}"));
                    if (questionDiv.Any() && questionDiv.First().Displayed)
                    {
                        _logger.LogInformation($"   ✅ Question {i} appeared");
                        await AnalyzeQuestionDiv(i, questionDiv.First());
                    }
                }
                
                _logger.LogInformation("");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"   ⚠️ Error testing purpose {purpose}: {ex.Message}");
            }
        }
    }

    private async Task<bool> SelectCountryAsync(string country)
    {
        try
        {
            var countryInput = _driver!.FindElement(By.CssSelector("input.autocomplete_input, input[type='text']"));
            countryInput.Clear();
            countryInput.SendKeys(country);
            await Task.Delay(1000);
            
            // Try to select from autocomplete
            var firstOption = _driver.FindElements(By.CssSelector(".ui-autocomplete li:first-child")).FirstOrDefault();
            if (firstOption != null)
            {
                firstOption.Click();
                return true;
            }
            
            countryInput.SendKeys(Keys.Enter);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Could not select country {country}: {ex.Message}");
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
            _logger.LogWarning($"Could not select purpose {purpose}: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ClickFindVisaAsync()
    {
        try
        {
            var findVisaButton = _driver!.FindElement(By.XPath("//a[contains(text(), 'Find a Visa')]"));
            findVisaButton.Click();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Could not click Find a Visa button: {ex.Message}");
            return false;
        }
    }

    private string ExtractQuestionText(IWebElement questionDiv)
    {
        try
        {
            var textElements = questionDiv.FindElements(By.CssSelector("h3, h4, label, .question-text, p"));
            foreach (var element in textElements)
            {
                var text = element.Text.Trim();
                if (!string.IsNullOrEmpty(text) && text.Length > 10)
                {
                    return text;
                }
            }
            return questionDiv.Text.Trim();
        }
        catch
        {
            return "Could not extract question text";
        }
    }

    private void InitializeDriver()
    {
        var options = new ChromeOptions();
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");
        
        // Add unique user data directory to avoid conflicts
        var tempUserDataDir = Path.Combine(Path.GetTempPath(), "ChromeUserDataAnalyzer_" + Guid.NewGuid().ToString());
        options.AddArgument($"--user-data-dir={tempUserDataDir}");
        
        _driver = new ChromeDriver(options);
        _logger.LogInformation("Chrome WebDriver initialized for website analysis");
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