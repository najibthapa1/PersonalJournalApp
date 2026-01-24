namespace PersonalJournal.Models;

public class EntryTag
{
    public int JournalEntryId { get; set; }
    public int TagId { get; set; }

    public virtual JournalEntry JournalEntry { get; set; } = null!;
    public virtual Tag Tag { get; set; } = null!;
}