using PersonalJournal.Models.DTO;
namespace PersonalJournal.Services.Interfaces;

/// <summary>
/// Provides analytics and statistics
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Gets mood distribution (positive, neutral, negative percentages)
    /// </summary>
    Task<MoodDistributionDto> GetMoodDistributionAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets the most frequently used mood
    /// </summary>
    Task<string?> GetMostFrequentMoodAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets most used tags with usage counts
    /// </summary>
    Task<List<TagUsageDto>> GetMostUsedTagsAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets word count trend over time
    /// </summary>
    Task<List<WordCountDto>> GetWordCountTrendAsync(DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Gets average word count
    /// </summary>
    Task<double> GetAverageWordCountAsync(DateTime? startDate = null, DateTime? endDate = null);
}