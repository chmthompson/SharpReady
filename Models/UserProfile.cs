using System.ComponentModel.DataAnnotations;

namespace DotNetStudyAssistant.Models;

/// <summary>
/// User profile model representing a user in the application
/// </summary>
public class UserProfile : BaseModel
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
