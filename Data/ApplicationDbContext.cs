using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Staybnb.Models;

namespace Staybnb.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets
    public DbSet<HostProperty> HostProperties { get; set; } = null!;
    public DbSet<PropertyImage> PropertyImages { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Review> Reviews { get; set; } = null!;
    public DbSet<Amenity> Amenities { get; set; } = null!;
    public DbSet<WishlistItem> WishlistItems { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;
    public DbSet<ActivityLog> ActivityLogs { get; set; } = null!;
    public DbSet<Notification> Notifications { get; set; } = null!;
    public DbSet<HostApplication> HostApplications { get; set; } = null!;
    public DbSet<CheckInProcess> CheckInProcesses { get; set; } = null!;
    public DbSet<GuestCheckIn> GuestCheckIns { get; set; } = null!;
    public DbSet<GuestDocument> GuestDocuments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser Configurations
        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.HostProperties)
            .WithOne(p => p.Host)
            .HasForeignKey(p => p.HostId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Bookings)
            .WithOne(b => b.Guest)
            .HasForeignKey(b => b.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Reviews)
            .WithOne(r => r.Reviewer)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.SentMessages)
            .WithOne(m => m.Sender)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.ReceivedMessages)
            .WithOne(m => m.Receiver)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.ActivityLogs)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // HostProperty Configurations
        modelBuilder.Entity<HostProperty>()
            .HasMany(p => p.PropertyImages)
            .WithOne(i => i.Property)
            .HasForeignKey(i => i.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HostProperty>()
            .HasMany(p => p.Bookings)
            .WithOne(b => b.Property)
            .HasForeignKey(b => b.PropertyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HostProperty>()
            .HasMany(p => p.Reviews)
            .WithOne(r => r.Property)
            .HasForeignKey(r => r.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HostProperty>()
            .HasMany(p => p.WishlistItems)
            .WithOne(w => w.Property)
            .HasForeignKey(w => w.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HostProperty>()
            .HasOne(p => p.CheckInProcess)
            .WithOne(c => c.Property)
            .HasForeignKey<CheckInProcess>(c => c.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HostProperty>()
            .HasOne(p => p.HostApplication)
            .WithOne(a => a.Property)
            .HasForeignKey<HostApplication>(a => a.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Booking Configurations
        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Payment)
            .WithOne(p => p.Booking)
            .HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.GuestCheckIn)
            .WithOne(g => g.Booking)
            .HasForeignKey<GuestCheckIn>(g => g.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // GuestCheckIn Configurations
        modelBuilder.Entity<GuestCheckIn>()
            .HasMany(g => g.GuestDocuments)
            .WithOne(d => d.GuestCheckIn)
            .HasForeignKey(d => d.GuestCheckInId)
            .OnDelete(DeleteBehavior.Cascade);

        // Amenity Many-to-Many with HostProperty (implicit join table)
        modelBuilder.Entity<HostProperty>()
            .HasMany(p => p.Amenities)
            .WithMany(a => a.Properties)
            .UsingEntity("PropertyAmenities");

        // HostApplication Configurations
        modelBuilder.Entity<HostApplication>()
            .HasOne(a => a.ApplicationUser)
            .WithMany(u => u.HostApplications)
            .HasForeignKey(a => a.ApplicationUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
