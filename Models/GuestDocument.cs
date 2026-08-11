namespace Staybnb.Models;

public class GuestDocument
{
    public int Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentUrl { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int GuestCheckInId { get; set; }

    // Relationships
    public GuestCheckIn GuestCheckIn { get; set; } = null!;
}
