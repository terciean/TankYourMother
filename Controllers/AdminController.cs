using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GasStationApp.Services;

namespace GasStationApp.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AdminController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public async Task<IActionResult> Analytics()
        {
            var stats = await _analyticsService.GetSystemStatsAsync();
            return View(stats);
        }
    }
}
