using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace GasStationApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Column("karma")]
        public int Karma { get; set; } = 0;

        public string GetBadge()
        {
            if (Karma >= 100) return "Expert Reporter";
            if (Karma >= 50) return "Trusted Scout";
            if (Karma >= 10) return "Frequent Helper";
            return "Newcomer";
        }

        public string GetBadgeColor()
        {
            if (Karma >= 100) return "danger"; // Red for high tier
            if (Karma >= 50) return "warning"; // Gold/Orange
            if (Karma >= 10) return "success"; // Green
            return "secondary"; // Grey
        }
    }
}