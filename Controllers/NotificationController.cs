using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Models;
using Staybnb.Services;

namespace Staybnb.Controllers;

[Authorize]
public class NotificationController : Controller
{
    private readonly IMessagingService _messagingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationController(IMessagingService messagingService, UserManager<ApplicationUser> userManager)
    {
        _messagingService = messagingService;
        _userManager = userManager;
    }

    public async Task<IActionResult> NotificationCenter()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var model = await _messagingService.GetNotificationsAsync(userId);
        //await _messagingService.MarkNotificationAsReadAsync(notificationId);
        await _messagingService.GetUnreadNotificationCountAsync(userId);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> MarkAsRead([FromBody] int notificationId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        await _messagingService.MarkNotificationAsReadAsync(notificationId);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var count = await _messagingService.GetUnreadNotificationCountAsync(userId);
        return Json(new { unreadCount = count });
    }
}
