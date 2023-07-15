using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class AppSettingConfig
    {
        public string AccountUri { get; set; }
        public string AccountPrimaryKey { get; set; }
        public string DBName { get; set; }
        public string DBContainerVenue { get; set; }
    }
}
