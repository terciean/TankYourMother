using Microsoft.AspNetCore.Mvc;
using GasStationApp.Services;
using GasStationApp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GasStationApp.Controllers
{
    public class StationsController : Controller
    {
        private readonly IStationService _stationService;

        public StationsController(IStationService stationService)
        {
            _stationService = stationService;
        }

        public async Task<IActionResult> Index(int page = 1, double? lat = null, double? lng = null)
        {
            const int pageSize = 12;
            var stations = await _stationService.GetAllStationsWithLatestReportAsync(page, pageSize, lat, lng);
            var totalStations = await _stationService.GetStationCountAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalStations / (double)pageSize);
            ViewBag.UserLat = lat;
            ViewBag.UserLng = lng;

            return View(stations);
        }

        public async Task<IActionResult> Map()
        {
            var stations = await _stationService.GetStationsForMapAsync();
            
            // Pre-serialize JSON to avoid overhead in the view
            var json = JsonSerializer.Serialize(stations, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });
            
            ViewBag.StationsJson = json;
            return View();
        }

        public async Task<IActionResult> Details(long id, bool partial = false)
        {
            var station = await _stationService.GetStationWithLatestReportAsync(id);
            if (station == null)
            {
                return NotFound();
            }
            
            var reports = await _stationService.GetReportsForStationAsync(id);
            ViewBag.Reports = reports;
            ViewBag.IsPartial = partial;
            
            return View(station);
        }
    }
}