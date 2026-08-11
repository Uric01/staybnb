using Staybnb.Data;
using Staybnb.Models;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Services;

public interface IBookingService
{
    Task<Booking> CreateBookingAsync(Booking booking);
    Task<Booking?> GetBookingAsync(int id);
    Task<List<Booking>> GetUserBookingsAsync(string userId);
    Task<List<Booking>> GetHostBookingsAsync(string hostId);
    Task UpdateBookingStatusAsync(int bookingId, BookingStatus status);
}

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Booking> CreateBookingAsync(Booking booking)
    {
        booking.CreatedAt = DateTime.UtcNow;
        booking.Status = BookingStatus.Pending;
        
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        // Log activity
        var activityLog = new ActivityLog
        {
            UserId = booking.GuestId, 
            ActivityType = ActivityType.BookingCreated, 
            Action = "Created booking...",              
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        return booking;
    }

    public async Task<Booking?> GetBookingAsync(int id)
    {
        return await _context.Bookings
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .Include(b => b.Payment)
            .Include(b => b.GuestCheckIn)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> GetUserBookingsAsync(string userId)
    {
        return await _context.Bookings
            .Where(b => b.GuestId == userId)
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Booking>> GetHostBookingsAsync(string hostId)
    {
        return await _context.Bookings
            .Where(b => b.Property.HostId == hostId)
            .Include(b => b.Property)
            .Include(b => b.Guest)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task UpdateBookingStatusAsync(int bookingId, BookingStatus status)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking != null)
        {
            booking.Status = status;
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }
    }
}
