namespace Staybnb.Models;

public class CheckInProcess
{
    public int Id { get; set; }
    public string Rules { get; set; } = string.Empty;
    public string RequiredDocuments { get; set; } = string.Empty;
    public CheckInStatus Status { get; set; } = CheckInStatus.NotStarted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int PropertyId { get; set; }

    // Relationships
    public HostProperty Property { get; set; } = null!;
}
