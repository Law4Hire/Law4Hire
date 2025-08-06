using Law4Hire.Core.Entities;
using Law4Hire.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace Law4Hire.GovScraper;

public class CountryScraper
{
    private readonly ILogger<CountryScraper> _logger;
    private readonly Law4HireDbContext _dbContext;
    private IBrowser? _browser;
    private IPage? _page;
    private readonly HashSet<string> _allCountries = new();
    
    public CountryScraper(ILogger<CountryScraper> logger, Law4HireDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<List<string>> ScrapeAllCountriesAsync()
    {
        try
        {
            await InitializeBrowserAsync();
            await ClearExistingCountriesAsync();
            await LoadCountriesFromAutocompleteAsync();
            await UpdateCountriesTableAsync();
            
            return _allCountries.OrderBy(c => c).ToList();
        }
        finally
        {
            await CleanupBrowserAsync();
        }
    }

    public async Task<List<string>> TestPopulateCountriesAsync()
    {
        try
        {
            _logger.LogInformation("Populating database with test countries (simulating scraper results)...");
            
            await ClearExistingCountriesAsync();
            
            // Add some sample countries that would typically be found through scraping
            var testCountries = new[]
            {
                "Afghanistan", "Albania", "Algeria", "Argentina", "Australia", "Austria",
                "Bangladesh", "Belgium", "Brazil", "Canada", "China", "Colombia",
                "Denmark", "Egypt", "France", "Germany", "India", "Italy",
                "Japan", "Mexico", "Netherlands", "Norway", "Poland", "Russia",
                "South Korea", "Spain", "Sweden", "Switzerland", "United Kingdom", "United States"
            };
            
            _allCountries.Clear();
            foreach (var country in testCountries)
            {
                _allCountries.Add(country);
            }
            
            await UpdateCountriesTableAsync();
            
            _logger.LogInformation($"Successfully populated database with {_allCountries.Count} test countries");
            return _allCountries.OrderBy(c => c).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during test country population");
            throw;
        }
    }

    private async Task InitializeBrowserAsync()
    {
        _logger.LogInformation("Initializing Puppeteer browser for country scraping...");
        
        // Download browser if needed
        await new BrowserFetcher().DownloadAsync();
        
        // Configure launch options
        var launchOptions = new LaunchOptions
        {
            Headless = true,
            Args = new[]
            {
                "--no-sandbox",
                "--disable-dev-shm-usage",
                "--disable-gpu",
                "--disable-web-security",
                "--disable-features=VizDisplayCompositor",
                "--window-size=1920,1080",
                "--disable-extensions",
                "--disable-plugins",
                "--disable-images",
                "--disable-background-timer-throttling",
                "--disable-backgrounding-occluded-windows",
                "--disable-renderer-backgrounding",
                "--disable-blink-features=AutomationControlled",
                "--no-first-run",
                "--no-default-browser-check",
                "--disable-default-apps",
                "--disable-sync",
                "--disable-features=VizDisplayCompositor,VizHitTestSurfaceLayer",
                "--disable-ipc-flooding-protection"
            }
        };
        
        _browser = await Puppeteer.LaunchAsync(launchOptions);
        _page = await _browser.NewPageAsync();
        
        // Set user agent
        await _page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        
        // Set viewport
        await _page.SetViewportAsync(new ViewPortOptions { Width = 1920, Height = 1080 });
        
        // Execute script to remove webdriver property
        await _page.EvaluateExpressionAsync("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");
        
        _logger.LogInformation("Puppeteer browser initialized successfully");
    }
    

    private async Task CleanupBrowserAsync()
    {
        try
        {
            if (_page != null)
            {
                await _page.CloseAsync();
                _page = null;
            }
            
            if (_browser != null)
            {
                await _browser.CloseAsync();
                _browser = null;
            }
            
            _logger.LogInformation("Puppeteer browser cleaned up successfully");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during Puppeteer browser cleanup");
        }
    }

    private async Task ClearExistingCountriesAsync()
    {
        try
        {
            _logger.LogInformation("Clearing existing country database entries...");
            
            var existingCountries = await _dbContext.Countries.CountAsync();
            if (existingCountries > 0)
            {
                _dbContext.Countries.RemoveRange(_dbContext.Countries);
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation($"Cleared {existingCountries} existing country records");
            }
            else
            {
                _logger.LogInformation("No existing country data to clear");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing existing country data");
            throw;
        }
    }

    private async Task LoadCountriesFromAutocompleteAsync()
    {
        try
        {
            _logger.LogInformation("Loading visa wizard page to extract countries from autocomplete...");
            await _page!.GoToAsync("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/wizard.html");
            
            // Wait for page to fully load
            await Task.Delay(5000);
            
            // Find the country input field
            var countryInputSelector = "input.autocomplete_input, input[type='text'][placeholder*='country' i], input[type='text'][id*='country' i]";
            
            try
            {
                await _page.WaitForSelectorAsync(countryInputSelector, new WaitForSelectorOptions { Timeout = 10000 });
            }
            catch
            {
                _logger.LogError("Could not find country input field on the page");
                throw new InvalidOperationException("Country input field not found");
            }

            _logger.LogInformation("Found country input field, starting A-Z autocomplete scraping...");

            // Iterate through A-Z to trigger autocomplete for each letter
            for (char letter = 'A'; letter <= 'Z'; letter++)
            {
                _logger.LogInformation($"Processing letter: {letter}");
                await ScrapeCountriesForLetterAsync(countryInputSelector, letter.ToString());
                await Task.Delay(1000); // Wait between requests to avoid being blocked
            }

            _logger.LogInformation($"Country scraping completed. Found {_allCountries.Count} unique countries");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading countries from autocomplete");
            throw;
        }
    }

    private async Task ScrapeCountriesForLetterAsync(string countryInputSelector, string letter)
    {
        try
        {
            // Clear the input field  
            await _page!.EvaluateFunctionAsync("(selector) => { const el = document.querySelector(selector); if(el) el.value = ''; }", countryInputSelector);
            await Task.Delay(500);
            
            // Type the letter to trigger autocomplete
            await _page.TypeAsync(countryInputSelector, letter);
            await Task.Delay(2000); // Wait for autocomplete to populate
            
            // Look for the autocomplete dropdown with the specific classes
            var autocompleteDropdownSelector = ".ui-autocomplete.ui-menu.ui-widget.ui-widget-content.ui-corner-all";
            
            try
            {
                await _page.WaitForSelectorAsync(autocompleteDropdownSelector, new WaitForSelectorOptions { Timeout = 5000 });
            }
            catch
            {
                _logger.LogWarning($"No autocomplete dropdown found for letter: {letter}");
                return;
            }

            // Find all menu items with the specific structure
            var menuItemsSelector = "li.ui-menu-item[role='menuitem']";
            var menuItems = await _page.QuerySelectorAllAsync($"{autocompleteDropdownSelector} {menuItemsSelector}");
            
            if (menuItems.Length == 0)
            {
                _logger.LogWarning($"No menu items found in autocomplete for letter: {letter}");
                return;
            }

            var foundCountries = new List<string>();
            
            foreach (var menuItem in menuItems)
            {
                try
                {
                    // Find the anchor element with class ui-corner-all
                    var anchor = await menuItem.QuerySelectorAsync("a.ui-corner-all");
                    if (anchor != null)
                    {
                        var textContentHandle = await anchor.GetPropertyAsync("textContent");
                        var countryName = (await textContentHandle.JsonValueAsync<string>()).Trim();
                        
                        // Filter out "No Results" entries and empty strings
                        if (!string.IsNullOrEmpty(countryName) && 
                            countryName != "No Results" && 
                            !_allCountries.Contains(countryName))
                        {
                            _allCountries.Add(countryName);
                            foundCountries.Add(countryName);
                        }
                    }
                }
                catch
                {
                    // Skip items that don't have the expected anchor structure
                    continue;
                }
            }
            
            _logger.LogInformation($"Letter {letter}: Found {foundCountries.Count} new countries: {string.Join(", ", foundCountries)}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Error scraping countries for letter: {letter}");
        }
    }


    private async Task UpdateCountriesTableAsync()
    {
        try
        {
            _logger.LogInformation($"Updating Countries table with {_allCountries.Count} countries...");
            
            var countriesToAdd = new List<Country>();
            var sortOrder = 1;
            var existingCodes2 = new HashSet<string>();
            var existingCodes3 = new HashSet<string>();
            
            foreach (var countryName in _allCountries.OrderBy(c => c))
            {
                // Generate unique country codes
                var countryCode2 = GenerateCountryCode(countryName, 2, existingCodes2);
                var countryCode3 = GenerateCountryCode(countryName, 3, existingCodes3);
                
                var country = new Country
                {
                    Id = Guid.NewGuid(),
                    Name = countryName,
                    CountryCode = countryCode3,
                    CountryCode2 = countryCode2,
                    SortOrder = sortOrder++,
                    IsUNRecognized = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                
                countriesToAdd.Add(country);
            }
            
            await _dbContext.Countries.AddRangeAsync(countriesToAdd);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation($"Successfully added {countriesToAdd.Count} countries to the database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating countries table");
            throw;
        }
    }

    private static string GenerateCountryCode(string countryName, int length, HashSet<string> existingCodes)
    {
        // Simple algorithm to generate country codes from names
        // For real-world use, you'd want to map to actual ISO codes
        var words = countryName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string baseCode;
        
        if (words.Length == 1)
        {
            baseCode = words[0].Length >= length 
                ? words[0].Substring(0, length).ToUpper()
                : words[0].ToUpper().PadRight(length, 'A');
        }
        else
        {
            // Take first letters of each word
            var code = string.Concat(words.Select(w => w[0])).ToUpper();
            
            if (code.Length >= length)
            {
                baseCode = code.Substring(0, length);
            }
            else
            {
                // If not enough letters, pad with characters from the first word
                var firstWord = words[0].ToUpper();
                for (int i = 1; code.Length < length && i < firstWord.Length; i++)
                {
                    code += firstWord[i];
                }
                baseCode = code.Length >= length ? code.Substring(0, length) : code.PadRight(length, 'A');
            }
        }
        
        // Ensure base code is exactly the right length
        if (baseCode.Length > length)
        {
            baseCode = baseCode.Substring(0, length);
        }
        else if (baseCode.Length < length)
        {
            baseCode = baseCode.PadRight(length, 'A');
        }
        
        // Make sure the code is unique using a more robust approach
        var finalCode = baseCode;
        int counter = 0;
        
        while (existingCodes.Contains(finalCode))
        {
            counter++;
            
            if (length == 2)
            {
                if (counter <= 9)
                {
                    // Try: first letter + digit (A1, B2, etc.)
                    finalCode = baseCode.Substring(0, 1) + counter.ToString();
                }
                else if (counter <= 35) // A-Z = 26 letters, so 10-35 maps to AA-AZ
                {
                    // Try: first letter + letter (AA, AB, AC...)
                    char secondChar = (char)('A' + (counter - 10));
                    finalCode = baseCode.Substring(0, 1) + secondChar;
                }
                else if (counter <= 61) // Next 26: BA-BZ
                {
                    char firstChar = (char)('A' + ((counter - 36) / 26));
                    char secondChar = (char)('A' + ((counter - 36) % 26));
                    finalCode = firstChar.ToString() + secondChar;
                }
                else
                {
                    // Fallback: use modulo approach with guaranteed uniqueness
                    var mod = counter % 100;
                    finalCode = ((char)('A' + (mod / 10))).ToString() + (mod % 10).ToString();
                }
            }
            else // length == 3
            {
                if (counter <= 9)
                {
                    // Try: first two chars + digit
                    finalCode = baseCode.Substring(0, Math.Min(2, baseCode.Length)) + counter.ToString();
                }
                else if (counter <= 99)
                {
                    // Try: first char + two digits
                    finalCode = baseCode.Substring(0, 1) + counter.ToString("00");
                }
                else if (counter <= 699) // A00-Z99 gives us 600 combinations
                {
                    char firstChar = (char)('A' + ((counter - 100) / 100));
                    int remaining = (counter - 100) % 100;
                    finalCode = firstChar + remaining.ToString("00");
                }
                else
                {
                    // Ultimate fallback: sequential letters + digit
                    var mod = counter % 1000;
                    char firstChar = (char)('A' + (mod / 100));
                    char secondChar = (char)('A' + ((mod / 10) % 10));
                    char thirdChar = (char)('0' + (mod % 10));
                    finalCode = firstChar.ToString() + secondChar + thirdChar;
                }
            }
            
            // Safety valve - prevent infinite loops
            if (counter > 10000)
            {
                // Generate completely unique code using timestamp
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
                finalCode = length == 2 
                    ? timestamp.Substring(timestamp.Length - 2)
                    : timestamp.Substring(timestamp.Length - 3);
                break;
            }
        }
        
        // Final safety check - ensure we never exceed length
        if (finalCode.Length > length)
        {
            finalCode = finalCode.Substring(0, length);
        }
        
        existingCodes.Add(finalCode);
        return finalCode;
    }
}