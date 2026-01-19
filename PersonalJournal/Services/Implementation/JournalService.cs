using PersonalJournal.Data;
using PersonalJournal.Models;
using PersonalJournal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace PersonalJournal.Services.Implementation;

public class JournalService:IJournalService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;
      public JournalService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<JournalEntry?> GetTodaysEntryAsync()
        {
            if (_userService.CurrentUser == null) return null;

            var today = DateTime.Today;
            return await _context.JournalEntries
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.UserId == _userService.CurrentUser.Id && 
                                         e.EntryDate.Date == today);
        }

        public async Task<JournalEntry?> GetEntryByDateAsync(DateTime date)
        {
            if (_userService.CurrentUser == null) return null;

            return await _context.JournalEntries
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.UserId == _userService.CurrentUser.Id && 
                                         e.EntryDate.Date == date.Date);
        }

        public async Task<(bool success, string message, JournalEntry? entry)> SaveEntryAsync(
            JournalEntry entry, 
            List<int> moodIds, 
            int primaryMoodId, 
            List<int> tagIds)
        {
            try
            {
                if (_userService.CurrentUser == null)
                {
                    return (false, "User not authenticated", null);
                }

                // Validate one entry per day
                var existingEntry = await _context.JournalEntries
                    .FirstOrDefaultAsync(e => 
                        e.UserId == _userService.CurrentUser.Id && 
                        e.EntryDate.Date == entry.EntryDate.Date &&
                        e.Id != entry.Id);

                if (existingEntry != null && entry.Id == 0)
                {
                    return (false, "An entry already exists for this date", null);
                }

                // Calculate word count from HTML content
                entry.WordCount = CalculateWordCount(entry.Content);

                // Set user ID
                entry.UserId = _userService.CurrentUser.Id;

                if (entry.Id == 0)
                {
                    // New entry
                    entry.CreatedAt = DateTime.UtcNow;
                    _context.JournalEntries.Add(entry);
                }
                else
                {
                    // Update existing entry
                    entry.UpdatedAt = DateTime.UtcNow;
                    _context.JournalEntries.Update(entry);

                    // Remove existing moods and tags
                    var existingMoods = _context.EntryMoods.Where(em => em.JournalEntryId == entry.Id);
                    _context.EntryMoods.RemoveRange(existingMoods);

                    var existingTags = _context.EntryTags.Where(et => et.JournalEntryId == entry.Id);
                    _context.EntryTags.RemoveRange(existingTags);
                }

                await _context.SaveChangesAsync();

                // Add moods
                if (moodIds.Any())
                {
                    foreach (var moodId in moodIds)
                    {
                        var entryMood = new EntryMood
                        {
                            JournalEntryId = entry.Id,
                            MoodId = moodId,
                            IsPrimary = moodId == primaryMoodId
                        };
                        _context.EntryMoods.Add(entryMood);
                    }
                }

                // Add tags
                if (tagIds.Any())
                {
                    foreach (var tagId in tagIds)
                    {
                        var entryTag = new EntryTag
                        {
                            JournalEntryId = entry.Id,
                            TagId = tagId
                        };
                        _context.EntryTags.Add(entryTag);
                    }
                }

                await _context.SaveChangesAsync();

                // Reload entry with relationships
                var savedEntry = await GetEntryByDateAsync(entry.EntryDate);

                return (true, "Entry saved successfully!", savedEntry);
            }
            catch (Exception ex)
            {
                return (false, $"Error saving entry: {ex.Message}", null);
            }
        }

        public async Task<List<JournalEntry>> GetAllEntriesAsync()
        {
            if (_userService.CurrentUser == null) 
                return new List<JournalEntry>();

            return await _context.JournalEntries
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .Include(e => e.Category)
                .Where(e => e.UserId == _userService.CurrentUser.Id)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        public async Task<(List<JournalEntry> entries, int totalCount)> GetEntriesPaginatedAsync(int page, int pageSize)
        {
            if (_userService.CurrentUser == null) 
                return (new List<JournalEntry>(), 0);

            var query = _context.JournalEntries
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .Include(e => e.Category)
                .Where(e => e.UserId == _userService.CurrentUser.Id)
                .OrderByDescending(e => e.EntryDate);

            var totalCount = await query.CountAsync();
            var entries = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (entries, totalCount);
        }

        public async Task<List<JournalEntry>> GetEntriesByMonthAsync(int year, int month)
        {
            if (_userService.CurrentUser == null) 
                return new List<JournalEntry>();

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await _context.JournalEntries
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .Include(e => e.Category)
                .Where(e => e.UserId == _userService.CurrentUser.Id &&
                           e.EntryDate >= startDate &&
                           e.EntryDate <= endDate)
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        public async Task<List<JournalEntry>> SearchEntriesAsync(string searchTerm)
        {
            if (_userService.CurrentUser == null || string.IsNullOrWhiteSpace(searchTerm))
                return new List<JournalEntry>();

            var lowerSearchTerm = searchTerm.ToLower();

            return await _context.JournalEntries
                .Include(e => e.EntryMoods)
                    .ThenInclude(em => em.Mood)
                .Include(e => e.EntryTags)
                    .ThenInclude(et => et.Tag)
                .Include(e => e.Category)
                .Where(e => e.UserId == _userService.CurrentUser.Id &&
                           ((e.Title != null && e.Title.ToLower().Contains(lowerSearchTerm)) ||
                            e.Content.ToLower().Contains(lowerSearchTerm)))
                .OrderByDescending(e => e.EntryDate)
                .ToListAsync();
        }

        public async Task<bool> DeleteEntryAsync(int entryId)
        {
            try
            {
                if (_userService.CurrentUser == null) return false;

                var entry = await _context.JournalEntries
                    .FirstOrDefaultAsync(e => e.Id == entryId && 
                                             e.UserId == _userService.CurrentUser.Id);

                if (entry == null) return false;

                _context.JournalEntries.Remove(entry);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        // Helper method to calculate word count from HTML
        private int CalculateWordCount(string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(htmlContent))
                return 0;

            // Strip HTML tags
            var textContent = Regex.Replace(htmlContent, "<.*?>", " ");
            
            // Replace multiple whitespace with single space
            textContent = Regex.Replace(textContent, @"\s+", " ");
            
            // Count words
            var words = textContent.Trim()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return words.Length;
        }
}