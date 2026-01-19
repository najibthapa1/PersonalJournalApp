using PersonalJournal.Models.Enums;
namespace PersonalJournal.Services.Interfaces;

/// <summary>
/// Manages theme (light/dark mode)
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets current theme for the user
    /// </summary>
    Task<ThemeMode> GetCurrentThemeAsync();

    /// <summary>
    /// Toggles between light and dark theme
    /// </summary>
    Task ToggleThemeAsync();

    /// <summary>
    /// Sets a specific theme
    /// </summary>
    Task SetThemeAsync(ThemeMode theme);

    /// <summary>
    /// Event fired when theme changes
    /// </summary>
    event Action? OnThemeChanged;
}