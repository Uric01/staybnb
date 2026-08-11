namespace Staybnb.Models;

public class GuestCheckIn
{
    public int Id { get; set; }
    public CheckInStatus Status { get; set; } = CheckInStatus.NotStarted;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Foreign Keys
    public int BookingId { get; set; }
    public int? CheckInProcessId { get; set; }

    // Relationships
    public Booking Booking { get; set; } = null!;
    public CheckInProcess? CheckInProcess { get; set; }
    public ICollection<GuestDocument> GuestDocuments { get; set; } = new List<GuestDocument>();
}
