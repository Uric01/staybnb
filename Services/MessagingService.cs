using Staybnb.Data;
using Staybnb.Models;
using Staybnb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Services;

public interface IMessagingService
{
    Task<MessageListViewModel> GetUserMessagesAsync(string userId);
    Task<MessageDetailViewModel?> GetConversationAsync(string userId, string otherUserId);
    Task<Message> SendMessageAsync(string senderId, string recipientId, string content);
    Task MarkMessageAsReadAsync(int messageId);
    Task MarkConversationAsReadAsync(string userId, string otherUserId);
    Task<NotificationCenterViewModel> GetNotificationsAsync(string userId);
    Task<int> GetUnreadNotificationCountAsync(string userId);
    Task MarkNotificationAsReadAsync(int notificationId);
    Task CreateNotificationAsync(string userId, string title, string message, NotificationType type, string? relatedUrl = null);
}

public class MessagingService : IMessagingService
{
    private readonly ApplicationDbContext _context;

    public MessagingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MessageListViewModel> GetUserMessagesAsync(string userId)
    {
        // Get unique conversations
        var conversations = await _context.Messages
            .Where(m => m.SenderId == userId || m.ReceiverId == userId)
            .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
            .Select(g => new
            {
                OtherUserId = g.Key,
                LastMessage = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()!.Content,
                LastMessageTime = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()!.Timestamp,
                UnreadCount = g.Where(m => m.ReceiverId == userId && !m.IsRead).Count()
            })
            .ToListAsync();

        var userIds = conversations.Select(c => c.OtherUserId).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
            .ToListAsync();

        var conversationVMs = conversations.Select(c =>
        {
            var otherUser = users.First(u => u.Id == c.OtherUserId);
            return new MessageConversationViewModel
            {
                ConversationId = c.OtherUserId.GetHashCode(),
                OtherUserId = c.OtherUserId,
                OtherUserName = $"{otherUser.FirstName} {otherUser.LastName}",
                OtherUserEmail = otherUser.Email ?? "",
                LastMessage = c.LastMessage,
                LastMessageTime = c.LastMessageTime,
                UnreadCount = c.UnreadCount
            };
        }).OrderByDescending(c => c.LastMessageTime).ToList();

        var unreadCount = await _context.Messages
            .Where(m => m.ReceiverId == userId && !m.IsRead)
            .CountAsync();

        return new MessageListViewModel
        {
            Conversations = conversationVMs,
            UnreadCount = unreadCount
        };
    }

    public async Task<MessageDetailViewModel?> GetConversationAsync(string userId, string otherUserId)
{
    // Get all messages between these two users
    var messages = await _context.Messages
        .Include(m => m.Sender)
        .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                    (m.SenderId == otherUserId && m.ReceiverId == userId))
        .OrderBy(m => m.Timestamp)
        .ToListAsync();

    // Get other user info even if no messages exist
    var otherUser = await _context.Users
        .Where(u => u.Id == otherUserId)
        .Select(u => new { u.Id, u.Email, u.FirstName, u.LastName })
        .FirstOrDefaultAsync();

    if (otherUser == null)
        return null;  // invalid user ID

    var messageVMs = messages.Select(m => new MessageItemViewModel
    {
        Id = m.Id,
        SenderId = m.SenderId,
        SenderName = m.Sender.FirstName + " " + m.Sender.LastName,
        Content = m.Content,
        SentAt = m.Timestamp,
        IsRead = m.IsRead,
        IsFromCurrentUser = m.SenderId == userId
    }).ToList();

    // Mark unread messages as read (if any)
    var unreadMessages = messages.Where(m => m.ReceiverId == userId && !m.IsRead).ToList();
    foreach (var msg in unreadMessages)
    {
        msg.IsRead = true;
    }
    if (unreadMessages.Any())
    {
        await _context.SaveChangesAsync();
    }

    // Always return a valid view model (messages may be empty)
    return new MessageDetailViewModel
    {
        OtherUserId = otherUserId,
        OtherUserName = $"{otherUser.FirstName} {otherUser.LastName}",
        OtherUserEmail = otherUser.Email ?? "",
        Messages = messageVMs,
        NewMessageContent = ""
    };
}

    public async Task<Message> SendMessageAsync(string senderId, string recipientId, string content)
    {
        var message = new Message
        {
            SenderId = senderId,
            ReceiverId = recipientId,
            Content = content,
            Timestamp = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(message);

        // Create notification for recipient
        var sender = await _context.Users
            .Where(u => u.Id == senderId)
            .Select(u => new { u.FirstName, u.LastName })
            .FirstOrDefaultAsync();

        if (sender != null)
        {
            var notification = new Notification
            {
                UserId = recipientId,
                Title = "New Message",
                Message = $"You have a new message from {sender.FirstName} {sender.LastName}",
                Type = NotificationType.NewMessage,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();
        return message;
    }

    public async Task MarkMessageAsReadAsync(int messageId)
    {
        var message = await _context.Messages.FindAsync(messageId);
        if (message != null)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkConversationAsReadAsync(string userId, string otherUserId)
    {
        var messages = await _context.Messages
            .Where(m => m.ReceiverId == userId && m.SenderId == otherUserId && !m.IsRead)
            .ToListAsync();

        foreach (var msg in messages)
        {
            msg.IsRead = true;
        }

        if (messages.Any())
        {
            await _context.SaveChangesAsync();
        }
    }

    public async Task<NotificationCenterViewModel> GetNotificationsAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .Select(n => new NotificationItemViewModel
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToListAsync();

        var unreadCount = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();

        return new NotificationCenterViewModel
        {
            Notifications = notifications,
            UnreadCount = unreadCount
        };
    }

    public async Task<int> GetUnreadNotificationCountAsync(string userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task MarkNotificationAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task CreateNotificationAsync(string userId, string title, string message, NotificationType type, string? relatedUrl = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}
