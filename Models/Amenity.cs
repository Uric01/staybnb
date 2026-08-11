namespace Staybnb.Models;

public class Amenity
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string IconClass { get; set; } = null!;

    // Relationships
    public ICollection<HostProperty> Properties { get; set; } = new List<HostProperty>();
}
