using Microsoft.EntityFrameworkCore;
using GasStationApp.Data;
using GasStationApp.Models;

namespace GasStationApp.Services
{
    public class StationService : IStationService
    {
        private readonly AppDbContext _context;

        public StationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Station>> GetAllStationsAsync()
        {
            return await _context.Stations.ToListAsync();
        }

        public async Task<int> GetStationCountAsync()
        {
            return await _context.Stations.CountAsync();
        }

        public async Task<List<Station>> GetAllStationsWithLatestReportAsync(int page = 1, int pageSize = 12, double? userLat = null, double? userLng = null)
        {
            // Filter out invalid coordinates (0,0) which appear in the ocean
            var query = _context.Stations
                .AsNoTracking()
                .Where(s => s.lat != 0 && s.lng != 0);

            // If user location is provided, sort by distance
            if (userLat.HasValue && userLng.HasValue)
            {
                query = query.OrderBy(s => 
                    Math.Pow(s.lat - userLat.Value, 2) + Math.Pow(s.lng - userLng.Value, 2)
                );
            }
            else
            {
                query = query.OrderBy(s => s.name);
            }

            // Apply pagination and fetch latest reports in ONE query
            var result = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new {
                    Station = s,
                    LatestReport = _context.Reports
                        .Where(r => r.station_id == s.id && !r.is_hidden)
                        .OrderByDescending(r => r.created_at)
                        .FirstOrDefault()
                })
                .ToListAsync();

            if (!result.Any()) return new List<Station>();

            // Fetch authors for these specific reports
            var reports = result.Where(r => r.LatestReport != null).Select(r => r.LatestReport!).ToList();
            var userIds = reports.Select(r => r.user_id).Distinct().ToList();
            var authors = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

            foreach (var item in result)
            {
                if (item.LatestReport != null)
                {
                    if (item.LatestReport.user_id != null && authors.TryGetValue(item.LatestReport.user_id, out var author))
                    {
                        item.LatestReport.Author = author;
                    }
                    item.Station.LatestReport = item.LatestReport;
                }
            }

            return result.Select(r => r.Station).ToList();
        }

        public async Task<object> GetStationsForMapAsync()
        {
            // 1. Fetch stations first (Fast)
            var stations = await _context.Stations
                .AsNoTracking()
                .OrderBy(s => s.id)
                .Take(1000)
                .Select(s => new {
                    s.id,
                    s.name,
                    s.brand,
                    s.lat,
                    s.lng,
                    s.street_address,
                    s.suburb
                })
                .ToListAsync();

            if (!stations.Any()) return new List<object>();

            var stationIds = stations.Select(s => s.id).ToList();

            // 2. Fetch ALL reports for these specific stations (Fast)
            var reports = await _context.Reports
                .AsNoTracking()
                .Where(r => stationIds.Contains(r.station_id) && !r.is_hidden)
                .OrderByDescending(r => r.created_at)
                .Select(r => new {
                    r.station_id,
                    r.available,
                    r.fuel_type,
                    r.queue_mins,
                    r.price,
                    r.created_at
                })
                .ToListAsync();

            // 3. Group in memory to find latest (Instant)
            var latestReportMap = reports
                .GroupBy(r => r.station_id)
                .ToDictionary(g => g.Key, g => g.First());

            // 4. Build final object
            return stations.Select(s => {
                return new {
                    s.id,
                    s.name,
                    s.brand,
                    s.lat,
                    s.lng,
                    s.street_address,
                    s.suburb,
                    logo_path = Station.ResolveLogoPath(s.brand),
                    LatestReport = latestReportMap.TryGetValue(s.id, out var report) ? report : null
                };
            }).ToList();
        }

        public async Task<Station?> GetStationWithLatestReportAsync(long id)
        {
            var result = await _context.Stations
                .Where(s => s.id == id)
                .Select(s => new
                {
                    Station = s,
                    LatestReport = _context.Reports
                        .Where(r => r.station_id == s.id && !r.is_hidden)
                        .OrderByDescending(r => r.created_at)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (result != null)
            {
                if (result.LatestReport?.user_id != null)
                {
                    result.LatestReport.Author = await _context.Users.FindAsync(result.LatestReport.user_id);
                }
                result.Station.LatestReport = result.LatestReport;
                return result.Station;
            }

            return null;
        }

        public async Task<List<Report>> GetReportsForStationAsync(long stationId)
        {
            var reports = await _context.Reports
                .Where(r => r.station_id == stationId && !r.is_hidden)
                .OrderByDescending(r => r.created_at)
                .ToListAsync();

            var userIds = reports.Select(r => r.user_id).Distinct().ToList();
            var authors = await _context.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id);

            foreach (var r in reports)
            {
                if (r.user_id != null && authors.TryGetValue(r.user_id, out var author))
                {
                    r.Author = author;
                }
            }
            return reports;
        }
    }
}