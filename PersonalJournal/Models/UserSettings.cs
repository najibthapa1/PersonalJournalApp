using System.ComponentModel.DataAnnotations;
using PersonalJournal.Models.Enums;
namespace PersonalJournal.Models;

public class UserSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    public ThemeMode ThemeMode { get; set; } = ThemeMode.Light;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public virtual Users User { get; set; } = null!;
}