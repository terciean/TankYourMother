using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GasStationApp.Models
{
    [Table("stations")]
    public class Station
    {
        [Key]
        [Column("id")]
        public long id { get; set; }

        [Column("name")]
        public string? name { get; set; }

        [Column("brand")]
        public string? brand { get; set; }

        [Column("street_address")]
        public string? street_address { get; set; }

        [Column("suburb")]
        public string? suburb { get; set; }

        [Column("lat")]
        public double lat { get; set; }

        [Column("lng")]
        public double lng { get; set; }

        [NotMapped]
        public Report? LatestReport { get; set; }

        public string GetLogoPath()
        {
            return ResolveLogoPath(brand) + $"?v={DateTime.Now.Ticks / 10000}"; 
        }

        public static string ResolveLogoPath(string? brand)
        {
            if (string.IsNullOrEmpty(brand)) return "/images/logos/fuel.png";
            
            var b = brand.ToLower().Replace(" ", "").Replace("-", "");
            string logoName = "fuel.png";

            // Mapping to the actual filenames found in wwwroot/images/logos/
            if (b.Contains("bp")) logoName = "BP.png";
            else if (b.Contains("shell")) logoName = "shell.jpg";
            else if (b.Contains("caltex")) logoName = "caltex.png";
            else if (b.Contains("7eleven")) logoName = "7eleven.png";
            else if (b.Contains("mobil")) logoName = "mobil.png";
            else if (b.Contains("sasol")) logoName = "sasol.png";
            else if (b.Contains("engen")) logoName = "engen.png";
            else if (b.Contains("total")) logoName = "total.jpg"; 
            else if (b.Contains("puma")) logoName = "puma.png";
            else if (b.Contains("astron")) logoName = "astron.jpg";
            else if (b.Contains("viva")) logoName = "viva.png";
            else if (b.Contains("fuel")) logoName = "fuel.png";
            else if (b.Contains("freshstop")) logoName = "freshstop.png";
            else if (b.Contains("costco")) logoName = "costcowholesale.png";
            else if (b.Contains("united")) logoName = "unitedpetroleum.png";
            else if (!string.IsNullOrEmpty(b)) logoName = b + ".png";
            else logoName = "fuel.png";

            return $"/images/logos/{logoName}";
        }
    }
}