namespace PersonalJournal.Services.Interfaces;

/// <summary>
/// Handles exporting journal entries
/// </summary>
public interface IExportService
{
    /// <summary>
    /// Exports journal entries to PDF
    /// </summary>
    Task<(bool success, string message, string? filePath)> ExportToPdfAsync(
        DateTime? startDate = null, 
        DateTime? endDate = null);
}