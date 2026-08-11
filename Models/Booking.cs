namespace Staybnb.Models;

public class Booking
{
    public int Id { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int PropertyId { get; set; }
    public string GuestId { get; set; } = null!;

    // Relationships
    public HostProperty Property { get; set; } = null!;
    public ApplicationUser Guest { get; set; } = null!;
    public Payment? Payment { get; set; }
    public GuestCheckIn? GuestCheckIn { get; set; }
}
