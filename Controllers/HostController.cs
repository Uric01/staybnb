using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Controllers;

// 1. We opened the main gate to both Roles
[Authorize(Roles = "Host,Guest")] 
public class HostController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IHostApplicationService _hostService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HostController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, 
        IHostApplicationService hostService, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _hostService = hostService;
        _webHostEnvironment = webHostEnvironment;
    }

    // 2. But we locked the specific rooms strictly for Hosts!
    [Authorize(Roles = "Host")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var properties = await _context.HostProperties
            .Where(p => p.HostId == userId)
            .Include(p => p.PropertyImages)
            .ToListAsync();

        var pendingBookings = await _context.Bookings
            .Where(b => b.Property.HostId == userId && b.Status == BookingStatus.Pending)
            .CountAsync();

        var totalEarnings = await _context.Payments
            .Where(p => p.Booking.Property.HostId == userId && p.Status == PaymentStatus.Completed)
            .SumAsync(p => p.Amount);

        var model = new HostDashboardViewModel
        {
            TotalProperties = properties.Count,
            ActiveProperties = properties.Count(p => p.IsActive),
            PendingBookings = pendingBookings,
            TotalEarnings = totalEarnings,
            Properties = properties.Select(p => new PropertyListItemViewModel
            {
                Id = p.Id,
                Title = p.Title,
                City = p.City,
                PricePerNight = p.PricePerNight,
                IsActive = p.IsActive,
                BookingCount = _context.Bookings.Count(b => b.PropertyId == p.Id),
                PendingBookingsCount = _context.Bookings.Count(b => b.PropertyId == p.Id && b.Status == BookingStatus.Pending),
                ThumbnailUrl = p.PropertyImages.FirstOrDefault()?.ImageUrl
            }).ToList()
        };

        return View(model);
    }

    [Authorize(Roles = "Host")]
    public async Task<IActionResult> Properties()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var properties = await _context.HostProperties
            .Where(p => p.HostId == userId)
            .Include(p => p.PropertyImages)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var model = properties.Select(p => new PropertyListItemViewModel
        {
            Id = p.Id,
            Title = p.Title,
            City = p.City,
            PricePerNight = p.PricePerNight,
            IsActive = p.IsActive,
            BookingCount = _context.Bookings.Count(b => b.PropertyId == p.Id),
            PendingBookingsCount = _context.Bookings.Count(b => b.PropertyId == p.Id && b.Status == BookingStatus.Pending),
            ThumbnailUrl = p.PropertyImages.FirstOrDefault()?.ImageUrl
        }).ToList();

        return View(model);
    }

    // 3. Notice we DO NOT lock CreateProperty. Both Guests and Hosts can use this!
    public IActionResult CreateProperty()
    {
        return View(new CreateHostApplicationViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProperty(CreateHostApplicationViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        if (!ModelState.IsValid)
        {
            model.ValidationMessage = "Please fill in all required fields.";
            return View(model);
        }

        if (model.PropertyImages == null || model.PropertyImages.Count == 0)
        {
            model.ValidationMessage = "At least one property image is required.";
            return View(model);
        }

        try
        {
            var property = new HostProperty
            {
                Title = model.Title,
                Description = model.Description,
                PricePerNight = model.PricePerNight,
                CleaningFee = model.CleaningFee,
                ServiceFee = model.ServiceFee,
                Address = model.Address,
                City = model.City,
                PropertyType = model.PropertyType,
                MaxGuests = model.MaxGuests,
                HostId = userId,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdProperty = await _hostService.CreatePropertyAsync(property);

            foreach (var imageFile in model.PropertyImages)
            {
                if (imageFile.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                    var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "properties");
                    
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    var imageUrl = $"/uploads/properties/{fileName}";
                    await _hostService.SavePropertyImageAsync(createdProperty.Id, imageUrl);
                }
            }

            var activityLog = new ActivityLog
            {
                UserId = userId,
                ActivityType = ActivityType.PropertyCreated,
                Action = $"Created property: {property.Title}",
                CreatedAt = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(activityLog);

            //NEW: Create the host application record
            var hostApplication = new HostApplication
            {
                ApplicationUserId = userId,
                PropertyId = createdProperty.Id,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.UtcNow
            };
            _context.HostApplications.Add(hostApplication);
            
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Property created successfully. It's currently inactive. Activate it once you submit your host application.";
            
            // Redirect differently depending on who just created the property
            if (User.IsInRole("Host"))
            {
                 return RedirectToAction(nameof(Dashboard));
            }
            
            // If they are a Guest, they don't have a Dashboard yet, send them to their bookings!
            return RedirectToAction("MyBookings", "Guest"); 
        }
        catch (Exception ex)
        {
            var fullMessage = ex.Message;
            var inner = ex.InnerException;
            while (inner != null)
            {
                fullMessage += " | Inner: " + inner.Message;
                inner = inner.InnerException;
            }
            model.ValidationMessage = $"Error creating property: {fullMessage}";
            return View(model);
        }
    }

    [Authorize(Roles = "Host")]
    public async Task<IActionResult> EditProperty(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var property = await _context.HostProperties
            .Include(p => p.PropertyImages)
            .FirstOrDefaultAsync(p => p.Id == id && p.HostId == userId);

        if (property == null)
            return NotFound();

        var model = new ManagePropertyViewModel
        {
            Id = property.Id,
            Title = property.Title,
            Description = property.Description,
            PricePerNight = property.PricePerNight,
            CleaningFee = property.CleaningFee,
            ServiceFee = property.ServiceFee,
            Address = property.Address,
            City = property.City,
            PropertyType = property.PropertyType,
            MaxGuests = property.MaxGuests,
            IsActive = property.IsActive,
            PropertyImages = property.PropertyImages.Select(pi => new PropertyImageViewModel
            {
                Id = pi.Id,
                ImageUrl = pi.ImageUrl,
                PropertyId = pi.PropertyId
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Host")]
    public async Task<IActionResult> EditProperty(int id, ManagePropertyViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == id && p.HostId == userId);

        if (property == null)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        property.Title = model.Title;
        property.Description = model.Description;
        property.PricePerNight = model.PricePerNight;
        property.CleaningFee = model.CleaningFee;
        property.ServiceFee = model.ServiceFee;
        property.Address = model.Address;
        property.City = model.City;
        property.PropertyType = model.PropertyType;
        property.MaxGuests = model.MaxGuests;
        property.UpdatedAt = DateTime.UtcNow;

        await _hostService.UpdatePropertyAsync(property);

        var activityLog = new ActivityLog
        {
            UserId = userId,
            ActivityType = ActivityType.PropertyUpdated,
            Action = $"Updated property: {property.Title}",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Property updated successfully.";
        return RedirectToAction(nameof(Properties));
    }

    [HttpPost]
    [Authorize(Roles = "Host")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == id && p.HostId == userId);

        if (property == null)
            return NotFound();

        property.IsActive = !property.IsActive;
        await _hostService.UpdatePropertyAsync(property);

        return RedirectToAction(nameof(Properties));
    }

    [Authorize(Roles = "Host")]
    public async Task<IActionResult> Bookings()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var bookings = await _context.Bookings
            .Where(b => b.Property.HostId == userId)
            .Include(b => b.Guest)
            .Include(b => b.Property)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        var model = bookings.Select(b => new BookingApprovalViewModel
        {
            Id = b.Id,
            GuestName = $"{b.Guest.FirstName} {b.Guest.LastName}",
            GuestId  = b.GuestId,
            PropertyTitle = b.Property.Title,
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            NumberOfGuests = b.NumberOfGuests,
            TotalPrice = b.TotalPrice,
            Status = b.Status,
            GuestEmail = b.Guest.Email ?? ""
        }).ToList();

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Host")]
    public async Task<IActionResult> ApproveBooking(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var booking = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == id && b.Property.HostId == userId);

        if (booking == null)
            return NotFound();

        booking.Status = BookingStatus.Approved;
        _context.Bookings.Update(booking);

        var notification = new Notification
        {
            UserId = booking.GuestId,
            Type = NotificationType.BookingApproved,
            Message = $"Your booking for {booking.Property.Title} has been approved!",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _context.Notifications.Add(notification);

        var activityLog = new ActivityLog
        {
            UserId = userId,
            ActivityType = ActivityType.BookingApproved,
            Action = $"Approved booking {booking.Id} for guest {booking.Guest.Email}",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Booking approved.";
        return RedirectToAction(nameof(Bookings));
    }

    [HttpPost]
    [Authorize(Roles = "Host")]
    public async Task<IActionResult> RejectBooking(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var booking = await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .FirstOrDefaultAsync(b => b.Id == id && b.Property.HostId == userId);

        if (booking == null)
            return NotFound();

        booking.Status = BookingStatus.Rejected;
        _context.Bookings.Update(booking);

        var notification = new Notification
        {
            UserId = booking.GuestId,
            Type = NotificationType.BookingRejected,
            Message = $"Your booking for {booking.Property.Title} has been rejected.",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _context.Notifications.Add(notification);

        var activityLog = new ActivityLog
        {
            UserId = userId,
            ActivityType = ActivityType.BookingRejected,
            Action = $"Rejected booking {booking.Id} for guest {booking.Guest.Email}",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Booking rejected.";
        return RedirectToAction(nameof(Bookings));
    }

    [HttpPost]
    [Authorize(Roles = "Host")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var image = await _context.PropertyImages
            .Include(pi => pi.Property)
            .FirstOrDefaultAsync(pi => pi.Id == imageId);

        if (image == null)
            return NotFound();

        if (image.Property.HostId != userId)
            return Forbid();

        var propertyId = image.PropertyId;
        
        try
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
        catch { }

        await _hostService.DeletePropertyImageAsync(imageId);

        return RedirectToAction(nameof(EditProperty), new { id = propertyId });
    }

    [Authorize(Roles = "Host")]
    public async Task<IActionResult> ConfigureCheckIn(int propertyId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var property = await _context.HostProperties
            .Include(p => p.CheckInProcess)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == userId);

        if (property == null)
            return NotFound();

        var model = new CheckInProcessViewModel
        {
            PropertyId = propertyId,
            Rules = property.CheckInProcess?.Rules ?? "",
            RequiredDocuments = property.CheckInProcess?.RequiredDocuments ?? ""
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Host")]
    public async Task<IActionResult> ConfigureCheckIn(int propertyId, CheckInProcessViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var property = await _context.HostProperties
            .Include(p => p.CheckInProcess)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == userId);

        if (property == null)
            return NotFound();

        if (property.CheckInProcess == null)
        {
            property.CheckInProcess = new CheckInProcess
            {
                PropertyId = propertyId,
                Rules = model.Rules,
                RequiredDocuments = model.RequiredDocuments,
                Status = CheckInStatus.NotStarted
            };
            _context.CheckInProcesses.Add(property.CheckInProcess);
        }
        else
        {
            property.CheckInProcess.Rules = model.Rules;
            property.CheckInProcess.RequiredDocuments = model.RequiredDocuments;
            _context.CheckInProcesses.Update(property.CheckInProcess);
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Check-in process configured successfully.";
        return RedirectToAction(nameof(Dashboard));
    }
}