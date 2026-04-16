using GasStationApp.Models;

namespace GasStationApp.Services
{
    public interface IStationService
    {
        Task<List<Station>> GetAllStationsAsync();
        Task<List<Station>> GetAllStationsWithLatestReportAsync(int page = 1, int pageSize = 12, double? userLat = null, double? userLng = null);
        Task<int> GetStationCountAsync();
        Task<object> GetStationsForMapAsync();
        Task<Station?> GetStationWithLatestReportAsync(long id);
        Task<List<Report>> GetReportsForStationAsync(long stationId);
    }
}