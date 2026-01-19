using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersonalJournal.Services.Implementation;
using PersonalJournal.Services.Interfaces;
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
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "journal.db");
        
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
        
        builder.Services.AddSingleton<IUserService, UserService>();

        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IJournalService, JournalService>();
        builder.Services.AddScoped<IMoodService, MoodService>();
        builder.Services.AddScoped<ITagService, TagService>();
        builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
        builder.Services.AddScoped<IStreakService, StreakService>();
        builder.Services.AddScoped<IThemeService, ThemeService>();
        builder.Services.AddScoped<IExportService, ExportService>();
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