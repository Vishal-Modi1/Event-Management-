using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCMWebApp.Model
{
    public class ImagesModel
    {
        public string Id { get; set; }

        public List<AttachmentModel> Attachments { get; set; }
    }

    public class AttachmentModel
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
