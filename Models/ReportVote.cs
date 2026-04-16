using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GasStationApp.Models
{
    [Table("report_votes")]
    public class ReportVote
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("report_id")]
        public long ReportId { get; set; }

        [Column("user_id")]
        public string? UserId { get; set; }

        [Column("is_upvote")]
        public bool IsUpvote { get; set; }

        // Navigation property
        public Report? Report { get; set; }
    }
}