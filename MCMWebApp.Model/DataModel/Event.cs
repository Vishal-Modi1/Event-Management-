using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MCMWebApp.Model.DataModel
{
    public class Event
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public DateTime? date { get; set; } = DateTime.Today;

        public TimeSpan? timeopen { get; set; }
        public TimeSpan? timeclose { get; set; }

        [MaxLength(30)]
        public string description { get; set; }
        public List<string> photos { get; set; }
        public string type { get; set; }
        public string state { get; set; }
        public string genere { get; set; }
        public bool isActive { get; set; }
        public string venueid { get; set; }

    }
}