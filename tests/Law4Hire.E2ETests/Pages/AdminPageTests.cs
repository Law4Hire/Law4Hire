using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Law4Hire.Web;

namespace Law4Hire.E2ETests.Pages;

[TestFixture]
public class AdminPageTests
{
    private WebApplicationFactory<Law4Hire.Web.Program> _factory;
    private IWebDriver _driver;
    private WebDriverWait _wait;
    private string _baseUrl;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        // Configure Chrome options for testing
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("--headless"); // Run in headless mode for CI/CD
        chromeOptions.AddArguments("--no-sandbox");
        chromeOptions.AddArguments("--disable-dev-shm-usage");
        chromeOptions.AddArguments("--disable-gpu");
        chromeOptions.AddArguments("--window-size=1920,1080");

        _driver = new ChromeDriver(chromeOptions);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

        // Set up the test server
        _factory = new WebApplicationFactory<Law4Hire.Web.Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.UseUrls("http://localhost:5000"); // Fixed port for testing
            });

        _baseUrl = "http://localhost:5000";
        
        // Start the server
        _ = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _driver?.Quit();
        _driver?.Dispose();
        _factory?.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        // Navigate to home page before each test
        _driver.Navigate().GoToUrl(_baseUrl);
    }

    [Test]
    public void HomePage_LoadsSuccessfully()
    {
        // Act
        _driver.Navigate().GoToUrl(_baseUrl);

        // Assert
        Assert.That(_driver.Title, Does.Contain("Law4Hire"), "Page title should contain Law4Hire");
        
        // Verify key elements are present
        var navigationExists = _driver.FindElements(By.TagName("nav")).Count > 0;
        Assert.That(navigationExists, Is.True, "Navigation should be present");
    }

    [Test]
    public void Navigation_ContainsExpectedLinks()
    {
        // Act
        _driver.Navigate().GoToUrl(_baseUrl);

        // Assert - Check for common navigation links
        try
        {
            var homeLink = _wait.Until(driver => driver.FindElement(By.LinkText("Home")));
            Assert.That(homeLink, Is.Not.Null, "Home link should be present");
        }
        catch (WebDriverTimeoutException)
        {
            // If exact text doesn't exist, check for navigation structure
            var navElements = _driver.FindElements(By.CssSelector("nav a, .nav a, .navbar a"));
            Assert.That(navElements.Count, Is.GreaterThan(0), "Should have navigation links");
        }
    }

    [Test]
    public void AdminPage_RequiresAuthentication()
    {
        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/admin");

        // Assert
        // Should either redirect to login or show access denied
        var currentUrl = _driver.Url;
        var pageSource = _driver.PageSource;
        
        bool isRedirectedToLogin = currentUrl.Contains("login") || currentUrl.Contains("Account");
        bool showsAccessDenied = pageSource.Contains("Access Denied") || pageSource.Contains("permission");
        bool showsUnauthorized = pageSource.Contains("401") || pageSource.Contains("Unauthorized");

        Assert.That(isRedirectedToLogin || showsAccessDenied || showsUnauthorized, Is.True, 
            "Admin page should require authentication or show access denied");
    }

    [Test]
    public void ServicePackagesPage_LoadsWithoutErrors()
    {
        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/service-packages");

        // Assert
        // Check that page loads without 500 errors
        Assert.That(_driver.PageSource, Does.Not.Contain("500"), "Should not show server error");
        Assert.That(_driver.PageSource, Does.Not.Contain("An error occurred"), "Should not show generic error");
        
        // Check for expected content or redirect to appropriate page
        var hasServiceContent = _driver.PageSource.Contains("Service") || _driver.PageSource.Contains("Package");
        var isRedirected = !_driver.Url.Contains("service-packages");
        
        Assert.That(hasServiceContent || isRedirected, Is.True, "Should show service content or redirect appropriately");
    }

    [Test]
    public void PricingPage_LoadsWithoutErrors()
    {
        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/pricing");

        // Assert
        Assert.That(_driver.PageSource, Does.Not.Contain("500"), "Should not show server error");
        Assert.That(_driver.PageSource, Does.Not.Contain("An error occurred"), "Should not show generic error");
    }

    [Test]
    public void InterviewPage_LoadsWithoutErrors()
    {
        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/interview");

        // Assert
        Assert.That(_driver.PageSource, Does.Not.Contain("500"), "Should not show server error");
        Assert.That(_driver.PageSource, Does.Not.Contain("An error occurred"), "Should not show generic error");
        
        // Should either show interview content or redirect to appropriate page
        var hasInterviewContent = _driver.PageSource.Contains("interview") || _driver.PageSource.Contains("question");
        var isRedirected = !_driver.Url.Contains("interview");
        
        Assert.That(hasInterviewContent || isRedirected, Is.True, "Should show interview content or redirect appropriately");
    }

    [Test]
    public void Dashboard_RequiresAuthentication()
    {
        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/dashboard");

        // Assert
        var currentUrl = _driver.Url;
        var pageSource = _driver.PageSource;
        
        bool isRedirectedToLogin = currentUrl.Contains("login") || currentUrl.Contains("Account");
        bool showsAccessDenied = pageSource.Contains("Access Denied") || pageSource.Contains("permission");
        bool showsUnauthorized = pageSource.Contains("401") || pageSource.Contains("Unauthorized");

        Assert.That(isRedirectedToLogin || showsAccessDenied || showsUnauthorized, Is.True, 
            "Dashboard should require authentication");
    }

    [Test]
    public void Profile_RequiresAuthentication()
    {
        // Act
        _driver.Navigate().GoToUrl($"{_baseUrl}/profile");

        // Assert
        var currentUrl = _driver.Url;
        var pageSource = _driver.PageSource;
        
        bool isRedirectedToLogin = currentUrl.Contains("login") || currentUrl.Contains("Account");
        bool showsAccessDenied = pageSource.Contains("Access Denied") || pageSource.Contains("permission");
        bool showsUnauthorized = pageSource.Contains("401") || pageSource.Contains("Unauthorized");

        Assert.That(isRedirectedToLogin || showsAccessDenied || showsUnauthorized, Is.True, 
            "Profile should require authentication");
    }

    [Test]
    public void ApplicationStartup_DatabaseConnectionWorks()
    {
        // Act - Navigate to any page that would trigger database operations
        _driver.Navigate().GoToUrl(_baseUrl);

        // Assert - Check that the application doesn't show database connection errors
        var pageSource = _driver.PageSource;
        
        Assert.That(pageSource, Does.Not.Contain("database"), "Should not show database errors");
        Assert.That(pageSource, Does.Not.Contain("connection"), "Should not show connection errors");
        Assert.That(pageSource, Does.Not.Contain("SqlException"), "Should not show SQL exceptions");
        Assert.That(pageSource, Does.Not.Contain("timeout"), "Should not show timeout errors");
    }

    [Test]
    public void ApplicationStartup_NoJavaScriptErrors()
    {
        // Act
        _driver.Navigate().GoToUrl(_baseUrl);

        // Wait for page to fully load
        _wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

        // Assert - Check for JavaScript errors in console
        var logs = _driver.Manage().Logs.GetLog(LogType.Browser);
        var jsErrors = logs.Where(log => log.Level == LogLevel.Severe).ToList();

        Assert.That(jsErrors.Count, Is.EqualTo(0), 
            $"Should not have JavaScript errors. Found: {string.Join(", ", jsErrors.Select(e => e.Message))}");
    }

    [Test]
    public void ResponsiveDesign_WorksOnMobileViewport()
    {
        // Arrange - Set mobile viewport
        _driver.Manage().Window.Size = new System.Drawing.Size(375, 667); // iPhone size

        // Act
        _driver.Navigate().GoToUrl(_baseUrl);

        // Assert
        var viewport = (Dictionary<string, object>)((IJavaScriptExecutor)_driver)
            .ExecuteScript("return {width: window.innerWidth, height: window.innerHeight};");
        
        Assert.That((long)viewport["width"], Is.LessThanOrEqualTo(375), "Should respect mobile viewport width");
        
        // Check that page is still functional in mobile view
        Assert.That(_driver.PageSource, Does.Not.Contain("500"), "Should not show errors in mobile view");
    }

    [Test]
    public void LoadTesting_MultiplePageNavigation()
    {
        // Act - Navigate through multiple pages quickly
        var pages = new[] { "/", "/pricing", "/service-packages" };
        
        foreach (var page in pages)
        {
            _driver.Navigate().GoToUrl($"{_baseUrl}{page}");
            
            // Assert each page loads without errors
            Assert.That(_driver.PageSource, Does.Not.Contain("500"), $"Page {page} should not show server error");
            Assert.That(_driver.PageSource, Does.Not.Contain("An error occurred"), $"Page {page} should not show generic error");
        }
    }

    [Test]
    public void PerformanceTest_PageLoadTime()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        _driver.Navigate().GoToUrl(_baseUrl);
        
        // Wait for page to be fully loaded
        _wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        
        stopwatch.Stop();

        // Assert - Page should load within reasonable time (10 seconds for initial load)
        Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(10000), 
            $"Page should load within 10 seconds, took {stopwatch.ElapsedMilliseconds}ms");
    }
}