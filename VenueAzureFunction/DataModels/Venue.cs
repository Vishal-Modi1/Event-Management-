using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DataModels
{
    public class Venue 
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        [MaxLength(10)]
        public string phone { get; set; }
        [Required]
        public string url { get; set; }
        [Required]
        public string address { get; set; }
        public List<string> photos { get; set; }

        public bool IsActive { get; set; }
    }
}