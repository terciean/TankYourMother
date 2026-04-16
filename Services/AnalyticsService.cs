using GasStationApp.Data;
using Microsoft.EntityFrameworkCore;

namespace GasStationApp.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetSystemStatsAsync()
        {
            var totalStations = await _context.Stations.CountAsync();
            var totalReports = await _context.Reports.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var hiddenReports = await _context.Reports.CountAsync(r => r.is_hidden);

            // Stats for charts
            var reportsByDay = await _context.Reports
                .GroupBy(r => r.created_at.Date)
                .OrderByDescending(g => g.Key)
                .Take(7)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToListAsync();

            var reportsByBrand = await _context.Stations
                .GroupBy(s => s.brand)
                .Select(g => new { Brand = g.Key ?? "Unknown", Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            var topContributors = await _context.Users
                .OrderByDescending(u => u.Karma)
                .Take(5)
                .Select(u => new { u.UserName, u.Karma })
                .ToListAsync();

            var latestReports = await _context.Reports
                .Include(r => r.Station)
                .OrderByDescending(r => r.created_at)
                .Take(10)
                .Select(r => new {
                    StationName = r.Station != null ? r.Station.name : "Unknown",
                    r.available,
                    r.created_at,
                    r.upvotes,
                    r.downvotes
                })
                .ToListAsync();

            return new
            {
                TotalStations = totalStations,
                TotalReports = totalReports,
                TotalUsers = totalUsers,
                ReportsByDay = reportsByDay,
                ReportsByBrand = reportsByBrand,
                TopContributors = topContributors,
                LatestReports = latestReports
            };
        }
    }
}
