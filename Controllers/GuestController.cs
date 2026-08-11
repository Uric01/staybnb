using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Staybnb.Data;
using Staybnb.Models;
using Staybnb.Services;
using Staybnb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Controllers;

// No class-level [Authorize] – we will protect only the methods that require the Guest role
public class GuestController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IGuestService _guestService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public GuestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        IGuestService guestService, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _userManager = userManager;
        _guestService = guestService;
        _webHostEnvironment = webHostEnvironment;
    }

    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var bookings = await _guestService.GetGuestBookingsAsync(userId);

        var model = new GuestDashboardViewModel
        {
            TotalBookings = bookings.Count,
            UpcomingBookings = bookings.Count(b => b.CheckInDate > DateTime.Now),
            CompletedBookings = bookings.Count(b => b.CheckOutDate < DateTime.Now),
            RecentBookings = bookings.Take(5).ToList()
        };

        return View(model);
    }

    // Anyone can browse, even unauthenticated visitors
    public async Task<IActionResult> Browse(string? city = null, string? propertyType = null)
    {
        var properties = await _guestService.GetActivePropertiesAsync(city, propertyType);
        var cities = await _guestService.GetUniqueCitiesAsync();
        var types = await _guestService.GetUniquePropertyTypesAsync();

        var model = new PropertyFilterViewModel
        {
            City = city,
            PropertyType = propertyType,
            Properties = properties,
            AvailableCities = cities,
            AvailablePropertyTypes = types
        };

        return View(model);
    }

    public async Task<IActionResult> PropertyDetail(int id)
    {
        var property = await _guestService.GetPropertyDetailAsync(id);
        if (property == null)
            return NotFound();

        return View(property);
    }

    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> Book(int propertyId)
    {
        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);

        if (property == null)
            return NotFound();

        var model = new CreateBookingViewModel
        {
            PropertyId = propertyId,
            PropertyTitle = property.Title,
            MaxGuests = property.MaxGuests,
            PricePerNight = property.PricePerNight,
            CleaningFee = property.CleaningFee,
            ServiceFee = property.ServiceFee,
            CheckInDate = DateTime.Now.AddDays(1),
            CheckOutDate = DateTime.Now.AddDays(2),
            NumberOfGuests = 1
        };

        CalculatePrice(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> SubmitBooking(CreateBookingViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        // Validate dates
        if (model.CheckInDate >= model.CheckOutDate)
        {
            ModelState.AddModelError("CheckOutDate", "Check-out date must be after check-in date");
            CalculatePrice(model);
            return View("Book", model);
        }

        if (model.NumberOfGuests > model.MaxGuests)
        {
            ModelState.AddModelError("NumberOfGuests", $"Maximum {model.MaxGuests} guests allowed");
            CalculatePrice(model);
            return View("Book", model);
        }

        try
        {
            var booking = await _guestService.CreateBookingAsync(
                model.PropertyId, userId, model.CheckInDate, model.CheckOutDate, model.NumberOfGuests);

            TempData["SuccessMessage"] = "Booking request submitted! The host will review it shortly.";
            return RedirectToAction(nameof(MyBookings));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Error creating booking: {ex.Message}");
            CalculatePrice(model);
            return View("Book", model);
        }
    }

    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> MyBookings()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var bookings = await _guestService.GetGuestBookingsAsync(userId);
        return View(bookings);
    }

    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> CheckIn(int bookingId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var checkInDetails = await _guestService.GetCheckInDetailsAsync(bookingId, userId);
        if (checkInDetails == null)
            return NotFound();

        if (checkInDetails.Status == CheckInStatus.NotStarted)
        {
            await _guestService.StartCheckInAsync(bookingId);
            checkInDetails.Status = CheckInStatus.InProgress;
        }

        return View(checkInDetails);
    }

    [HttpPost]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> UploadDocument(int checkInId, IFormFile documentFile)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        if (documentFile == null || documentFile.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a document to upload.";
            return RedirectToAction(nameof(CheckIn));
        }

        try
        {
            var fileName = $"{Guid.NewGuid()}_{documentFile.FileName}";
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "documents");

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var filePath = Path.Combine(uploadPath, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await documentFile.CopyToAsync(stream);
            }

            var documentUrl = $"/uploads/documents/{fileName}";
            await _guestService.UploadGuestDocumentAsync(checkInId, documentUrl, userId);

            TempData["SuccessMessage"] = "Document uploaded successfully. The host will verify it.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error uploading document: {ex.Message}";
        }

        var booking = await _context.GuestCheckIns
            .Where(gc => gc.Id == checkInId)
            .Select(gc => gc.BookingId)
            .FirstOrDefaultAsync();

        return RedirectToAction(nameof(CheckIn), new { bookingId = booking });
    }

    [HttpPost]
    [Authorize(Roles = "Guest")]
    public async Task<IActionResult> CompleteCheckIn(int bookingId)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        await _guestService.CompleteCheckInAsync(bookingId);
        TempData["SuccessMessage"] = "Check-in completed!";
        return RedirectToAction(nameof(MyBookings));
    }

    public IActionResult Wishlist()
    {
        return View();
    }

    private void CalculatePrice(CreateBookingViewModel model)
    {
        var nights = (int)(model.CheckOutDate - model.CheckInDate).TotalDays;
        if (nights < 1) nights = 1;
       
        model.NightsCount = nights;
        model.TotalPrice = (model.PricePerNight * nights) + model.CleaningFee + model.ServiceFee;
    }
}