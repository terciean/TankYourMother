# WhereGass - Fuel Finder

WhereGass is a community-driven fuel finder application built with ASP.NET Core 8.0. It helps users find gas stations and check real-time fuel availability, queue times, and prices reported by other users.

## Features

- **Real-time Map**: View gas stations on an interactive map.
- **Community Reporting**: Quick, wizard-style reporting flow for fuel availability, type, queue times, and prices.
- **Trust System**: User karma based on report upvotes/downvotes. Automatic hiding of unreliable reports.
- **Admin Analytics**: System-wide statistics and top contributors dashboard.
- **PWA Support**: Installable on mobile devices with offline capabilities.
- **Security**: Google OAuth integration and rate limiting for reports/votes.

## Tech Stack

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: PostgreSQL (Supabase)
- **Frontend**: Bootstrap 5, Leaflet.js (Map), jQuery
- **Logging**: Serilog (Structured Logging)
- **Authentication**: ASP.NET Core Identity + Google OAuth

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- PostgreSQL database

### Configuration

Update `appsettings.json` with your credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD"
  },
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    }
  }
}
```

### Running Locally

1. Restore dependencies:
   ```bash
   dotnet restore
   ```
2. Apply migrations:
   ```bash
   dotnet ef database update
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

## Deployment

Refer to [DEPLOY_CHECKLIST.md](DEPLOY_CHECKLIST.md) for pre-deploy and post-deploy validation steps.

## Health Checks

The application provides a health check endpoint at `/health`.
