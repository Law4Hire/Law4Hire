using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Globalization;

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

        // Load saved language preference
        LoadSavedLanguage();

        // Set default language selection in picker
        SetPickerFromCurrentLanguage();

        // Update UI with current language
        UpdateUI();

        _logger.LogInformation("MainPage initialized with language: {Language}", _currentLanguage);
    }

    private void LoadSavedLanguage()
    {
        // Load language from MAUI Preferences
        _currentLanguage = Preferences.Default.Get("app_language", "en-US");

        // Set the culture
        SetCulture(_currentLanguage);
    }

    private void SetCulture(string cultureCode)
    {
        try
        {
            var culture = new CultureInfo(cultureCode);

            // Set current thread culture
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            // Set default cultures for new threads
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            _currentLanguage = cultureCode;

            // Save to preferences
            Preferences.Default.Set("app_language", cultureCode);

            _logger.LogInformation("Culture set to: {Culture}", cultureCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting culture to {Culture}", cultureCode);
        }
    }

    private void SetPickerFromCurrentLanguage()
    {
        var displayName = GetDisplayNameFromCultureCode(_currentLanguage);

        // Find the index of the current language in the picker
        var items = LanguagePicker.ItemsSource as string[];
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == displayName)
                {
                    LanguagePicker.SelectedIndex = i;
                    break;
                }
            }
        }
    }

    private string GetDisplayNameFromCultureCode(string cultureCode)
    {
        return cultureCode switch
        {
            "es-ES" => "🇪🇸 Español",
            "zh-CN" => "🇨🇳 中文 (简体)",
            "hi-IN" => "🇮🇳 हिन्दी",
            "ar-SA" => "🇸🇦 العربية",
            "bn-BD" => "🇧🇩 বাংলা",
            "pt-PT" => "🇵🇹 Português",
            "ru-RU" => "🇷🇺 Русский",
            "ja-JP" => "🇯🇵 日本語",
            "de-DE" => "🇩🇪 Deutsch",
            "fr-FR" => "🇫🇷 Français",
            "ur-PK" => "🇵🇰 اردو",
            "id-ID" => "🇮🇩 Bahasa Indonesia",
            "tr-TR" => "🇹🇷 Türkçe",
            "it-IT" => "🇮🇹 Italiano",
            "vi-VN" => "🇻🇳 Tiếng Việt",
            "ko-KR" => "🇰🇷 한국어",
            "ta-IN" => "🇮🇳 தமிழ்",
            "te-IN" => "🇮🇳 తెలుగు",
            "mr-IN" => "🇮🇳 मराठी",
            "pl-PL" => "🇵🇱 Polski",
            _ => "🇺🇸 English"
        };
    }

    private string GetCultureCodeFromDisplayName(string displayName)
    {
        return displayName switch
        {
            "🇪🇸 Español" => "es-ES",
            "🇨🇳 中文 (简体)" => "zh-CN",
            "🇮🇳 हिन्दी" => "hi-IN",
            "🇸🇦 العربية" => "ar-SA",
            "🇧🇩 বাংলা" => "bn-BD",
            "🇵🇹 Português" => "pt-PT",
            "🇷🇺 Русский" => "ru-RU",
            "🇯🇵 日本語" => "ja-JP",
            "🇩🇪 Deutsch" => "de-DE",
            "🇫🇷 Français" => "fr-FR",
            "🇵🇰 اردو" => "ur-PK",
            "🇮🇩 Bahasa Indonesia" => "id-ID",
            "🇹🇷 Türkçe" => "tr-TR",
            "🇮🇹 Italiano" => "it-IT",
            "🇻🇳 Tiếng Việt" => "vi-VN",
            "🇰🇷 한국어" => "ko-KR",
            "🇮🇳 தமிழ்" => "ta-IN",
            "🇮🇳 తెలుగు" => "te-IN",
            "🇮🇳 मराठी" => "mr-IN",
            "🇵🇱 Polski" => "pl-PL",
            _ => "en-US"
        };
    }

    private string GetLanguageChangedMessage(string cultureCode)
    {
        return cultureCode switch
        {
            "es-ES" => "Idioma cambiado a Español",
            "zh-CN" => "语言已更改为中文",
            "hi-IN" => "भाषा हिन्दी में बदली गई",
            "ar-SA" => "تم تغيير اللغة إلى العربية",
            "bn-BD" => "ভাষা বাংলায় পরিবর্তন করা হয়েছে",
            "pt-PT" => "Idioma alterado para Português",
            "ru-RU" => "Язык изменен на Русский",
            "ja-JP" => "言語が日本語に変更されました",
            "de-DE" => "Sprache auf Deutsch geändert",
            "fr-FR" => "Langue changée en Français",
            "ur-PK" => "زبان اردو میں تبدیل کر دی گئی",
            "id-ID" => "Bahasa diubah ke Bahasa Indonesia",
            "tr-TR" => "Dil Türkçe olarak değiştirildi",
            "it-IT" => "Lingua cambiata in Italiano",
            "vi-VN" => "Ngôn ngữ đã được thay đổi thành Tiếng Việt",
            "ko-KR" => "언어가 한국어로 변경되었습니다",
            "ta-IN" => "மொழி தமிழ் என மாற்றப்பட்டது",
            "te-IN" => "భాష తెలుగుకు మార్చబడింది",
            "mr-IN" => "भाषा मराठीमध्ये बदलली",
            "pl-PL" => "Język zmieniony na Polski",
            _ => "Language changed to English"
        };
    }

    private void UpdateUI()
    {
        try
        {
            // Update all UI elements with localized strings
            WelcomeLabel.Text = _localizer["Welcome"];
            StartIntakeBtn.Text = $"🚀 {_localizer["StartIntake"]}";
            ViewPackagesBtn.Text = $"📋 {_localizer["ViewPackages"]}";
            MyAccountBtn.Text = $"👤 {_localizer["MyAccount"]}";
            DescriptionLabel.Text = _localizer["Description"];

            // Update static labels
            AppTitleLabel.Text = _localizer["AppTitle"] ?? "Law4Hire";
            AppSubtitleLabel.Text = _localizer["AppSubtitle"] ?? "Your Legal Document Partner";

            _logger.LogInformation("UI updated for language: {Language} (Culture: {Culture})",
                _currentLanguage, CultureInfo.CurrentUICulture.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating UI for language: {Language}", _currentLanguage);
        }
    }

    // Debug method - remove this later
    private async void OnDebugCultureClicked(object sender, EventArgs e)
    {
        var currentCulture = CultureInfo.CurrentUICulture.Name;
        var welcomeText = _localizer["Welcome"];
        var startIntakeText = _localizer["StartIntake"];
        var savedLanguage = Preferences.Default.Get("app_language", "none");

        var message = $"Current Culture: {currentCulture}\n" +
                     $"Welcome Text: {welcomeText}\n" +
                     $"Start Intake Text: {startIntakeText}\n" +
                     $"Saved Language: {savedLanguage}\n" +
                     $"Display Name: {GetDisplayNameFromCultureCode(currentCulture)}";

        await DisplayAlert("Debug Info", message, "OK");
    }

    private async void OnStartIntakeClicked(object sender, EventArgs e)
    {
        try
        {
            _logger.LogInformation("Start intake button clicked");

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
            UpdateUI(); // Restore localized text
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
            UpdateUI(); // Restore localized text
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
            UpdateUI(); // Restore localized text
        }
    }

    private async void OnLanguageChanged(object sender, EventArgs e)
    {
        try
        {
            var picker = (Picker)sender;
            var selectedLanguage = picker.SelectedItem?.ToString();

            _logger.LogInformation("Language picker changed to: {Language}", selectedLanguage);

            // Map display names to culture codes
            var newCultureCode = GetCultureCodeFromDisplayName(selectedLanguage ?? "🇺🇸 English");

            // Only change if it's actually different
            if (newCultureCode != _currentLanguage)
            {
                // Set the new culture
                SetCulture(newCultureCode);

                // Update the UI with new localization
                UpdateUI();

                // Show confirmation in the new language
                var message = GetLanguageChangedMessage(newCultureCode);

                await DisplayAlert(_localizer["LanguageUpdated"] ?? "Language Updated", message, "OK");

                _logger.LogInformation("Language successfully changed to: {Language}", newCultureCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing language");
            await DisplayAlert("Error", "Failed to change language", "OK");
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _logger.LogInformation("MainPage appeared");

        // Refresh UI when page appears (in case culture changed elsewhere)
        UpdateUI();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _logger.LogInformation("MainPage disappeared");
    }
}