using PersonalJournal.Services.Interfaces;
using PersonalJournal.Data;
using PersonalJournal.Models.DTO;
using PersonalJournal.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace PersonalJournal.Services.Implementation;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public AnalyticsService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<MoodDistributionDto> GetMoodDistributionAsync(DateTime? startDate = null,
        DateTime? endDate = null)
    {
        if (_userService.CurrentUser == null)
        {
            return new MoodDistributionDto();
        }

        var query = _context.JournalEntries
            .Include(e => e.EntryMoods)
            .ThenInclude(em => em.Mood)
            .Where(e => e.UserId == _userService.CurrentUser.Id);

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        var entries = await query.ToListAsync();

        if (!entries.Any())
        {
            return new MoodDistributionDto();
        }
        // Count primary moods only by category
        var moodCounts = entries
            .SelectMany(e => e.EntryMoods.Where(em => em.IsPrimary))
            .GroupBy(em => em.Mood.Category)
            .ToDictionary(g => g.Key, g => g.Count());

        var positiveCount = moodCounts.GetValueOrDefault(MoodCategory.Positive, 0);
        var neutralCount = moodCounts.GetValueOrDefault(MoodCategory.Neutral, 0);
        var negativeCount = moodCounts.GetValueOrDefault(MoodCategory.Negative, 0);
        var total = positiveCount + neutralCount + negativeCount;

        return new MoodDistributionDto
        {
            PositiveCount = positiveCount,
            NeutralCount = neutralCount,
            NegativeCount = negativeCount,
            TotalEntries = total,
            PositivePercentage = total > 0 ? Math.Round((double)positiveCount / total * 100, 1) : 0,
            NeutralPercentage = total > 0 ? Math.Round((double)neutralCount / total * 100, 1) : 0,
            NegativePercentage = total > 0 ? Math.Round((double)negativeCount / total * 100, 1) : 0
        };

    }
    public async Task<string?> GetMostFrequentMoodAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (_userService.CurrentUser == null) return null;

        var query = _context.JournalEntries
            .Include(e => e.EntryMoods)
            .ThenInclude(em => em.Mood)
            .Where(e => e.UserId == _userService.CurrentUser.Id);

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        var entries = await query.ToListAsync();

        if (!entries.Any()) return null;

        var mostFrequentMood = entries
            .SelectMany(e => e.EntryMoods.Where(em => em.IsPrimary))
            .GroupBy(em => em.Mood.Name)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return mostFrequentMood?.Key;
    }
    public async Task<List<TagUsageDto>> GetMostUsedTagsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (_userService.CurrentUser == null) 
            return new List<TagUsageDto>();

        var query = _context.JournalEntries
            .Include(e => e.EntryTags)
            .ThenInclude(et => et.Tag)
            .Where(e => e.UserId == _userService.CurrentUser.Id);

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        var entries = await query.ToListAsync();

        if (!entries.Any()) return new List<TagUsageDto>();

        var totalEntries = entries.Count;

        var tagUsage = entries
            .SelectMany(e => e.EntryTags)
            .GroupBy(et => et.Tag.Name)
            .Select(g => new TagUsageDto
            {
                TagName = g.Key,
                UsageCount = g.Count(),
                Percentage = Math.Round((double)g.Count() / totalEntries * 100, 1)
            })
            .OrderByDescending(t => t.UsageCount)
            .Take(10)
            .ToList();

        return tagUsage;
    }
    public async Task<List<WordCountDto>> GetWordCountTrendAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (_userService.CurrentUser == null) 
            return new List<WordCountDto>();

        var query = _context.JournalEntries
            .Where(e => e.UserId == _userService.CurrentUser.Id);

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        var entries = await query
            .OrderBy(e => e.EntryDate)
            .Select(e => new WordCountDto
            {
                Date = e.EntryDate,
                WordCount = e.WordCount
            })
            .ToListAsync();

        return entries;
    }
    public async Task<double> GetAverageWordCountAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        if (_userService.CurrentUser == null) return 0;

        var query = _context.JournalEntries
            .Where(e => e.UserId == _userService.CurrentUser.Id);

        if (startDate.HasValue)
            query = query.Where(e => e.EntryDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(e => e.EntryDate <= endDate.Value);

        var entries = await query.ToListAsync();

        if (!entries.Any()) return 0;

        return Math.Round(entries.Average(e => e.WordCount), 1);
    }
}