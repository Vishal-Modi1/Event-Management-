using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMWebApp.Model.AzureConfig
{
    public class AzureStorageSettings
    {
        public string StorageConnectionString { get; set; }
        public string EventImageContainerName { get; set; }
        public string VenueImageContainerName { get; set; }
    }
}
