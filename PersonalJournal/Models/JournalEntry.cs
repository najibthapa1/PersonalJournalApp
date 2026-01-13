using System.ComponentModel.DataAnnotations;
using PersonalJournal.Components.Pages;

namespace PersonalJournal.Models;

public class JournalEntry
{
    [Key] public int Id { get; set; }
    [Required] public int UserId { get; set; }
    [Required] public DateTime EntryDate { get; set; }
    [StringLength(200)] public string? Title { get; set; }
    [Required] public string Content { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public int WordCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime?  UpdatedAt { get; set; }
    
    //Navigation Properties
    public virtual Users Users { get; set; } = null!;
    public virtual Category?  Category { get; set; }   
    public virtual ICollection<EntryMood> EntryMoods { get; set; } = new List<EntryMood>();
    public virtual ICollection<EntryTag> EntryTags { get; set; } = new List<EntryTag>();
}