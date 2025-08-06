using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Law4Hire.E2ETests;

/// <summary>
/// Base configuration for E2E tests with Selenium WebDriver
/// </summary>
public class TestConfiguration
{
    public static ChromeOptions GetChromeOptions(bool headless = true)
    {
        var options = new ChromeOptions();
        
        if (headless)
        {
            options.AddArguments("--headless");
        }
        
        options.AddArguments(
            "--no-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--disable-extensions",
            "--disable-plugins",
            "--disable-images",
            "--disable-javascript",
            "--window-size=1920,1080",
            "--remote-debugging-port=9222"
        );

        return options;
    }

    public static WebDriverWait CreateWait(IWebDriver driver, int timeoutSeconds = 30)
    {
        return new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
    }

    public static WebApplicationFactory<T> CreateTestServer<T>() where T : class
    {
        return new WebApplicationFactory<T>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Configure services for testing
                    services.AddLogging(logging =>
                    {
                        logging.ClearProviders();
                        logging.AddConsole();
                        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning);
                    });
                });
                builder.UseUrls("http://localhost:5555"); // Use a specific port for testing
            });
    }
}

/// <summary>
/// Test helper methods for common operations
/// </summary>
public static class TestHelpers
{
    public static void WaitForPageLoad(IWebDriver driver, int timeoutSeconds = 30)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
    }

    public static bool IsElementPresent(IWebDriver driver, By locator)
    {
        try
        {
            driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public static void TakeScreenshot(IWebDriver driver, string fileName)
    {
        try
        {
            var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
            var directory = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
            Directory.CreateDirectory(directory);
            var filePath = Path.Combine(directory, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            screenshot.SaveAsFile(filePath);
            Console.WriteLine($"Screenshot saved: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to take screenshot: {ex.Message}");
        }
    }

    public static void LogTestInfo(string message)
    {
        Console.WriteLine($"[TEST INFO] {DateTime.Now:HH:mm:ss} - {message}");
    }

    public static void LogTestError(string message, Exception? ex = null)
    {
        Console.WriteLine($"[TEST ERROR] {DateTime.Now:HH:mm:ss} - {message}");
        if (ex != null)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    public static void AssertNoJavaScriptErrors(IWebDriver driver)
    {
        try
        {
            var logs = driver.Manage().Logs.GetLog(LogType.Browser);
            var jsErrors = logs.Where(log => log.Level == OpenQA.Selenium.LogLevel.Severe).ToList();

            if (jsErrors.Any())
            {
                var errorMessages = string.Join("\n", jsErrors.Select(e => e.Message));
                throw new AssertionException($"JavaScript errors found:\n{errorMessages}");
            }
        }
        catch (Exception ex) when (!(ex is AssertionException))
        {
            // Some drivers might not support browser logs
            LogTestInfo($"Could not check JavaScript errors: {ex.Message}");
        }
    }

    public static void AssertPageLoadTime(Action action, int maxMilliseconds = 10000)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        action();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > maxMilliseconds)
        {
            throw new AssertionException($"Page load took {stopwatch.ElapsedMilliseconds}ms, expected less than {maxMilliseconds}ms");
        }

        LogTestInfo($"Page loaded in {stopwatch.ElapsedMilliseconds}ms");
    }
}