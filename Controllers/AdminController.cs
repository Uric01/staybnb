using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;

namespace Staybnb.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(IAdminService adminService, UserManager<ApplicationUser> userManager)
    {
        _adminService = adminService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await _adminService.GetDashboardAsync();
        return View(model);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Applications()
    {
        var pending = await _adminService.GetPendingApplicationsAsync();
        return View(pending);
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ApplicationDetail(int id)
    {
        var app = await _adminService.GetApplicationDetailAsync(id);
        if (app == null)
            return NotFound();

        return View(app);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ApproveApplication(int id)
    {
        var adminId = _userManager.GetUserId(User);
        if (adminId == null)
            return Unauthorized();

        var user = await _adminService.ApproveApplicationAsync(id, adminId);
        if (user == null)
            return NotFound();

        TempData["SuccessMessage"] = $"Application approved. {user.Email} is now a Host.";
        return RedirectToAction(nameof(Applications));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> RejectApplication(int id, string reason)
    {
        var adminId = _userManager.GetUserId(User);
        if (adminId == null)
            return Unauthorized();

        await _adminService.RejectApplicationAsync(id, reason, adminId);
        TempData["SuccessMessage"] = "Application rejected.";
        return RedirectToAction(nameof(Applications));
    }

    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UserManagement()
    {
        var guests = await _adminService.GetAllGuestsAsync();
        return View(guests);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> PromoteToAdmin(string guestId)
    {
        var superAdminId = _userManager.GetUserId(User);
        if (superAdminId == null)
            return Unauthorized();

        await _adminService.PromoteGuestToAdminAsync(guestId, superAdminId);
        TempData["SuccessMessage"] = "Guest promoted to Admin role.";
        return RedirectToAction(nameof(UserManagement));
    }

    public async Task<IActionResult> ActivityLog(string? userId = null, ActivityType? actionType = null, string? fromDate = null, string? toDate = null)
    {
        DateTime? from = null, to = null;

        if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var fromDateTime))
            from = fromDateTime;

        if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var toDateTime))
            to = toDateTime;

        var logs = await _adminService.GetActivityLogsAsync(userId, actionType, from, to);

        var model = new AdminAuditLogViewModel
        {
            ActivityLogs = logs,
            FilterUser = userId,
            FilterActionType = actionType,
            FilterFromDate = from,
            FilterToDate = to
        };

        return View(model);
    }
}
