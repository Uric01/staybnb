using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Staybnb.Models;

public class ActivityLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)] // Meets the string(100) requirement
    public string Action { get; set; } = string.Empty;

    [Required]
    public ActivityType ActivityType { get; set; } // Renamed from ActionType

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Renamed from Timestamp

    // Foreign Keys
    [Required]
    public string UserId { get; set; } = null!;

    // Relationships
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; } = null!;
}