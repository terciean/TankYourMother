using Microsoft.EntityFrameworkCore;
using GasStationApp.Data;
using GasStationApp.Models;

namespace GasStationApp.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Report> AddReportAsync(Report report)
        {
            report.created_at = DateTime.UtcNow;
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<Report?> GetLatestReportForStationAsync(long stationId)
        {
            return await _context.Reports
                .Where(r => r.station_id == stationId && !r.is_hidden)
                .OrderByDescending(r => r.created_at)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Report>> GetReportsForStationAsync(long stationId)
        {
            return await _context.Reports
                .Where(r => r.station_id == stationId && !r.is_hidden)
                .OrderByDescending(r => r.created_at)
                .ToListAsync();
        }

        public async Task<Report?> GetReportByIdAsync(long reportId)
        {
            return await _context.Reports.FindAsync(reportId);
        }

        public async Task VoteAsync(long reportId, string userId, bool isUpvote)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return;

            // Security Shield: Prevent self-voting
            if (!string.IsNullOrEmpty(report.user_id) && report.user_id == userId)
            {
                throw new InvalidOperationException("You cannot vote on your own report.");
            }

            var existingVote = await _context.ReportVotes
                .FirstOrDefaultAsync(v => v.ReportId == reportId && v.UserId == userId);

            var author = report.user_id != null ? await _context.Users.FindAsync(report.user_id) : null;

            if (existingVote != null)
            {
                if (existingVote.IsUpvote == isUpvote)
                {
                    // Remove vote if clicking same button
                    _context.ReportVotes.Remove(existingVote);
                    if (isUpvote) 
                    {
                        report.upvotes--;
                        if (author != null) author.Karma = Math.Max(0, author.Karma - 1);
                    }
                    else 
                    {
                        report.downvotes--;
                        if (author != null) author.Karma++;
                    }
                }
                else
                {
                    // Change vote
                    existingVote.IsUpvote = isUpvote;
                    if (isUpvote)
                    {
                        report.upvotes++;
                        report.downvotes--;
                        if (author != null) author.Karma += 2; // -1 to +1
                    }
                    else
                    {
                        report.upvotes--;
                        report.downvotes++;
                        if (author != null) author.Karma = Math.Max(0, author.Karma - 2); // +1 to -1
                    }
                }
            }
            else
            {
                // New vote
                var vote = new ReportVote
                {
                    ReportId = reportId,
                    UserId = userId,
                    IsUpvote = isUpvote
                };
                _context.ReportVotes.Add(vote);
                if (isUpvote) 
                {
                    report.upvotes++;
                    if (author != null) author.Karma++;
                }
                else 
                {
                    report.downvotes++;
                    if (author != null) author.Karma = Math.Max(0, author.Karma - 1);
                }
            }

            // Robust Security Shield: Hiding Logic
            // Hiding occurs if a report has at least 3 downvotes AND 
            // the downvotes significantly outweigh the upvotes (e.g., 2:1 ratio)
            // or if it reaches a critical mass of 5 downvotes.
            if ((report.downvotes >= 3 && report.downvotes > (report.upvotes * 2)) || 
                report.downvotes >= 5)
            {
                report.is_hidden = true;
            }
            else
            {
                report.is_hidden = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}