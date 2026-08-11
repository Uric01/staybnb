using Staybnb.Models;

namespace Staybnb.ViewModels;

public class MessageListViewModel
{
    public List<MessageConversationViewModel> Conversations { get; set; } = new();
    public int UnreadCount { get; set; }
}

public class MessageConversationViewModel
{
    public int ConversationId { get; set; }
    public string OtherUserId { get; set; } = string.Empty;
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserEmail { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
    public bool IsUnread => UnreadCount > 0;
}

public class MessageDetailViewModel
{
    public string OtherUserId { get; set; } = string.Empty;
    public string OtherUserName { get; set; } = string.Empty;
    public string OtherUserEmail { get; set; } = string.Empty;
    public List<MessageItemViewModel> Messages { get; set; } = new();
    public string NewMessageContent { get; set; } = string.Empty;
}

public class MessageItemViewModel
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsFromCurrentUser { get; set; }
}

public class CreateMessageViewModel
{
    public string RecipientId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class NotificationCenterViewModel
{
    public List<NotificationItemViewModel> Notifications { get; set; } = new();
    public int UnreadCount { get; set; }
}

public class NotificationItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? RelatedUrl { get; set; }
}
