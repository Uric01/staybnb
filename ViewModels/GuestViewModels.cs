using Staybnb.Models;

namespace Staybnb.ViewModels;

public class PropertyBrowseViewModel
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
    public string HostName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string? ThumbnailUrl { get; set; }
}

public class PropertyDetailViewModel
{
    public int Id { get; set; }
    public string HostId  { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PricePerNight { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal ServiceFee { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PropertyType { get; set; } = string.Empty;
    public int MaxGuests { get; set; }
    public string HostName { get; set; } = string.Empty;
    public string? HostEmail { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<string> Amenities { get; set; } = new();
    public List<ReviewViewModel> Reviews { get; set; } = new();
}

public class ReviewViewModel
{
    public string GuestName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateBookingViewModel
{
    public int PropertyId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public int MaxGuests { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal CleaningFee { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal TotalPrice { get; set; }
    public int NightsCount { get; set; }
}

public class GuestDashboardViewModel
{
    public int TotalBookings { get; set; }
    public int UpcomingBookings { get; set; }
    public int CompletedBookings { get; set; }
    public List<BookingListItemViewModel> RecentBookings { get; set; } = new();
}

public class BookingListItemViewModel
{
    public int Id { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public string HostId { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public CheckInStatus? CheckInStatus { get; set; }
    public string? PropertyThumbnail { get; set; }
}

public class GuestCheckInViewModel
{
    public int BookingId { get; set; }
    public string PropertyTitle { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public string CheckInRules { get; set; } = string.Empty;
    public string RequiredDocuments { get; set; } = string.Empty;
    public List<DocumentUploadViewModel> UploadedDocuments { get; set; } = new();
    public CheckInStatus Status { get; set; }
}

public class DocumentUploadViewModel
{
    public int Id { get; set; }
    public string DocumentUrl { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public DateTime UploadedAt { get; set; }
}

public class PropertyFilterViewModel
{
    public string? City { get; set; }
    public string? PropertyType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<PropertyBrowseViewModel> Properties { get; set; } = new();
    public List<string> AvailableCities { get; set; } = new();
    public List<string> AvailablePropertyTypes { get; set; } = new();
}
