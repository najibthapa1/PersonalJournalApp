using PersonalJournal.Models;
using PersonalJournal.Data;
using PersonalJournal.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace PersonalJournal.Services.Implementation;

public class TagService : ITagService
{
    private readonly AppDbContext _context;
    private readonly IUserService _userService;

    public TagService(AppDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<List<Tag>> GetAllTagsAsync()
    {
        if (_userService.CurrentUser == null)
            return new List<Tag>();

        // Get pre-defined tags + user's custom tags
        return await _context.Tags
            .Where(t => !t.IsCustom || t.UserId == _userService.CurrentUser.Id)
            .OrderBy(t => t.IsCustom)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetPreDefinedTagsAsync()
    {
        return await _context.Tags
            .Where(t => !t.IsCustom)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Tag>> GetCustomTagsAsync()
    {
        if (_userService.CurrentUser == null)
            return new List<Tag>();

        return await _context.Tags
            .Where(t => t.IsCustom && t.UserId == _userService.CurrentUser.Id)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<(bool success, string message, Tag? tag)> CreateCustomTagAsync(string tagName)
    {
        try
        {
            if (_userService.CurrentUser == null)
            {
                return (false, "User not authenticated", null);
            }

            if (string.IsNullOrWhiteSpace(tagName))
            {
                return (false, "Tag name cannot be empty", null);
            }

            // Check if tag already exists for this user
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == tagName.ToLower() &&
                                          (t.UserId == _userService.CurrentUser.Id || !t.IsCustom));

            if (existingTag != null)
            {
                return (false, "Tag already exists", null);
            }

            var newTag = new Tag
            {
                Name = tagName.Trim(),
                IsCustom = true,
                UserId = _userService.CurrentUser.Id
            };

            _context.Tags.Add(newTag);
            await _context.SaveChangesAsync();

            return (true, "Tag created successfully", newTag);
        }
        catch (Exception ex)
        {
            return (false, $"Error creating tag: {ex.Message}", null);
        }
    }
}