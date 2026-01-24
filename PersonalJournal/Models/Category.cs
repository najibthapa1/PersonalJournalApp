using System.ComponentModel.DataAnnotations;
namespace PersonalJournal.Models;

public class Category
{
    [Key] public int Id { get; set; }
    [Required] [StringLength(100)] public string Name { get; set; } = string.Empty;
    
    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}