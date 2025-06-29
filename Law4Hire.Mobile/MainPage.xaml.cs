using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Law4Hire.Mobile;

public partial class MainPage : ContentPage
{
    private readonly IStringLocalizer<MainPage> _localizer;
    private readonly ILogger<MainPage> _logger;
    private string _currentLanguage = "en-US";

    public MainPage(IStringLocalizer<MainPage> localizer, ILogger<MainPage> logger)
    {
        InitializeComponent();
        _localizer = localizer;
        _logger = logger; 
        UpdateUI();
        
        // Set default language selection
        LanguagePicker.SelectedIndex = 0;
        
        _logger.LogInformation("MainPage initialized");
    }

    private void UpdateUI()
    {
        try
        {
            WelcomeLabel.Text = _localizer["Welcome"];
            StartIntakeBtn.Text = $"🚀 {_localizer["StartIntake"]}";
            ViewPackagesBtn.Text = $"📋 {_localizer["ViewPackages"]}";
            MyAccountBtn.Text = $"👤 {_localizer["MyAccount"]}";
            DescriptionLabel.Text = _localizer["Description"];
            
            _logger.LogInformation("UI updated for language: {Language}", _currentLanguage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating UI");
        }
    }

    private async void OnStartIntakeClicked(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Start intake button clicked");
            
            // Disable button to prevent double-clicks
            StartIntakeBtn.IsEnabled = false;
            StartIntakeBtn.Text = "⏳ Loading...";
            
            // TODO: Navigate to intake page
            await Shell.Current.GoToAsync("//intake");
            
            _logger.LogInformation("Navigated to intake page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to intake page");
            await DisplayAlert("Error", "Unable to start intake process. Please try again.", "OK");
        }
        finally
        {
            StartIntakeBtn.IsEnabled = true;
            StartIntakeBtn.Text = $"🚀 {_localizer["StartIntake"]}";
        }
    }

    private async void OnViewPackagesClicked(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("View packages button clicked");
            
            ViewPackagesBtn.IsEnabled = false;
            ViewPackagesBtn.Text = "⏳ Loading...";
            
            // TODO: Navigate to packages page
            await Shell.Current.GoToAsync("//packages");
            
            _logger.LogInformation("Navigated to packages page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to packages page");
            await DisplayAlert("Error", "Unable to load packages. Please try again.", "OK");
        }
        finally
        {
            ViewPackagesBtn.IsEnabled = true;
            ViewPackagesBtn.Text = $"📋 {_localizer["ViewPackages"]}";
        }
    }

    private async void OnMyAccountClicked(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("My account button clicked");
            
            MyAccountBtn.IsEnabled = false;
            MyAccountBtn.Text = "⏳ Loading...";
            
            // TODO: Navigate to account page or login if not authenticated
            await Shell.Current.GoToAsync("//account");
            
            _logger.LogInformation("Navigated to account page");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to account page");
            await DisplayAlert("Error", "Unable to access account. Please try again.", "OK");
        }
        finally
        {
            MyAccountBtn.IsEnabled = true;
            MyAccountBtn.Text = $"👤 {_localizer["MyAccount"]}";
        }
    }

    private void OnLanguageChanged(object sender, EventArgs e)
    {
        try
        {
            var picker = (Picker)sender;
            var selectedLanguage = picker.SelectedItem?.ToString();
            
            _logger.LogInformation("Language changed to: {Language}", selectedLanguage);
            
            // Map display names to culture codes
            _currentLanguage = selectedLanguage switch
            {
                "🇪🇸 Español" => "es-ES",
                "🇫🇷 Français" => "fr-FR", 
                "🇩🇪 Deutsch" => "de-DE",
                "🇨🇳 中文" => "zh-CN",
                "🇧🇷 Português" => "pt-BR",
                "🇮🇹 Italiano" => "it-IT",
                "🇷🇺 Русский" => "ru-RU",
                _ => "en-US"
            };

            // TODO: Implement actual culture switching
            // This would typically involve:
            // 1. Setting the current culture
            // 2. Reloading localized resources
            // 3. Updating the UI
            
            // For now, just update the UI with current localizations
            UpdateUI();
            
            // Show confirmation
            var message = _currentLanguage switch
            {
                "es-ES" => "Idioma cambiado a Español",
                "fr-FR" => "Langue changée en Français",
                "de-DE" => "Sprache auf Deutsch geändert",
                "zh-CN" => "语言已更改为中文",
                "pt-BR" => "Idioma alterado para Português",
                "it-IT" => "Lingua cambiata in Italiano",
                "ru-RU" => "Язык изменен на Русский",
                _ => "Language changed to English"
            };
            
            DisplayAlert("Language Updated", message, "OK");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing language");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _logger.LogInformation("MainPage appeared");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _logger.LogInformation("MainPage disappeared");
    }
}
