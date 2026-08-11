namespace Staybnb.Models;

public class Review
{
    public int Id { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign Keys
    public int PropertyId { get; set; }
    public string ReviewerId { get; set; } = null!;

    // Relationships
    public HostProperty Property { get; set; } = null!;
    public ApplicationUser Reviewer { get; set; } = null!;
    public ApplicationUser Guest { get; set; } = null!;
}
