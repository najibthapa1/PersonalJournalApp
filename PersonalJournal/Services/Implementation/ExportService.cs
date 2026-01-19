using PersonalJournal.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.RegularExpressions;
using PdfContainer = QuestPDF.Infrastructure.IContainer;
using PdfColors = QuestPDF.Helpers.Colors;

namespace PersonalJournal.Services.Implementation;

public class ExportService:IExportService
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

            // Generate PDF
            var fileName = $"Journal_Export_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            // Create PDF document
            QuestPDF.Settings.License = LicenseType.Community;

            Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(50);
                        page.PageColor(PdfColors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                        // Header
                        page.Header().Element(ComposeHeader);

                        // Content
                        page.Content().Element(content => ComposeContent(content, entries));

                        // Footer with page numbers
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                    });
                })
                .GeneratePdf(filePath);

            return (true, $"Successfully exported {entries.Count} entries to PDF", filePath);
        }
        catch (Exception ex)
        {
            return (false, $"Error exporting to PDF: {ex.Message}", null);
        }
    }
    
        private void ComposeHeader(PdfContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Personal Journal Export")
                        .FontSize(20)
                        .Bold()
                        .FontColor(PdfColors.Blue.Darken2);

                    column.Item().Text($"User: {_userService.CurrentUser?.Username}")
                        .FontSize(10)
                        .FontColor(PdfColors.Grey.Darken1);

                    column.Item().Text($"Exported: {DateTime.Now:MMMM dd, yyyy HH:mm}")
                        .FontSize(10)
                        .FontColor(PdfColors.Grey.Darken1);
                });
            });
        }

        private void ComposeContent(PdfContainer container, List<Models.JournalEntry> entries)
        {
            container.PaddingVertical(20).Column(column =>
            {
                column.Spacing(20);

                foreach (var entry in entries)
                {
                    column.Item().Element(c => ComposeEntry(c, entry));
                }
            });
        }

        private void ComposeEntry(PdfContainer container, Models.JournalEntry entry)
        {
            container.Border(1).BorderColor(PdfColors.Grey.Lighten2).Padding(15).Column(column =>
            {
                // Date
                column.Item().Text(entry.EntryDate.ToString("dddd, MMMM d, yyyy"))
                    .FontSize(14)
                    .Bold()
                    .FontColor(PdfColors.Blue.Darken1);

                column.Item().PaddingVertical(5);

                // Title (if exists)
                if (!string.IsNullOrEmpty(entry.Title))
                {
                    column.Item().Text(entry.Title)
                        .FontSize(13)
                        .SemiBold()
                        .FontColor(PdfColors.Black);
                    
                    column.Item().PaddingVertical(3);
                }

                // Mood
                var primaryMood = entry.EntryMoods.FirstOrDefault(em => em.IsPrimary);
                if (primaryMood != null)
                {
                    column.Item().Text($"Mood: {primaryMood.Mood.Name} ({primaryMood.Mood.Category})")
                        .FontSize(10)
                        .FontColor(GetMoodColor(primaryMood.Mood.Category));
                }

                // Tags
                if (entry.EntryTags.Any())
                {
                    var tags = string.Join(", ", entry.EntryTags.Select(et => et.Tag.Name));
                    column.Item().Text($"Tags: {tags}")
                        .FontSize(10)
                        .FontColor(PdfColors.Grey.Darken1);
                }

                column.Item().PaddingVertical(8);

                // Content (strip HTML)
                var textContent = StripHtml(entry.Content);
                column.Item().Text(textContent)
                    .FontSize(11)
                    .LineHeight(1.5f);

                column.Item().PaddingVertical(5);

                // Word count
                column.Item().AlignRight().Text($"{entry.WordCount} words")
                    .FontSize(9)
                    .Italic()
                    .FontColor(PdfColors.Grey.Medium);
            });
        }

        private string GetMoodColor(Models.Enums.MoodCategory category)
        {
            return category switch
            {
                Models.Enums.MoodCategory.Positive => PdfColors.Green.Darken1,
                Models.Enums.MoodCategory.Neutral => PdfColors.Grey.Darken1,
                Models.Enums.MoodCategory.Negative => PdfColors.Red.Darken1,
                _ => PdfColors.Black
            };
        }

        private string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove HTML tags
            var text = Regex.Replace(html, "<.*?>", " ");
            
            // Replace multiple spaces with single space
            text = Regex.Replace(text, @"\s+", " ");
            
            return text.Trim();
        }
    
}