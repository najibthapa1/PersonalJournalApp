using PersonalJournal.Models;
namespace PersonalJournal.Services.Interfaces;

/// <summary>
/// Manages tag operations (pre-defined and custom tags)
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Gets all tags (pre-defined + user's custom tags)
    /// </summary>
    Task<List<Tag>> GetAllTagsAsync();

    /// <summary>
    /// Gets only pre-defined tags
    /// </summary>
    Task<List<Tag>> GetPreDefinedTagsAsync();

    /// <summary>
    /// Gets user's custom tags
    /// </summary>
    Task<List<Tag>> GetCustomTagsAsync();

    /// <summary>
    /// Creates a new custom tag for the current user
    /// </summary>
    Task<(bool success, string message, Tag? tag)> CreateCustomTagAsync(string tagName);
}