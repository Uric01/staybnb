namespace Staybnb.Models;

public class HostApplication
{
    public int Id { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }

    // Foreign Keys
    public string ApplicationUserId { get; set; } = null!;
    public int PropertyId { get; set; }

    // Relationships
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public HostProperty Property { get; set; } = null!;
}
