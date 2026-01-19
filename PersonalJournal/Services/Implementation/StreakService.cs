using PersonalJournal.Data;
using PersonalJournal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PersonalJournal.Services.Implementation;

public class StreakService: IStreakService
{
     private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public StreakService(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<int> GetCurrentStreakAsync()
        {
            if (_userService.CurrentUser == null) return 0;

            var entries = await _context.JournalEntries
                .Where(e => e.UserId == _userService.CurrentUser.Id)
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();

            if (!entries.Any()) return 0;

            int streak = 0;
            var currentDate = DateTime.Today;

            // Check if there's an entry today or yesterday
            if (entries.First() != currentDate && entries.First() != currentDate.AddDays(-1))
            {
                return 0; // Streak broken
            }

            // If today has no entry, start from yesterday
            if (entries.First() != currentDate)
            {
                currentDate = currentDate.AddDays(-1);
            }

            // Count consecutive days
            foreach (var entryDate in entries)
            {
                if (entryDate == currentDate)
                {
                    streak++;
                    currentDate = currentDate.AddDays(-1);
                }
                else
                {
                    break;
                }
            }

            return streak;
        }

        public async Task<int> GetLongestStreakAsync()
        {
            if (_userService.CurrentUser == null) return 0;

            var entries = await _context.JournalEntries
                .Where(e => e.UserId == _userService.CurrentUser.Id)
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            if (!entries.Any()) return 0;

            int maxStreak = 1;
            int currentStreak = 1;

            for (int i = 1; i < entries.Count; i++)
            {
                // Check if dates are consecutive
                if ((entries[i] - entries[i - 1]).Days == 1)
                {
                    currentStreak++;
                    maxStreak = Math.Max(maxStreak, currentStreak);
                }
                else
                {
                    currentStreak = 1; // Reset streak
                }
            }

            return maxStreak;
        }

        public async Task<int> GetMissedDaysAsync(DateTime startDate, DateTime endDate)
        {
            if (_userService.CurrentUser == null) return 0;

            var entries = await _context.JournalEntries
                .Where(e => e.UserId == _userService.CurrentUser.Id &&
                           e.EntryDate >= startDate &&
                           e.EntryDate <= endDate)
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .ToListAsync();

            var totalDays = (endDate.Date - startDate.Date).Days + 1;
            return totalDays - entries.Count;
        }

        public async Task<List<DateTime>> GetEntryDatesAsync()
        {
            if (_userService.CurrentUser == null) 
                return new List<DateTime>();

            return await _context.JournalEntries
                .Where(e => e.UserId == _userService.CurrentUser.Id)
                .Select(e => e.EntryDate.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();
        }
}