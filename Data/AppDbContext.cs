using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GasStationApp.Models;

namespace GasStationApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Station> Stations { get; set; } = null!;
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<ReportVote> ReportVotes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Station mapping
            modelBuilder.Entity<Station>(entity =>
            {
                entity.ToTable("stations");
                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.name).HasColumnName("name");
                entity.Property(e => e.brand).HasColumnName("brand");
                entity.Property(e => e.street_address).HasColumnName("street_address");
                entity.Property(e => e.suburb).HasColumnName("suburb");
                entity.Property(e => e.lat).HasColumnName("lat");
                entity.Property(e => e.lng).HasColumnName("lng");
            });

            // Report mapping
            modelBuilder.Entity<Report>(entity =>
            {
                entity.ToTable("reports");
                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.station_id).HasColumnName("station_id");
                entity.Property(e => e.user_id).HasColumnName("user_id");
                entity.Property(e => e.fuel_type).HasColumnName("fuel type"); // Note the space
                entity.Property(e => e.available).HasColumnName("available");
                entity.Property(e => e.queue_mins).HasColumnName("queue_mins");
                entity.Property(e => e.notes).HasColumnName("notes");
                entity.Property(e => e.photo_url).HasColumnName("photo_url");
                entity.Property(e => e.created_at).HasColumnName("created_at");
                entity.Property(e => e.upvotes).HasColumnName("upvotes");
                entity.Property(e => e.downvotes).HasColumnName("downvotes");
                entity.Property(e => e.is_hidden).HasColumnName("is_hidden");

                entity.HasOne(r => r.Station)
                    .WithMany()
                    .HasForeignKey(r => r.station_id);

                // Optimization: Index for fetching latest report
                entity.HasIndex(r => new { r.station_id, r.created_at })
                    .HasDatabaseName("IX_reports_station_id_created_at");
            });

            // ReportVote mapping
            modelBuilder.Entity<ReportVote>(entity =>
            {
                entity.ToTable("report_votes");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ReportId).HasColumnName("report_id");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.IsUpvote).HasColumnName("is_upvote");

                entity.HasOne(rv => rv.Report)
                    .WithMany()
                    .HasForeignKey(rv => rv.ReportId);
            });
        }
    }
}