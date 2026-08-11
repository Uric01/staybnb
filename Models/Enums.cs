namespace Staybnb.Models;

public enum BookingStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3,
    CheckedIn = 4,
    CheckedOut = 5
}

public enum ApplicationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3
}

public enum DocumentStatus
{
    Pending = 0,
    Verified = 1,
    Rejected = 2
}

public enum NotificationType
{
    BookingApproved = 0,
    BookingRejected = 1,
    NewMessage = 2,
    ApplicationApproved = 3,
    ApplicationRejected = 4,
    CheckInRequired = 5,
    PaymentCompleted = 6
}

public enum ActivityType
{
    UserLogin = 0,
    RoleChanged = 1,
    BookingCreated = 2,
    BookingApproved = 3,
    BookingRejected = 4,
    ApplicationSubmitted = 5,
    ApplicationApproved = 6,
    ApplicationRejected = 7,
    PropertyCreated = 8,
    PropertyUpdated = 9,
    DocumentUploaded = 10
}

public enum CheckInStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3
}
