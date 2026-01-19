using PersonalJournal.Models;
using PersonalJournal.Models.Enums;
namespace PersonalJournal.Services.Interfaces;

/// <summary>
/// Manages mood operations
/// </summary>
public interface IMoodService
{
    /// <summary>
    /// Gets all available moods
    /// </summary>
    Task<List<Mood>> GetAllMoodsAsync();

    /// <summary>
    /// Gets moods by category (Positive, Neutral, Negative)
    /// </summary>
    Task<List<Mood>> GetMoodsByCategoryAsync(MoodCategory category);

    /// <summary>
    /// Gets a mood by ID
    /// </summary>
    Task<Mood?> GetMoodByIdAsync(int moodId);
}