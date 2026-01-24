using PersonalJournal.Services.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace PersonalJournal.Services.Implementation;

public class ExportService : IExportService
{
    private readonly IJournalService _journalService;
    private readonly IUserService _userService;

    public ExportService(IJournalService journalService, IUserService userService)
    {
        _journalService = journalService;
        _userService = userService;
    }

    public async Task<(bool success, string message, string? filePath)> ExportToPdfAsync(
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            if (_userService.CurrentUser == null)
            {
                return (false, "User not authenticated", null);
            }

            // Get entries in date range
            var allEntries = await _journalService.GetAllEntriesAsync();

            var entries = allEntries.Where(e =>
            {
                if (startDate.HasValue && e.EntryDate < startDate.Value) return false;
                if (endDate.HasValue && e.EntryDate > endDate.Value) return false;
                return true;
            }).OrderBy(e => e.EntryDate).ToList();

            if (!entries.Any())
            {
                return (false, "No entries found in the selected date range", null);
            }

            // Generate HTML
            var fileName = $"Journal_Export_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            
            // Use Documents folder for easier access
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var journalFolder = Path.Combine(documentsPath, "Journal Exports");
            
            // Create folder if it doesn't exist
            if (!Directory.Exists(journalFolder))
            {
                Directory.CreateDirectory(journalFolder);
            }
            
            var filePath = Path.Combine(journalFolder, fileName);

            var htmlContent = GenerateHtml(entries);
            await File.WriteAllTextAsync(filePath, htmlContent);

            return (true, $"Successfully exported {entries.Count} entries. File saved to Documents/Journal Exports/", filePath);
        }
        catch (Exception ex)
        {
            return (false, $"Error exporting: {ex.Message}", null);
        }
    }

    private string GenerateHtml(List<Models.JournalEntry> entries)
    {
        var sb = new StringBuilder();

        sb.AppendLine(@"<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Personal Journal Export</title>
    <style>
        @media print {
            @page {
                margin: 2cm;
            }
            .no-print {
                display: none;
            }
            .entry {
                page-break-inside: avoid;
            }
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 900px;
            margin: 0 auto;
            padding: 40px 20px;
            background: #f9fafb;
        }

        .header {
            text-align: center;
            margin-bottom: 40px;
            padding-bottom: 20px;
            border-bottom: 3px solid #3b82f6;
        }

        .header h1 {
            font-size: 32px;
            color: black;
            margin-bottom: 10px;
        }

        .header .meta {
            color: black;
            font-size: 14px;
        }

        .print-button {
            background: black;
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 8px;
            font-size: 16px;
            font-weight: 600;
            cursor: pointer;
            margin: 20px auto;
            display: block;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        }

        .print-button:hover {
            background: black;
        }

        .entry {
            background: white;
            margin-bottom: 30px;
            padding: 30px;
            border-radius: 12px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
        }

        .entry-date {
            font-size: 18px;
            font-weight: 700;
            color: black;
            margin-bottom: 10px;
        }

        .entry-title {
            font-size: 22px;
            font-weight: 600;
            color: black;
            margin-bottom: 15px;
        }

        .entry-meta {
            display: flex;
            gap: 15px;
            flex-wrap: wrap;
            margin-bottom: 15px;
            font-size: 14px;
        }

        .mood {
            padding: 4px 12px;
            border-radius: 20px;
            font-weight: 600;
        }

        .mood-positive {
            background: #dcfce7;
            color: #15803d;
        }

        .mood-neutral {
            background: #dbeafe;
            color: #1e40af;
        }

        .mood-negative {
            background: #fee2e2;
            color: #b91c1c;
        }

        .tags {
            color: #64748b;
        }

        .entry-content {
            margin-top: 20px;
            line-height: 1.8;
            color: #334155;
        }

        .entry-footer {
            margin-top: 15px;
            padding-top: 15px;
            border-top: 1px solid #e2e8f0;
            text-align: right;
            font-size: 12px;
            color: #94a3af;
            font-style: italic;
        }

        .summary {
            background: white;
            padding: 20px;
            border-radius: 12px;
            margin-bottom: 30px;
            text-align: center;
        }

        .summary h2 {
            color: black;
            margin-bottom: 10px;
        }
    </style>
</head>
<body>
    <div class='header'>
        <h1>📖 Personal Journal</h1>
        <div class='meta'>
            <strong>User:</strong> " + _userService.CurrentUser?.Username + @"<br>
            <strong>Exported:</strong> " + DateTime.Now.ToString("MMMM dd, yyyy 'at' HH:mm") + @"
        </div>
    </div>

    <button class='print-button no-print' onclick='window.print()'>
        🖨️ Save as PDF
    </button>

    <div class='summary'>
        <h2>Export Summary</h2>
        <p><strong>" + entries.Count + @"</strong> journal entries</p>
    </div>
");

        foreach (var entry in entries)
        {
            sb.AppendLine("    <div class='entry'>");
            sb.AppendLine($"        <div class='entry-date'>{entry.EntryDate:dddd, MMMM d, yyyy}</div>");

            if (!string.IsNullOrWhiteSpace(entry.Title))
            {
                sb.AppendLine($"        <div class='entry-title'>{System.Net.WebUtility.HtmlEncode(entry.Title)}</div>");
            }

            sb.AppendLine("        <div class='entry-meta'>");

            // Mood
            var primaryMood = entry.EntryMoods.FirstOrDefault(em => em.IsPrimary);
            if (primaryMood != null)
            {
                var moodClass = primaryMood.Mood.Category switch
                {
                    Models.Enums.MoodCategory.Positive => "mood-positive",
                    Models.Enums.MoodCategory.Neutral => "mood-neutral",
                    Models.Enums.MoodCategory.Negative => "mood-negative",
                    _ => ""
                };
                sb.AppendLine($"            <span class='mood {moodClass}'>{primaryMood.Mood.Name}</span>");
            }

            // Tags
            if (entry.EntryTags.Any())
            {
                var tags = string.Join(", ", entry.EntryTags.Select(et => et.Tag.Name));
                sb.AppendLine($"            <span class='tags'>🏷️ {System.Net.WebUtility.HtmlEncode(tags)}</span>");
            }

            sb.AppendLine("        </div>");

            // Content
            sb.AppendLine("        <div class='entry-content'>");
            sb.AppendLine($"            {entry.Content}");
            sb.AppendLine("        </div>");

            // Footer
            sb.AppendLine("        <div class='entry-footer'>");
            sb.AppendLine($"            {entry.WordCount} words");
            sb.AppendLine("        </div>");

            sb.AppendLine("    </div>");
        }

        sb.AppendLine(@"
</body>
</html>");

        return sb.ToString();
    }
}