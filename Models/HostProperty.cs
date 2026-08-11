namespace Staybnb.Models;

public class HostProperty
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal PricePerNight { get; set; }
    public decimal CleaningFee { get; set; } = 0;
    public decimal ServiceFee { get; set; } = 0;
    public string Address { get; set; } = null!;
    public string City { get; set; } = null!;
    public string PropertyType { get; set; } = null!;
    public int MaxGuests { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Foreign Keys
    public string HostId { get; set; } = null!;

    // Relationships
    public ApplicationUser Host { get; set; } = null!;
    public ICollection<PropertyImage> PropertyImages { get; set; } = new List<PropertyImage>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public CheckInProcess? CheckInProcess { get; set; }
    public HostApplication? HostApplication { get; set; }
}
