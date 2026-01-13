using System.ComponentModel.DataAnnotations;
namespace PersonalJournal.Models;

public class Tag
{
    [Key]public int Id { get; set; }
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;
    public bool IsCustom { get; set; } = false;
    public int? UserId {get; set; }
    
    //Navigation properties
    public virtual Users?  User { get; set; }
    public virtual ICollection<EntryTag> EntryTags { get; set; } = new List<EntryTag>();
}