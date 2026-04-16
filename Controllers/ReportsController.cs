using Microsoft.AspNetCore.Mvc;
using GasStationApp.Services;
using GasStationApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;

namespace GasStationApp.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("reporting")]
        public async Task<IActionResult> Create([Bind("station_id,fuel_type,available,queue_mins,notes,photo_url")] Report report)
        {
            if (ModelState.IsValid)
            {
                // Set the current user's ID
                report.user_id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                await _reportService.AddReportAsync(report);
                return RedirectToAction("Details", "Stations", new { id = report.station_id });
            }
            return RedirectToAction("Details", "Stations", new { id = report.station_id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("reporting")]
        public async Task<IActionResult> CreateAjax(Report report)
        {
            if (!ModelState.IsValid) return BadRequest();

            try 
            {
                report.user_id = User.FindFirstValue(ClaimTypes.NameIdentifier);
                report.created_at = DateTime.UtcNow;
                
                await _reportService.AddReportAsync(report);
                return Json(new { 
                    station_id = report.station_id,
                    available = report.available,
                    fuel_type = report.fuel_type,
                    queue_mins = report.queue_mins,
                    created_at = report.created_at
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while saving your report.");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        [EnableRateLimiting("voting")]
        public async Task<IActionResult> Vote(long reportId, bool isUpvote)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            try 
            {
                await _reportService.VoteAsync(reportId, userId, isUpvote);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }

            // Get updated counts
            var report = await _reportService.GetReportByIdAsync(reportId);
            if (report == null) return NotFound();
            
            return Json(new { 
                upvotes = report.upvotes, 
                downvotes = report.downvotes,
                isHidden = report.is_hidden
            });
        }
    }
}