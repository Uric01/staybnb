namespace Staybnb.Models;

public class PropertyImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int PropertyId { get; set; }

    // Relationships
    public HostProperty Property { get; set; } = null!;
}
