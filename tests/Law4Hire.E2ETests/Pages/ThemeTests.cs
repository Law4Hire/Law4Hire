using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Law4Hire.E2ETests.Pages;

[TestFixture]
public class ThemeTests : PageTest
{
    private string BaseUrl => TestConfiguration.BaseUrl;

    [SetUp]
    public async Task SetUp()
    {
        await Page.GotoAsync(BaseUrl);
        // Wait for the page to fully load
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    [Test]
    public async Task ThemeSelector_ShouldBeVisibleForAllUsers()
    {
        // Theme selector should be visible even when not logged in
        var themeSelector = Page.Locator("fluent-select").Filter(new() { HasText = "Light" });
        await Expect(themeSelector).ToBeVisibleAsync();
    }

    [Test]
    public async Task LightTheme_ShouldApplyCorrectColors()
    {
        // Select Light theme
        await SelectTheme("Light");
        
        // Check header background color (should be light blue #3498db)
        var header = Page.Locator("fluent-header");
        var headerBgColor = await header.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        
        // Check body background color (should be white #ffffff)
        var bodyBgColor = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).backgroundColor");
        
        // Check body text color (should be dark #333333)
        var bodyTextColor = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).color");
        
        // Verify colors - converting hex to rgb for comparison
        Assert.That(headerBgColor, Does.Contain("52, 152, 219").Or.Contain("rgb(52, 152, 219)"), 
            $"Header background should be light blue, but was: {headerBgColor}");
        Assert.That(bodyBgColor, Does.Contain("255, 255, 255").Or.Contain("rgb(255, 255, 255)"), 
            $"Body background should be white, but was: {bodyBgColor}");
        Assert.That(bodyTextColor, Does.Contain("51, 51, 51").Or.Contain("rgb(51, 51, 51)"), 
            $"Body text should be dark, but was: {bodyTextColor}");
    }

    [Test] 
    public async Task AutumnTheme_ShouldApplyCorrectColors()
    {
        // Select Autumn theme
        await SelectTheme("Autumn");
        
        // Check header background color (should be burnt orange #cd853f)
        var header = Page.Locator("fluent-header");
        var headerBgColor = await header.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        
        // Check body background color (should be light yellow #fefae0)
        var bodyBgColor = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).backgroundColor");
        
        // Check body text color (should be brown #5d4e37)
        var bodyTextColor = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).color");
        
        // Verify colors
        Assert.That(headerBgColor, Does.Contain("205, 133, 63").Or.Contain("rgb(205, 133, 63)"), 
            $"Header background should be burnt orange, but was: {headerBgColor}");
        Assert.That(bodyBgColor, Does.Contain("254, 250, 224").Or.Contain("rgb(254, 250, 224)"), 
            $"Body background should be light yellow, but was: {bodyBgColor}");
        Assert.That(bodyTextColor, Does.Contain("93, 78, 55").Or.Contain("rgb(93, 78, 55)"), 
            $"Body text should be brown, but was: {bodyTextColor}");
    }

    [Test]
    public async Task DarkTheme_ShouldApplyCorrectColors()
    {
        // Select Dark theme
        await SelectTheme("Dark");
        
        // Check header background color (should be very dark blue #1e3a8a)
        var header = Page.Locator("fluent-header");
        var headerBgColor = await header.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        
        // Check body background color (should be black #000000)
        var bodyBgColor = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).backgroundColor");
        
        // Check body text color (should be white #ffffff)
        var bodyTextColor = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).color");
        
        // Verify colors
        Assert.That(headerBgColor, Does.Contain("30, 58, 138").Or.Contain("rgb(30, 58, 138)"), 
            $"Header background should be very dark blue, but was: {headerBgColor}");
        Assert.That(bodyBgColor, Does.Contain("0, 0, 0").Or.Contain("rgb(0, 0, 0)"), 
            $"Body background should be black, but was: {bodyBgColor}");
        Assert.That(bodyTextColor, Does.Contain("255, 255, 255").Or.Contain("rgb(255, 255, 255)"), 
            $"Body text should be white, but was: {bodyTextColor}");
    }

    [Test]
    public async Task ThemeSelection_ShouldPersistAcrossPageReloads()
    {
        // Select Autumn theme
        await SelectTheme("Autumn");
        
        // Reload the page
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Check that Autumn theme is still selected
        var themeSelector = Page.Locator("fluent-select").Filter(new() { HasText = "Autumn" });
        await Expect(themeSelector).ToBeVisibleAsync();
        
        // Verify autumn colors are still applied
        var bodyBgColor = await Page.EvaluateAsync<string>("() => getComputedStyle(document.body).backgroundColor");
        Assert.That(bodyBgColor, Does.Contain("254, 250, 224").Or.Contain("rgb(254, 250, 224)"), 
            $"Body background should remain light yellow after reload, but was: {bodyBgColor}");
    }

    [Test]
    public async Task AllThemes_ShouldChangeMainContentAreaColors()
    {
        // Test that the main content area changes colors with each theme
        var contentArea = Page.Locator(".content");
        
        // Light theme
        await SelectTheme("Light");
        var lightBgColor = await contentArea.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        
        // Autumn theme
        await SelectTheme("Autumn");
        var autumnBgColor = await contentArea.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        
        // Dark theme
        await SelectTheme("Dark");
        var darkBgColor = await contentArea.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        
        // All three should be different
        Assert.That(lightBgColor, Is.Not.EqualTo(autumnBgColor), 
            "Light and Autumn themes should have different content background colors");
        Assert.That(autumnBgColor, Is.Not.EqualTo(darkBgColor), 
            "Autumn and Dark themes should have different content background colors");
        Assert.That(lightBgColor, Is.Not.EqualTo(darkBgColor), 
            "Light and Dark themes should have different content background colors");
    }

    private async Task SelectTheme(string themeName)
    {
        // Click on the theme selector dropdown
        var themeSelector = Page.Locator("fluent-select").Filter(new() { HasText = themeName }).Or(
            Page.Locator("fluent-select").Filter(new() { HasText = "Light" }).Or(
            Page.Locator("fluent-select").Filter(new() { HasText = "Autumn" }).Or(
            Page.Locator("fluent-select").Filter(new() { HasText = "Dark" }))));
            
        await themeSelector.ClickAsync();
        
        // Select the specific theme option
        var themeOption = Page.Locator($"fluent-option[value='{themeName}']");
        await themeOption.ClickAsync();
        
        // Wait for theme to be applied
        await Page.WaitForTimeoutAsync(500);
    }
}