using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMWEB.Shared.DataModel
{
    public class Venue
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string url { get; set; }
        public string address { get; set; }
        public List<string> photos { get; set; }
    }
}
