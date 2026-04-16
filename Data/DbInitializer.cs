using GasStationApp.Models;
using Microsoft.EntityFrameworkCore;

namespace GasStationApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            context.Database.EnsureCreated();

            // Create admin user if it doesn't exist
            var adminEmail = "admin@wheregass.com";
            var adminUser = context.Users.FirstOrDefault(u => u.UserName == adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser 
                { 
                    UserName = adminEmail, 
                    Email = adminEmail, 
                    EmailConfirmed = true,
                    Karma = 100
                };
                
                adminUser.PasswordHash = "AQAAAAIAAYagAAAAEG8vM+X0y+7Z2z+7z+7z+7z+7z+7z+7z+7z+7z+7z+7z+7z+7z+7z+7z+7z+7z=="; // Mock hash for 'Admin123!'
                context.Users.Add(adminUser);
                context.SaveChanges();
            }

            // Ensure Admin Role Claim exists
            var hasAdminClaim = context.UserClaims.Any(c => c.UserId == adminUser.Id && c.ClaimType == System.Security.Claims.ClaimTypes.Role && c.ClaimValue == "Admin");
            if (!hasAdminClaim)
            {
                context.UserClaims.Add(new Microsoft.AspNetCore.Identity.IdentityUserClaim<string>
                {
                    UserId = adminUser.Id,
                    ClaimType = System.Security.Claims.ClaimTypes.Role,
                    ClaimValue = "Admin"
                });
                context.SaveChanges();
            }

            // Look for any stations.
            if (context.Stations.Any())
            {
                return;   // DB has been seeded
            }

            var stations = new Station[]
            {
                new Station { name = "BP Sandton", brand = "BP", street_address = "123 Sandton Dr", suburb = "Sandton", lat = -26.1076, lng = 28.0567 },
                new Station { name = "Shell Rosebank", brand = "Shell", street_address = "456 Jan Smuts Ave", suburb = "Rosebank", lat = -26.1450, lng = 28.0372 },
                new Station { name = "Engen Hyde Park", brand = "Engen", street_address = "789 William Nicol Dr", suburb = "Hyde Park", lat = -26.1234, lng = 28.0234 },
                new Station { name = "Sasol Rivonia", brand = "Sasol", street_address = "101 Rivonia Rd", suburb = "Rivonia", lat = -26.0567, lng = 28.0678 },
                new Station { name = "Total Melrose Arch", brand = "Total", street_address = "202 Melrose Blvd", suburb = "Melrose Arch", lat = -26.1345, lng = 28.0678 },
                new Station { name = "Caltex Bryanston", brand = "Caltex", street_address = "303 Main Rd", suburb = "Bryanston", lat = -26.0456, lng = 28.0123 },
                new Station { name = "Puma Randburg", brand = "Puma", street_address = "404 Bram Fischer Dr", suburb = "Randburg", lat = -26.0987, lng = 27.9876 }
            };

            foreach (var s in stations)
            {
                context.Stations.Add(s);
            }
            context.SaveChanges();

            string userId = adminUser.Id;

            var reports = new Report[]
            {
                new Report { 
                    station_id = stations[0].id, 
                    user_id = userId, 
                    fuel_type = "Gasoline", 
                    available = true, 
                    queue_mins = 5, 
                    price = 22.45m, 
                    created_at = DateTime.UtcNow.AddHours(-1) 
                },
                new Report { 
                    station_id = stations[1].id, 
                    user_id = userId, 
                    fuel_type = "Diesel", 
                    available = false, 
                    queue_mins = 0, 
                    created_at = DateTime.UtcNow.AddHours(-2) 
                },
                new Report { 
                    station_id = stations[2].id, 
                    user_id = userId, 
                    fuel_type = "Gasoline", 
                    available = true, 
                    queue_mins = 15, 
                    price = 22.50m, 
                    created_at = DateTime.UtcNow.AddMinutes(-30) 
                }
            };

            foreach (var r in reports)
            {
                context.Reports.Add(r);
            }
            context.SaveChanges();
        }
    }
}
