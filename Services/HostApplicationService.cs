using Staybnb.Data;
using Staybnb.Models;
using Microsoft.EntityFrameworkCore;

namespace Staybnb.Services;

public interface IHostApplicationService
{
    Task<HostApplication> SubmitApplicationAsync(string userId, int propertyId);
    Task<HostApplication?> GetApplicationAsync(int id);
    Task<List<HostApplication>> GetUserApplicationsAsync(string userId);
    Task<HostProperty?> GetPropertyAsync(int id);
    Task<HostProperty> CreatePropertyAsync(HostProperty property);
    Task<HostProperty> UpdatePropertyAsync(HostProperty property);
    Task DeletePropertyAsync(int id);
    Task SavePropertyImageAsync(int propertyId, string imageUrl);
    Task DeletePropertyImageAsync(int imageId);
    Task<List<PropertyImage>> GetPropertyImagesAsync(int propertyId);
}

public class HostApplicationService : IHostApplicationService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public HostApplicationService(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<HostApplication> SubmitApplicationAsync(string userId, int propertyId)
    {
        var property = await _context.HostProperties
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.HostId == userId);

        if (property == null)
            throw new InvalidOperationException("Property not found or unauthorized.");

        var application = new HostApplication
        {
            ApplicationUserId = userId,
            PropertyId = propertyId,
            Status = ApplicationStatus.Pending,
            AppliedAt = DateTime.UtcNow
        };

        _context.HostApplications.Add(application);
        await _context.SaveChangesAsync();

        // Log activity
        var activityLog = new ActivityLog
        {
            UserId = userId,
            ActivityType = ActivityType.PropertyCreated,
            Action = "Created property...",         
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);
        await _context.SaveChangesAsync();

        return application;
    }

    public async Task<HostApplication?> GetApplicationAsync(int id)
    {
        return await _context.HostApplications
            .Include(ha => ha.Property)
            .Include(ha => ha.ApplicationUser)
            .FirstOrDefaultAsync(ha => ha.Id == id);
    }

    public async Task<List<HostApplication>> GetUserApplicationsAsync(string userId)
    {
        return await _context.HostApplications
            .Include(ha => ha.Property)
            .Where(ha => ha.ApplicationUserId == userId)
            .OrderByDescending(ha => ha.AppliedAt)
            .ToListAsync();
    }

    public async Task<HostProperty?> GetPropertyAsync(int id)
    {
        return await _context.HostProperties
            .Include(p => p.PropertyImages)
            .Include(p => p.CheckInProcess)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<HostProperty> CreatePropertyAsync(HostProperty property)
    {
        _context.HostProperties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<HostProperty> UpdatePropertyAsync(HostProperty property)
    {
        _context.HostProperties.Update(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task DeletePropertyAsync(int id)
    {
        var property = await _context.HostProperties.FindAsync(id);
        if (property != null)
        {
            _context.HostProperties.Remove(property);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SavePropertyImageAsync(int propertyId, string imageUrl)
    {
        var image = new PropertyImage
        {
            PropertyId = propertyId,
            ImageUrl = imageUrl
        };
        _context.PropertyImages.Add(image);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePropertyImageAsync(int imageId)
    {
        var image = await _context.PropertyImages.FindAsync(imageId);
        if (image != null)
        {
            _context.PropertyImages.Remove(image);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<PropertyImage>> GetPropertyImagesAsync(int propertyId)
    {
        return await _context.PropertyImages
            .Where(pi => pi.PropertyId == propertyId)
            .ToListAsync();
    }
}
