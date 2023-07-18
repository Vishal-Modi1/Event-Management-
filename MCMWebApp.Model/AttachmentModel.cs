using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMWebApp.Model
{
    public class AttachmentModel
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
