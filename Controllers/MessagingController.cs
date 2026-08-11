using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;

namespace Staybnb.Controllers;

[Authorize]
public class MessagingController : Controller
{
    private readonly IMessagingService _messagingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public MessagingController(IMessagingService messagingService, UserManager<ApplicationUser> userManager)
    {
        _messagingService = messagingService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Inbox()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var model = await _messagingService.GetUserMessagesAsync(userId);
        return View(model);
    }

    public async Task<IActionResult> Conversation(string otherUserId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var model = await _messagingService.GetConversationAsync(userId, otherUserId);
        if (model == null)
            return NotFound();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage(string recipientId, string content)
    {
        var senderId = _userManager.GetUserId(User);
        if (senderId == null)
            return Unauthorized();

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Message cannot be empty.";
            return RedirectToAction(nameof(Conversation), new { otherUserId = recipientId });
        }

        await _messagingService.SendMessageAsync(senderId, recipientId, content.Trim());
        
        TempData["SuccessMessage"] = "Message sent.";
        return RedirectToAction(nameof(Conversation), new { otherUserId = recipientId });
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead(int messageId, string? returnUrl)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        await _messagingService.MarkMessageAsReadAsync(messageId);
        
        var redirectUrl = returnUrl ?? Url.Action(nameof(Inbox)) ?? "/";
        return Redirect(redirectUrl);
    }
}
