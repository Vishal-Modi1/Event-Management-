namespace MCMWebApp.Model.AzureConfig
{
    /// <summary>
    ///  Class to define upload blob request.       
    /// </summary>
    public class UploadBlobRequestDto
    {
        /// <summary>
        /// Gets or sets a value of content.
        /// </summary>
        /// <value>File data in bytes.</value>
        public byte[] Content { get; set; }

        /// <summary>
        /// Gets or sets a value of name.
        /// </summary>
        /// <value>File Name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value of content type.
        /// </summary>
        /// <value>Content type of file.</value>
        public string ContentType { get; set; }
    }
}
