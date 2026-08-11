using Staybnb.Models;

namespace Staybnb.ViewModels;

public class CreateHostApplicationViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public decimal CleaningFee { get; set; } = 0;
    public decimal ServiceFee { get; set; } = 0;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public int MaxGuests { get; set; }
    public List<IFormFile> PropertyImages { get; set; } = new();
    public string ValidationMessage { get; set; } = string.Empty;
}

public class PropertyImageViewModel
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int PropertyId { get; set; }
}

public class ManagePropertyViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal ServiceFee { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public int MaxGuests { get; set; }
    public bool IsActive { get; set; }
    public List<PropertyImageViewModel> PropertyImages { get; set; } = new();
}

public class HostDashboardViewModel
{
    public int TotalProperties { get; set; }
    public int ActiveProperties { get; set; }
    public int PendingBookings { get; set; }
    public decimal TotalEarnings { get; set; }
    public List<PropertyListItemViewModel> Properties { get; set; } = new();
}

public class PropertyListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; }
    public int BookingCount { get; set; }
    public int PendingBookingsCount { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class BookingApprovalViewModel
{
    public int Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestId  { get; set; } = string.Empty;
    public string PropertyTitle { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public string GuestEmail { get; set; } = string.Empty;
}

public class CheckInProcessViewModel
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string Rules { get; set; } = string.Empty;
    public string RequiredDocuments { get; set; } = string.Empty;
    public CheckInStatus Status { get; set; }
}
