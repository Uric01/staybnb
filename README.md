# Staybnb

Staybnb is a vacation rental web application built with ASP.NET Core 9.0, Entity Framework Core, and ASP.NET Identity. It provides a property booking experience similar to Airbnb, including user accounts, host property management, guest bookings, messaging, notifications, and admin management.

## Key Features

- ASP.NET Core MVC web application
- SQL Server / LocalDB database with Entity Framework Core
- ASP.NET Core Identity for authentication and role management
- Host property creation and management
- Guest browsing, booking, and check-in flow
- Messaging and notification support
- Admin dashboard and user management

## Project Structure

- `Controllers/` - MVC controllers for account, guest, host, admin, messaging, and notifications
- `Data/` - EF Core `ApplicationDbContext`, migrations, and seeders
- `Models/` - application domain models for users, bookings, properties, reviews, messages, and notifications
- `Services/` - business logic classes for booking, guest, host application, admin, and messaging
- `ViewModels/` - view model classes used by Razor views
- `Views/` - Razor pages and shared layout
- `wwwroot/` - static assets such as CSS, JavaScript, images, and uploads

## Prerequisites

- .NET SDK 9.0
- SQL Server or LocalDB
- Visual Studio, VS Code, or another compatible editor

## Setup

1. Clone the repository.
2. Open the solution folder in your editor.
3. Update the connection string in `appsettings.json` if needed.

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=StaybnbDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

4. Restore packages and build the project.

```bash
dotnet restore
dotnet build
```

5. Apply migrations to create the database.

```bash
dotnet ef database update
```

6. Run the application.

```bash
dotnet run
```

7. Open the browser at `https://localhost:5001` or the URL shown in the terminal.

## Notes

- The application seeds default roles and sample data at startup using `RoleSeeder` and `DataSeeder`.
- If you need a different database provider, update `ApplicationDbContext` and the connection string accordingly.

## Contact

For questions or troubleshooting, review the source files in `Controllers/`, `Services/`, and `Data/` for the application flow.
