using Staybnb.Models;

namespace Staybnb.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalHosts { get; set; }
    public int TotalGuests { get; set; }
    public int TotalAdmins { get; set; }
    public int PendingApplications { get; set; }
    public int ApprovedApplications { get; set; }
    public int RejectedApplications { get; set; }
    public int TotalProperties { get; set; }
    public int TotalBookings { get; set; }
    public List<HostApplicationListItemViewModel> RecentApplications { get; set; } = new();
    public List<ActivityLogViewModel> RecentActivityLogs { get; set; } = new();
}

public class HostApplicationListItemViewModel
{
    public int Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string PropertyTitle { get; set; } = string.Empty;
    public string PropertyCity { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class HostApplicationDetailViewModel
{
    public int Id { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string ApplicantEmail { get; set; } = string.Empty;
    public string PropertyTitle { get; set; } = string.Empty;
    public string PropertyDescription { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public int MaxGuests { get; set; }
    public decimal PricePerNight { get; set; }
    public List<string> PropertyImages { get; set; } = new();
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedAt { get; set; }
    public string ApprovalNotes { get; set; } = string.Empty;
}

public class ActivityLogViewModel
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public ActivityType ActionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class UserManagementViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class AdminApplicationsListViewModel
{
    public List<HostApplicationListItemViewModel> PendingApplications { get; set; } = new();
    public List<HostApplicationListItemViewModel> ApprovedApplications { get; set; } = new();
    public List<HostApplicationListItemViewModel> RejectedApplications { get; set; } = new();
}

public class AdminAuditLogViewModel
{
    public List<ActivityLogViewModel> ActivityLogs { get; set; } = new();
    public string? FilterUser { get; set; }
    public ActivityType? FilterActionType { get; set; }
    public DateTime? FilterFromDate { get; set; }
    public DateTime? FilterToDate { get; set; }
}
