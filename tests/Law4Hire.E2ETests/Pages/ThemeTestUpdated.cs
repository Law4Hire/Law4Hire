using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using System.Text.Json;
using System.Drawing;

namespace Law4Hire.E2ETests.Pages;

[TestFixture]
public class ThemeTestUpdated : PageTest
{
    private string BaseUrl => "http://localhost:5161";

    [SetUp]
    public async Task SetUp()
    {
        // Set viewport for consistent testing
        await Page.SetViewportSizeAsync(1280, 720);
        await Page.GotoAsync(BaseUrl);
        // Wait for the page to fully load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task DiagnoseCurrentThemeState()
    {
        Console.WriteLine("=== THEME DIAGNOSIS TEST ===");
        
        // First, inspect the DOM structure
        await InspectDOMStructure();
        
        // Test light mode
        await TestLightMode();
        
        // Test dark mode  
        await TestDarkMode();
    }

    private async Task InspectDOMStructure()
    {
        Console.WriteLine("\n--- DOM Structure Analysis ---");
        
        // Check if FluentDesignTheme exists
        var fluentTheme = await Page.Locator("fluent-design-theme").CountAsync();
        Console.WriteLine($"FluentDesignTheme elements found: {fluentTheme}");
        
        if (fluentTheme > 0)
        {
            var mode = await Page.Locator("fluent-design-theme").GetAttributeAsync("mode");
            Console.WriteLine($"Current FluentDesignTheme mode: {mode}");
        }
        
        // Check for header element
        var headers = await Page.Locator("fluent-header").CountAsync();
        Console.WriteLine($"FluentHeader elements found: {headers}");
        
        // Check for main element
        var mains = await Page.Locator("fluent-main").CountAsync();
        Console.WriteLine($"FluentMain elements found: {mains}");
        
        // Check theme selector
        var themeSelector = await Page.Locator("fluent-select").Filter(new() { HasText = "Light" }).CountAsync();
        Console.WriteLine($"Theme selector found: {themeSelector > 0}");
    }

    private async Task TestLightMode()
    {
        Console.WriteLine("\n--- Testing Light Mode ---");
        
        // Select light mode
        await SelectTheme("Light");
        
        // Get colors
        var results = await GetThemeColors();
        
        Console.WriteLine("Light Mode Results:");
        Console.WriteLine($"  Header Background: {results.HeaderBg}");
        Console.WriteLine($"  Header Text: {results.HeaderText}");  
        Console.WriteLine($"  Body Background: {results.BodyBg}");
        Console.WriteLine($"  Body Text: {results.BodyText}");
        Console.WriteLine($"  Main Background: {results.MainBg}");
        
        // Expected: Header should be blue, body should be white
        Assert.That(results.BodyBg, Does.Contain("255, 255, 255").Or.Contain("rgb(255, 255, 255)"), 
            $"Light mode body should be white, but was: {results.BodyBg}");
    }

    private async Task TestDarkMode()
    {
        Console.WriteLine("\n--- Testing Dark Mode ---");
        
        // Select dark mode
        await SelectTheme("Dark");
        
        // Wait for theme to apply
        await Page.WaitForTimeoutAsync(1000);
        
        // Get colors
        var results = await GetThemeColors();
        
        Console.WriteLine("Dark Mode Results:");
        Console.WriteLine($"  Header Background: {results.HeaderBg}");
        Console.WriteLine($"  Header Text: {results.HeaderText}");
        Console.WriteLine($"  Body Background: {results.BodyBg}");
        Console.WriteLine($"  Body Text: {results.BodyText}");
        Console.WriteLine($"  Main Background: {results.MainBg}");
        
        // Check FluentDesignTheme mode attribute
        var themeMode = await Page.Locator("fluent-design-theme").GetAttributeAsync("mode");
        Console.WriteLine($"  FluentDesignTheme mode attribute: {themeMode}");
        
        // Expected: Should NOT be white background
        Assert.That(results.BodyBg, Does.Not.Contain("255, 255, 255"), 
            $"Dark mode body should NOT be white, but was: {results.BodyBg}");
            
        // Expected: Header should be dark
        Assert.That(results.HeaderBg, Does.Not.Contain("255, 255, 255"), 
            $"Dark mode header should NOT be white/light, but was: {results.HeaderBg}");
    }

    private async Task<ThemeColors> GetThemeColors()
    {
        var headerBg = "N/A";
        var headerText = "N/A";
        var bodyBg = "N/A";
        var bodyText = "N/A";
        var mainBg = "N/A";

        try
        {
            // Get header colors
            var headerExists = await Page.Locator("fluent-header").CountAsync() > 0;
            if (headerExists)
            {
                headerBg = await Page.Locator("fluent-header").EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
                headerText = await Page.Locator("fluent-header").EvaluateAsync<string>("el => getComputedStyle(el).color");
            }
            
            // Get body colors
            bodyBg = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).backgroundColor");
            bodyText = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).color");
            
            // Get main colors
            var mainExists = await Page.Locator("fluent-main").CountAsync() > 0;
            if (mainExists)
            {
                mainBg = await Page.Locator("fluent-main").EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting colors: {ex.Message}");
        }

        return new ThemeColors
        {
            HeaderBg = headerBg,
            HeaderText = headerText,
            BodyBg = bodyBg,
            BodyText = bodyText,
            MainBg = mainBg
        };
    }

    private async Task SelectTheme(string themeName)
    {
        try
        {
            // Look for theme selector dropdown
            var themeSelector = Page.Locator("fluent-select").Filter(new() { HasText = themeName }).Or(
                Page.Locator("fluent-select").Filter(new() { HasText = "Light" }).Or(
                Page.Locator("fluent-select").Filter(new() { HasText = "Dark" })));
            
            await themeSelector.ClickAsync();
            
            // Wait for dropdown to open
            await Page.WaitForTimeoutAsync(200);
            
            // Click the specific theme option
            var themeOption = Page.Locator($"fluent-option").Filter(new() { HasText = themeName });
            await themeOption.ClickAsync();
            
            Console.WriteLine($"Selected theme: {themeName}");
            
            // Wait for theme to be applied
            await Page.WaitForTimeoutAsync(500);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting theme {themeName}: {ex.Message}");
        }
    }

    private class ThemeColors
    {
        public string HeaderBg { get; set; } = "";
        public string HeaderText { get; set; } = "";
        public string BodyBg { get; set; } = "";
        public string BodyText { get; set; } = "";
        public string MainBg { get; set; } = "";
    }
}