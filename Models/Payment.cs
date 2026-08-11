namespace Staybnb.Models;

public class Payment
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int BookingId { get; set; }

    // Relationships
    public Booking Booking { get; set; } = null!;
}
