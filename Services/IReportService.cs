using GasStationApp.Models;

namespace GasStationApp.Services
{
    public interface IReportService
    {
        Task<Report> AddReportAsync(Report report);
        Task<Report?> GetLatestReportForStationAsync(long stationId);
        Task<List<Report>> GetReportsForStationAsync(long stationId);
        Task<Report?> GetReportByIdAsync(long reportId);
        Task VoteAsync(long reportId, string userId, bool isUpvote);
    }
}