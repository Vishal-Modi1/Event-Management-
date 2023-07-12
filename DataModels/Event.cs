using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModels
{
    public class Event
    {
        //public int EventID { get; set; }
        //public string Name { get; set; }
        ////public DateTime TimeOpen { get; set; }
        ////public DateTime TimeClose { get; set; }
        ////public string Description { get; set; }
        //public string Type { get; set; }

        public Guid id { get; set; }
        public string name { get; set; }
        public DateTime date { get; set; }
        public TimeSpan? timeopen { get; set; }
        public TimeSpan? timecolse { get; set; }

        [MaxLength(30)]
        public string description { get; set; }
        public List<string> photos { get; set; }
        public string type { get; set; }
        public string state { get; set; }
        public string genere { get; set; }

        [ForeignKey("id")]
        public Venue venue { get; set; }

    }
}