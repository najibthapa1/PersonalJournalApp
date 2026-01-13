namespace PersonalJournal.Models;

public class EntryMood
{
    public int JournalEntryId { get; set; }
    public int MoodId { get; set; }
    public bool IsPrimary { get; set; } // True for primary mood, false for secondary
    
    //Navigation properties
    public virtual JournalEntry JournalEntry { get; set; } = null!;
    public virtual Mood Mood { get; set; } = null!;
}