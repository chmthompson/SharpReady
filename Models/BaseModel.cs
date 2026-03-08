using System.ComponentModel.DataAnnotations;

namespace SharpReady.Models;

/// <summary>
/// Base model class for all entities
/// </summary>
public abstract class BaseModel
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
