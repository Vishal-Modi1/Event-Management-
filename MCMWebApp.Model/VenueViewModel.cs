using MCMWebApp.Model.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMWebApp.Model
{
    public class VenueViewModel
    {
        public Venue createModel { get; set; }
        public List<AttachmentModel> attachmentModels { get; set; }
    }
}
