namespace Staybnb.Models;

public class Message
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    // Foreign Keys
    public string SenderId { get; set; } = null!;
    public string ReceiverId { get; set; } = null!;

    // Relationships
    public ApplicationUser Sender { get; set; } = null!;
    public ApplicationUser Receiver { get; set; } = null!;
}
