using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Controllers;

[Authorize(Roles = "Guest")]
public class HostApplicationController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHostApplicationService _hostService;

    public HostApplicationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        IHostApplicationService hostService)
    {
        _context = context;
        _userManager = userManager;
        _hostService = hostService;
    }

    public async Task<IActionResult> Apply()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        // Check if guest already has properties without approved applications
        var existingProperty = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.HostId == userId);

        if (existingProperty == null)
        {
            return RedirectToAction("CreateProperty", "Host");
        }

        var applications = await _hostService.GetUserApplicationsAsync(userId);
        
        if (applications.Any(a => a.Status == ApplicationStatus.Pending))
        {
            TempData["InfoMessage"] = "You already have a pending application. Please wait for it to be reviewed.";
            return RedirectToAction("Dashboard", "Guest");
        }

        if (applications.Any(a => a.Status == ApplicationStatus.Approved))
        {
            TempData["InfoMessage"] = "Your application has been approved! You are now a Host.";
            return RedirectToAction("Dashboard", "Host");
        }

        var model = new CreateHostApplicationViewModel
        {
            Title = existingProperty.Title,
            Description = existingProperty.Description,
            PricePerNight = existingProperty.PricePerNight,
            CleaningFee = existingProperty.CleaningFee,
            ServiceFee = existingProperty.ServiceFee,
            Address = existingProperty.Address,
            City = existingProperty.City,
            PropertyType = existingProperty.PropertyType,
            MaxGuests = existingProperty.MaxGuests
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int propertyId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var property = await _context.HostProperties
            .Include(p => p.PropertyImages)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == userId);

        if (property == null)
        {
            TempData["ErrorMessage"] = "Property not found.";
            return RedirectToAction("Apply");
        }

        if (!property.PropertyImages.Any())
        {
            TempData["ErrorMessage"] = "Your property must have at least one image before submitting an application.";
            return RedirectToAction("Apply");
        }

        try
        {
            var application = await _hostService.SubmitApplicationAsync(userId, propertyId);
            TempData["SuccessMessage"] = "Host application submitted successfully! An admin will review it shortly.";
            return RedirectToAction("MyApplications");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error submitting application: {ex.Message}";
            return RedirectToAction("Apply");
        }
    }

    public async Task<IActionResult> MyApplications()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var applications = await _hostService.GetUserApplicationsAsync(userId);
        return View(applications);
    }
}
