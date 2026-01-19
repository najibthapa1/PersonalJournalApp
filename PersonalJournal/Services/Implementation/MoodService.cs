using PersonalJournal.Data;
using PersonalJournal.Models;
using PersonalJournal.Models.Enums;
using PersonalJournal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PersonalJournal.Services.Implementation;

public class MoodService: IMoodService
{
    private readonly AppDbContext _context;

    public MoodService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Mood>> GetAllMoodsAsync()
    {
        return await _context.Moods
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<List<Mood>> GetMoodsByCategoryAsync(MoodCategory category)
    {
        return await _context.Moods
            .Where(m => m.Category == category)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Mood?> GetMoodByIdAsync(int moodId)
    {
        return await _context.Moods.FindAsync(moodId);
    }
}