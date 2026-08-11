using Staybnb.Data;
using Staybnb.Models;
using Staybnb.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Staybnb.Services;

public interface IAdminService
{
    Task<AdminDashboardViewModel> GetDashboardAsync();
    Task<List<HostApplicationListItemViewModel>> GetPendingApplicationsAsync();
    Task<HostApplicationDetailViewModel?> GetApplicationDetailAsync(int applicationId);
    Task<ApplicationUser?> ApproveApplicationAsync(int applicationId, string adminId);
    Task RejectApplicationAsync(int applicationId, string reason, string adminId);
    Task<List<ActivityLogViewModel>> GetActivityLogsAsync(string? userId = null, ActivityType? actionType = null, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<UserManagementViewModel>> GetAllGuestsAsync();
    Task PromoteGuestToAdminAsync(string guestId, string superAdminId);
}

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AdminDashboardViewModel> GetDashboardAsync()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var hosts = await _userManager.GetUsersInRoleAsync("Host");
        var guests = await _userManager.GetUsersInRoleAsync("Guest");
        var admins = await _userManager.GetUsersInRoleAsync("Admin");

        var pendingApps = await _context.HostApplications
            .Where(ha => ha.Status == ApplicationStatus.Pending)
            .CountAsync();

        var approvedApps = await _context.HostApplications
            .Where(ha => ha.Status == ApplicationStatus.Approved)
            .CountAsync();

        var rejectedApps = await _context.HostApplications
            .Where(ha => ha.Status == ApplicationStatus.Rejected)
            .CountAsync();

        var totalProperties = await _context.HostProperties.CountAsync();
        var totalBookings = await _context.Bookings.CountAsync();

        var recentApps = await _context.HostApplications
            .Include(ha => ha.ApplicationUser)
            .Include(ha => ha.Property)
            .OrderByDescending(ha => ha.AppliedAt)
            .Take(5)
            .Select(ha => new HostApplicationListItemViewModel
            {
                Id = ha.Id,
                ApplicantName = $"{ha.ApplicationUser.FirstName} {ha.ApplicationUser.LastName}",
                ApplicantEmail = ha.ApplicationUser.Email ?? "",
                PropertyTitle = ha.Property.Title,
                PropertyCity = ha.Property.City,
                Status = ha.Status,
                AppliedAt = ha.AppliedAt,
                ReviewedAt = ha.ReviewedAt
            })
            .ToListAsync();

        var recentLogs = await _context.ActivityLogs
            .Include(al => al.User)
            .OrderByDescending(al => al.CreatedAt)
            .Take(10)
            .Select(al => new ActivityLogViewModel
            {
                Id = al.Id,
                UserName = $"{al.User.FirstName} {al.User.LastName}",
                UserEmail = al.User.Email ?? "",
                ActionType = al.ActivityType,
                Description = al.Action,
                Timestamp = al.CreatedAt
            })
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            TotalHosts = hosts.Count,
            TotalGuests = guests.Count,
            TotalAdmins = admins.Count,
            PendingApplications = pendingApps,
            ApprovedApplications = approvedApps,
            RejectedApplications = rejectedApps,
            TotalProperties = totalProperties,
            TotalBookings = totalBookings,
            RecentApplications = recentApps,
            RecentActivityLogs = recentLogs
        };
    }

    public async Task<List<HostApplicationListItemViewModel>> GetPendingApplicationsAsync()
    {
        return await _context.HostApplications
            .Where(ha => ha.Status == ApplicationStatus.Pending)
            .Include(ha => ha.ApplicationUser)
            .Include(ha => ha.Property)
            .OrderByDescending(ha => ha.AppliedAt)
            .Select(ha => new HostApplicationListItemViewModel
            {
                Id = ha.Id,
                ApplicantName = $"{ha.ApplicationUser.FirstName} {ha.ApplicationUser.LastName}",
                ApplicantEmail = ha.ApplicationUser.Email ?? "",
                PropertyTitle = ha.Property.Title,
                PropertyCity = ha.Property.City,
                Status = ha.Status,
                AppliedAt = ha.AppliedAt,
                ReviewedAt = ha.ReviewedAt
            })
            .ToListAsync();
    }

    public async Task<HostApplicationDetailViewModel?> GetApplicationDetailAsync(int applicationId)
    {
        var app = await _context.HostApplications
            .Include(ha => ha.ApplicationUser)
            .Include(ha => ha.Property).ThenInclude(p => p.PropertyImages)
            .FirstOrDefaultAsync(ha => ha.Id == applicationId);

        if (app == null)
            return null;

        return new HostApplicationDetailViewModel
        {
            Id = app.Id,
            ApplicantName = $"{app.ApplicationUser.FirstName} {app.ApplicationUser.LastName}",
            ApplicantEmail = app.ApplicationUser.Email ?? "",
            PropertyTitle = app.Property.Title,
            PropertyDescription = app.Property.Description,
            PropertyType = app.Property.PropertyType,
            Address = app.Property.Address,
            City = app.Property.City,
            MaxGuests = app.Property.MaxGuests,
            PricePerNight = app.Property.PricePerNight,
            PropertyImages = app.Property.PropertyImages.Select(pi => pi.ImageUrl).ToList(),
            Status = app.Status,
            AppliedAt = app.AppliedAt
        };
    }

    public async Task<ApplicationUser?> ApproveApplicationAsync(int applicationId, string adminId)
    {
        var app = await _context.HostApplications
            .Include(ha => ha.ApplicationUser)
            .Include(ha => ha.Property)
            .FirstOrDefaultAsync(ha => ha.Id == applicationId);

        if (app == null)
            return null;

        app.Status = ApplicationStatus.Approved;
        app.ReviewedAt = DateTime.UtcNow;

        var user = app.ApplicationUser;

        // Add Host role
        if (!await _userManager.IsInRoleAsync(user, "Host"))
        {
            await _userManager.AddToRoleAsync(user, "Host");
        }

        // Remove Guest role
        if (await _userManager.IsInRoleAsync(user, "Guest"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Guest");
        }

        // Activate property
        app.Property.IsActive = true;

        _context.HostApplications.Update(app);
        _context.HostProperties.Update(app.Property);

        // Log activity
        var activityLog = new ActivityLog
        {
            UserId = adminId,
            ActivityType = ActivityType.ApplicationApproved,
            Action = $"Approved host application for {app.Property.Title} by {user.Email}",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        // Notify user
        var notification = new Notification
        {
            UserId = user.Id,
            Title = "Application Approved",
            Message = $"Your host application for '{app.Property.Title}' has been approved!",
            Type = NotificationType.ApplicationApproved,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task RejectApplicationAsync(int applicationId, string reason, string adminId)
    {
        var app = await _context.HostApplications
            .Include(ha => ha.ApplicationUser)
            .Include(ha => ha.Property)
            .FirstOrDefaultAsync(ha => ha.Id == applicationId);

        if (app == null)
            return;

        app.Status = ApplicationStatus.Rejected;
        app.ReviewedAt = DateTime.UtcNow;

        _context.HostApplications.Update(app);

        var activityLog = new ActivityLog
        {
            UserId = adminId,
            ActivityType = ActivityType.ApplicationRejected,
            Action = $"Rejected host application for {app.Property.Title}: {reason}",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        var notification = new Notification
        {
            UserId = app.ApplicationUser.Id,
            Title = "Application Rejected",
            Message = $"Your host application for '{app.Property.Title}' was not approved. Reason: {reason}",
            Type = NotificationType.ApplicationRejected,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
    }

    public async Task<List<ActivityLogViewModel>> GetActivityLogsAsync(string? userId = null, ActivityType? actionType = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.ActivityLogs
            .Include(al => al.User)
            .AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(al => al.UserId == userId);

        if (actionType.HasValue)
            query = query.Where(al => al.ActivityType == actionType.Value);

        if (fromDate.HasValue)
            query = query.Where(al => al.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(al => al.CreatedAt <= toDate.Value);

        return await query
            .OrderByDescending(al => al.CreatedAt)
            .Select(al => new ActivityLogViewModel
            {
                Id = al.Id,
                UserName = $"{al.User.FirstName} {al.User.LastName}",
                UserEmail = al.User.Email ?? "",
                ActionType = al.ActivityType,
                Description = al.Action,
                Timestamp = al.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<List<UserManagementViewModel>> GetAllGuestsAsync()
    {
        var guests = await _userManager.GetUsersInRoleAsync("Guest");
        var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");

        return guests.Select(g => new UserManagementViewModel
        {
            UserId = g.Id,
            UserName = g.UserName ?? "",
            Email = g.Email ?? "",
            FirstName = g.FirstName ?? "",
            LastName = g.LastName ?? "",
            Roles = new List<string> { "Guest" },
            CreatedAt = g.CreatedAt
        }).ToList();
    }

    public async Task PromoteGuestToAdminAsync(string guestId, string superAdminId)
    {
        var user = await _userManager.FindByIdAsync(guestId);
        if (user == null)
            return;

        // Add Admin role
        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        // Remove Guest role
        if (await _userManager.IsInRoleAsync(user, "Guest"))
        {
            await _userManager.RemoveFromRoleAsync(user, "Guest");
        }

        var activityLog = new ActivityLog
        {
            UserId = superAdminId,
            ActivityType = ActivityType.RoleChanged,
            Action = $"Promoted {user.Email} from Guest to Admin role",
            CreatedAt = DateTime.UtcNow
        };
        _context.ActivityLogs.Add(activityLog);

        var notification = new Notification
        {
            UserId = user.Id,
            Title = "Role Updated",
            Message = "You have been promoted to Admin! You now have access to the admin dashboard.",
            Type = NotificationType.ApplicationApproved,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };
        _context.Notifications.Add(notification);

        await _context.SaveChangesAsync();
    }
}