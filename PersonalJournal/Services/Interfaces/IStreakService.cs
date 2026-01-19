namespace PersonalJournal.Services.Interfaces;

/// <summary>
/// Calculates streaks
/// </summary>
public interface IStreakService
{
    /// <summary>
    /// Gets current streak 
    /// </summary>
    Task<int> GetCurrentStreakAsync();

    /// <summary>
    /// Gets longest streak ever achieved
    /// </summary>
    Task<int> GetLongestStreakAsync();

    /// <summary>
    /// Gets number of missed days in a date range
    /// </summary>
    Task<int> GetMissedDaysAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets all dates that have entries
    /// </summary>
    Task<List<DateTime>> GetEntryDatesAsync();
}