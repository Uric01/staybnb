using Microsoft.AspNetCore.Identity;
using Staybnb.Data;
using Staybnb.Models;

namespace Staybnb.Data;

public class DataSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DataSeeder(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        // ---- Seed a default Host ----
        var hostEmail = "host@staybnb.com";
        var hostUser = await _userManager.FindByEmailAsync(hostEmail);
        if (hostUser == null)
        {
            hostUser = new ApplicationUser
            {
                UserName = hostEmail,
                Email = hostEmail,
                FirstName = "John",
                LastName = "Doe",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await _userManager.CreateAsync(hostUser, "Host@123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(hostUser, "Host");
            }
        }

        // ---- Seed sample properties if none exist ----
        if (!_context.HostProperties.Any())
        {
            var properties = new List<HostProperty>
            {
                new HostProperty
                {
                    HostId = hostUser.Id,
                    Title = "Cozy Cottage in Sandton",
                    Description = "A charming cottage with a beautiful garden, perfect for a weekend getaway.",
                    PricePerNight = 1200m,
                    CleaningFee = 200m,
                    ServiceFee = 100m,
                    Address = "12 Main Road, Sandton",
                    City = "Johannesburg",
                    PropertyType = "Cottage",
                    MaxGuests = 4,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PropertyImages = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "/images/placeholder.jpg" }
                    }
                },
                new HostProperty
                {
                    HostId = hostUser.Id,
                    Title = "Modern Apartment in Sea Point",
                    Description = "Stunning ocean views from this newly renovated 2-bedroom apartment.",
                    PricePerNight = 1800m,
                    CleaningFee = 300m,
                    ServiceFee = 150m,
                    Address = "5 Beach Road, Sea Point",
                    City = "Cape Town",
                    PropertyType = "Apartment",
                    MaxGuests = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PropertyImages = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "/images/placeholder.jpg" }
                    }
                },
                new HostProperty
                {
                    HostId = hostUser.Id,
                    Title = "Safari Lodge in Kruger",
                    Description = "Experience the Big Five from the comfort of a luxury safari lodge.",
                    PricePerNight = 3500m,
                    CleaningFee = 500m,
                    ServiceFee = 200m,
                    Address = "Kruger National Park, Skukuza",
                    City = "Mbombela",
                    PropertyType = "Lodge",
                    MaxGuests = 6,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PropertyImages = new List<PropertyImage>
                    {
                        new PropertyImage { ImageUrl = "/images/placeholder.jpg" }
                    }
                }
            };

            _context.HostProperties.AddRange(properties);
            await _context.SaveChangesAsync();
        }
    }
}