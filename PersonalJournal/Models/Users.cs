using System;
using System.ComponentModel.DataAnnotations;
namespace PersonalJournal.Models;

public class Users
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required] public string PinHash { get; set; } = string.Empty;

    [Required] public string Salt { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    
    //Navigation Properties
    public virtual ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
    public virtual UserSettings? UserSettings { get; set; }
}