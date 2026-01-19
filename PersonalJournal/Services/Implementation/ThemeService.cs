using PersonalJournal.Services.Interfaces;
using PersonalJournal.Models.Enums;
using PersonalJournal.Data;
using Microsoft.EntityFrameworkCore;

namespace PersonalJournal.Services.Implementation;

public class ThemeService: IThemeService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
    private ThemeMode _currentTheme = ThemeMode.Light;

    public event Action? OnThemeChanged;

    public ThemeService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<ThemeMode> GetCurrentThemeAsync()
    {
        if (_userService.CurrentUser == null) return ThemeMode.Light;

        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == _userService.CurrentUser.Id);

        _currentTheme = settings?.ThemeMode ?? ThemeMode.Light;
        return _currentTheme;
    }

    public async Task ToggleThemeAsync()
    {
        if (_userService.CurrentUser == null) return;

        var currentTheme = await GetCurrentThemeAsync();
        var newTheme = currentTheme == ThemeMode.Light ? ThemeMode.Dark : ThemeMode.Light;

        await SetThemeAsync(newTheme);
    }

    public async Task SetThemeAsync(ThemeMode theme)
    {
        if (_userService.CurrentUser == null) return;

        var settings = await _context.UserSettings
            .FirstOrDefaultAsync(s => s.UserId == _userService.CurrentUser.Id);

        if (settings != null)
        {
            settings.ThemeMode = theme;
            settings.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _currentTheme = theme;
            OnThemeChanged?.Invoke();
        }
    }
}