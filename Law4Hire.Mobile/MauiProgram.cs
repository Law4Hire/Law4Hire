using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Law4Hire.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Add localization services
        builder.Services.AddLocalization(options =>
        {
            options.ResourcesPath = "Resources";
        });

        // Register MainPage with DI
        builder.Services.AddTransient<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}