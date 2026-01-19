using PersonalJournal.Models;
namespace PersonalJournal.Services.Interfaces;

/// <summary>
/// Manages journal entry CRUD operations
/// </summary>
public interface IJournalService
{
    /// <summary>
    /// Gets today's entry for the current user
    /// </summary>
    Task<JournalEntry?> GetTodaysEntryAsync();

    /// <summary>
    /// Gets entry for a specific date
    /// </summary>
    Task<JournalEntry?> GetEntryByDateAsync(DateTime date);

    /// <summary>
    /// Saves (creates or updates) an entry
    /// </summary>
    Task<(bool success, string message, JournalEntry? entry)> SaveEntryAsync(
        JournalEntry entry,
        List<int> moodIds,
        int primaryMoodId,
        List<int> tagIds);

    /// <summary>
    /// Gets all entries for the current user
    /// </summary>
    Task<List<JournalEntry>> GetAllEntriesAsync();

    /// <summary>
    /// Gets entries with pagination
    /// </summary>
    Task<(List<JournalEntry> entries, int totalCount)> GetEntriesPaginatedAsync(int page, int pageSize);

    /// <summary>
    /// Gets entries for a specific month
    /// </summary>
    Task<List<JournalEntry>> GetEntriesByMonthAsync(int year, int month);

    /// <summary>
    /// Searches entries by title or content
    /// </summary>
    Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm);

    /// <summary>
    /// Deletes an entry
    /// </summary>
    Task<bool> DeleteEntryAsync(int entryId);
}