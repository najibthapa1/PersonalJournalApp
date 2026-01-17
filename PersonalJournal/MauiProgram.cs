using Microsoft.Extensions.Logging;
using PersonalJournal.Data;

namespace PersonalJournal;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        string dbPath = GetDatabasePath();
        
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        var app = builder.Build();
        
        // Create database if it doesn't exist
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();
        }

        return app;
    }

}