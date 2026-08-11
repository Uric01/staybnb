using Staybnb.Data;
using Staybnb.Models;
using Staybnb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Services;

public interface IGuestService
{
    Task<List<PropertyBrowseViewModel>> GetActivePropertiesAsync(string? city = null, string? propertyType = null);
    Task<PropertyDetailViewModel?> GetPropertyDetailAsync(int propertyId);
    Task<Booking> CreateBookingAsync(int propertyId, string guestId, DateTime checkIn, DateTime checkOut, int guests);
    Task<List<BookingListItemViewModel>> GetGuestBookingsAsync(string guestId);
    Task<GuestCheckInViewModel?> GetCheckInDetailsAsync(int bookingId, string guestId);
    Task<GuestCheckIn> StartCheckInAsync(int bookingId);
    Task UploadGuestDocumentAsync(int checkInId, string documentUrl, string guestId);
    Task CompleteCheckInAsync(int bookingId);
    Task<List<string>> GetUniqueCitiesAsync();
    Task<List<string>> GetUniquePropertyTypesAsync();
}

public class GuestService : IGuestService
{
    private readonly ApplicationDbContext _context;

    public GuestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PropertyBrowseViewModel>> GetActivePropertiesAsync(string? city = null, string? propertyType = null)
    {
        var query = _context.HostProperties
            .Where(p => p.IsActive)
            .Include(p => p.PropertyImages)
            .Include(p => p.Host)
            .Include(p => p.Reviews)
            .AsQueryable();

        if (!string.IsNullOrEmpty(city))
            query = query.Where(p => p.City == city);

        if (!string.IsNullOrEmpty(propertyType))
            query = query.Where(p => p.PropertyType == propertyType);

        var properties = await query.ToListAsync();

        return properties.Select(p => new PropertyBrowseViewModel
        {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            PricePerNight = p.PricePerNight,
            CleaningFee = p.CleaningFee,
            ServiceFee = p.ServiceFee,
            Address = p.Address,
            City = p.City,
            PropertyType = p.PropertyType,
            MaxGuests = p.MaxGuests,
            HostName = $"{p.Host.FirstName} {p.Host.LastName}",
            AverageRating = p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
            ReviewCount = p.Reviews.Count,
            ThumbnailUrl = p.PropertyImages.FirstOrDefault()?.ImageUrl
        }).ToList();
    }

    public async Task<PropertyDetailViewModel?> GetPropertyDetailAsync(int propertyId)
    {
        var property = await _context.HostProperties
            .Include(p => p.PropertyImages)
            .Include(p => p.Host)
            .Include(p => p.Reviews).ThenInclude(r => r.Guest)
            .Include(p => p.Amenities)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.IsActive);

        if (property == null)
            return null;

        return new PropertyDetailViewModel
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
            HostName = $"{property.Host.FirstName} {property.Host.LastName}",
            HostEmail = property.Host.Email,
            HostId  = property.HostId,
            ImageUrls = property.PropertyImages.Select(pi => pi.ImageUrl).ToList(),
            Amenities = property.Amenities.Select(a => a.Name).ToList(),
            Reviews = property.Reviews.Select(r => new ReviewViewModel
            {
                GuestName = $"{r.Guest.FirstName} {r.Guest.LastName}",
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            }).ToList()
        };
    }

    public async Task<Booking> CreateBookingAsync(int propertyId, string guestId, DateTime checkIn, DateTime checkOut, int guests)
    {
        var property = await _context.HostProperties.FindAsync(propertyId);
        if (property == null)
            throw new InvalidOperationException("Property not found");

        var nights = (int)(checkOut - checkIn).TotalDays;
        var totalPrice = (property.PricePerNight * nights) + property.CleaningFee + property.ServiceFee;

        var booking = new Booking
        {
            PropertyId = propertyId,
            GuestId = guestId,
            CheckInDate = checkIn,
            CheckOutDate = checkOut,
            NumberOfGuests = guests,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Log activity
        var activityLog = new ActivityLog
        {
            UserId = guestId,
            ActivityType = ActivityType.BookingCreated,
            Action = "Created booking...",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        return booking;
    }

    public async Task<List<BookingListItemViewModel>> GetGuestBookingsAsync(string guestId)
    {
        var bookings = await _context.Bookings
            .Where(b => b.GuestId == guestId)
            .Include(b => b.Property).ThenInclude(p => p.PropertyImages)
            .Include(b => b.Property).ThenInclude(p => p.Host)
            .Include(b => b.GuestCheckIn)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(b => new BookingListItemViewModel
        {
            Id = b.Id,
            PropertyTitle = b.Property.Title,
            HostId = b.Property.HostId, 
            HostName = $"{b.Property.Host.FirstName} {b.Property.Host.LastName}",
            CheckInDate = b.CheckInDate,
            CheckOutDate = b.CheckOutDate,
            NumberOfGuests = b.NumberOfGuests,
            TotalPrice = b.TotalPrice,
            Status = b.Status,
            CheckInStatus = b.GuestCheckIn?.Status,
            PropertyThumbnail = b.Property.PropertyImages.FirstOrDefault()?.ImageUrl
        }).ToList();
    }

    public async Task<GuestCheckInViewModel?> GetCheckInDetailsAsync(int bookingId, string guestId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Property).ThenInclude(p => p.CheckInProcess)
            .Include(b => b.GuestCheckIn)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.GuestId == guestId);

        if (booking == null || booking.GuestCheckIn == null)
            return null;

        var documents = await _context.GuestDocuments
            .Where(gd => gd.GuestCheckInId == booking.GuestCheckIn.Id)
            .ToListAsync();

        return new GuestCheckInViewModel
        {
            BookingId = booking.Id,
            PropertyTitle = booking.Property.Title,
            CheckInDate = booking.CheckInDate,
            CheckInRules = booking.Property.CheckInProcess?.Rules ?? "",
            RequiredDocuments = booking.Property.CheckInProcess?.RequiredDocuments ?? "",
            Status = booking.GuestCheckIn.Status,
            UploadedDocuments = documents.Select(gd => new DocumentUploadViewModel
            {
                Id = gd.Id,
                DocumentUrl = gd.DocumentUrl,
                Status = gd.Status,
                UploadedAt = gd.UploadedAt
            }).ToList() ?? new()
        };
    }

    public async Task<GuestCheckIn> StartCheckInAsync(int bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
            throw new InvalidOperationException("Booking not found");

        var existingCheckIn = await _context.GuestCheckIns.FirstOrDefaultAsync(gc => gc.BookingId == bookingId);
        if (existingCheckIn != null)
            return existingCheckIn;

        var checkIn = new GuestCheckIn
        {
            BookingId = bookingId,
            Status = CheckInStatus.InProgress,
            StartedAt = DateTime.UtcNow
        };

        _context.GuestCheckIns.Add(checkIn);
        await _context.SaveChangesAsync();

        return checkIn;
    }

    public async Task UploadGuestDocumentAsync(int checkInId, string documentUrl, string guestId)
    {
        var checkIn = await _context.GuestCheckIns
            .Include(gc => gc.Booking)
            .FirstOrDefaultAsync(gc => gc.Id == checkInId && gc.Booking.GuestId == guestId);

        if (checkIn == null)
            throw new InvalidOperationException("Check-in not found");

        var document = new GuestDocument
        {
            GuestCheckInId = checkInId,
            DocumentUrl = documentUrl,
            Status = DocumentStatus.Pending,
            UploadedAt = DateTime.UtcNow
        };

        _context.GuestDocuments.Add(document);
        await _context.SaveChangesAsync();
    }

    public async Task CompleteCheckInAsync(int bookingId)
    {
        var checkIn = await _context.GuestCheckIns
            .FirstOrDefaultAsync(gc => gc.BookingId == bookingId);

        if (checkIn != null)
        {
            checkIn.Status = CheckInStatus.Completed;
            checkIn.CompletedAt = DateTime.UtcNow;
            _context.GuestCheckIns.Update(checkIn);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<string>> GetUniqueCitiesAsync()
    {
        return await _context.HostProperties
            .Where(p => p.IsActive)
            .Select(p => p.City)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<List<string>> GetUniquePropertyTypesAsync()
    {
        return await _context.HostProperties
            .Where(p => p.IsActive)
            .Select(p => p.PropertyType)
            .Distinct()
            .OrderBy(pt => pt)
            .ToListAsync();
    }
}
