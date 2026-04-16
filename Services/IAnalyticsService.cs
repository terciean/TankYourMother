using GasStationApp.Models;

namespace GasStationApp.Services
{
    public interface IAnalyticsService
    {
        Task<object> GetSystemStatsAsync();
    }
}
