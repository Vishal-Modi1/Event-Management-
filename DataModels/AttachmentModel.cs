namespace DataModels
{
    public class ImagesModel
    {
        public Guid Id { get; set; }

        public List<AttachmentModel> Attachments { get; set; }
    }

    public class AttachmentModel
    {
        public byte[] Content { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }


}
