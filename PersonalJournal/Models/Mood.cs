using System.ComponentModel.DataAnnotations;
using PersonalJournal.Models.Enums;

namespace PersonalJournal.Models;

public class Mood
{
    [Key]public int Id { get; set; }
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;
    [Required] public MoodCategory Category { get; set; }
    public string Emoji { get; set; } = string.Empty;
    
    //Navigation property
    public virtual ICollection<EntryMood> EntryMoods { get; set; } = new List<EntryMood>();
    
}