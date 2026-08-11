namespace Staybnb.Models;

public class WishlistItem
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public string UserId { get; set; } = null!;
    public int PropertyId { get; set; }

    // Relationships
    public ApplicationUser User { get; set; } = null!;
    public HostProperty Property { get; set; } = null!;
}
