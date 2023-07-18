using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MCMWebApp.Model.DataModel
{
    public class Event
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [Required]
        [MaxLength(100)]
        public string name { get; set; }

        [Required]
        public DateTime? date { get; set; } = DateTime.Today;
        [Required]
        public TimeSpan? timeopen { get; set; }
        [Required]
        public TimeSpan? timeclose { get; set; }

        [MaxLength(500)]
        [Required]
        public string description { get; set; }

        public List<string> photos { get; set; }

        [Required]
        public string type { get; set; }

        [Required]
        public string state { get; set; }

        [Required]
        public string genere { get; set; }

        [Required(ErrorMessage = "Please select venue")]
        public string venueid { get; set; }

        public bool isActive { get; set; }

        [Required]
        [MaxLength(500)]
        public string? Environment { get; set; }

        [Required]
        [Range(0, 10, ErrorMessage = "Music rating must be between 0 to 10")]
        public int? Music { get; set; } = 0;

        [Required]
        [Range(0, 10, ErrorMessage = "Drinks rating must be between 0 to 10")]
        public int? Drinks { get; set; } = 0;

        [Required]
        [Range(0, 10, ErrorMessage = "Drinks rating must be between 0 to 10")]
        public int? Cost { get; set; } = 0;

        [Required]
        [Range(0, 10, ErrorMessage = "Position must be between 0 to 10")]
        public int? Position { get; set; } = 0;
        [Required]
        public string Reason { get; set; }
        [Required]
        public string OrgType { get; set; }

    }
}