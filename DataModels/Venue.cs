using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace DataModels
{
    public class Venue 
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [Required]
        [MaxLength(100)]
        public string name { get; set; }

        [Required(ErrorMessage = "Contact no. is required")]
        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Please enter valid phone no.")]
        public string phone { get; set; }

        [Required]
        [Url]
        public string url { get; set; }

        [Required]
        public string address { get; set; }
        public List<string> photos { get; set; }
        public bool isActive { get; set; }
    }
}