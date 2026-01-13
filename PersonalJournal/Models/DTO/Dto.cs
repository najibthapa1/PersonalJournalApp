using System.ComponentModel.DataAnnotations;
namespace PersonalJournal.Models.DTO;

public class RegisterDto
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string Username { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "PIN is required")]
    [StringLength(6, MinimumLength = 4, ErrorMessage = "PIN must be between 4 and 6 digits")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "PIN must contain only numbers")]
    public string Pin { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Please confirm your PIN")]
    [Compare("Pin", ErrorMessage = "PINs do not match")]
    public string ConfirmPin { get; set; } = string.Empty;
}

public class LoginDto
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "PIN is required")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "PIN must contain only numbers")]
    public string Pin { get; set; } = string.Empty;

}

public class MoodDistributionDto
{
    public double PositivePercentage { get; set; }
    public double NeutralPercentage { get; set; }
    public double NegativePercentage { get; set; }
    public int PositiveCount { get; set; }
    public int NeutralCount { get; set; }
    public int NegativeCount { get; set; }
    public int TotalEntries { get; set; }
}

public class TagUsageDto
{
    public string TagName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
    public double Percentage { get; set; }
}

public class WordCountDto
{
    public DateTime Date { get; set; }
    public int WordCount { get; set; }
}