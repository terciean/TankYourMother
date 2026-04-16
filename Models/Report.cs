using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GasStationApp.Models
{
    [Table("reports")]
    public class Report
    {
        [Key]
        [Column("id")]
        public long id { get; set; }

        [Column("station_id")]
        public long station_id { get; set; }

        [Column("user_id")]
        public string? user_id { get; set; }

        [Column("fuel type")]
        public string? fuel_type { get; set; }

        [Column("available")]
        public bool available { get; set; }

        [Column("queue_mins")]
        public int queue_mins { get; set; }

        [Column("notes")]
        public string? notes { get; set; }

        [Column("photo_url")]
        public string? photo_url { get; set; }

        [Column("price")]
        public decimal? price { get; set; }

        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;

        [Column("upvotes")]
        public int upvotes { get; set; } = 0;

        [Column("downvotes")]
        public int downvotes { get; set; } = 0;

        [Column("is_hidden")]
        public bool is_hidden { get; set; } = false;

        public string GetQueueStatus()
        {
            if (queue_mins <= 10) return "Low";
            if (queue_mins <= 30) return "Medium";
            return "High";
        }

        public string GetQueueColor()
        {
            if (queue_mins <= 10) return "success";
            if (queue_mins <= 30) return "warning";
            return "danger";
        }

        [NotMapped]
        public ApplicationUser? Author { get; set; }

        // Navigation property
        public Station? Station { get; set; }
    }
}