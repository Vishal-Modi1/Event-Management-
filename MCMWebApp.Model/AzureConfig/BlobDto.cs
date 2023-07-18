namespace MCMWebApp.Model.AzureConfig
{
    public class BlobDto
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
        /// Gets or sets a value of size.
        /// </summary>
        /// <value>File size.</value>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets a value of uri.
        /// </summary>
        /// <value>File uri.</value>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets a value of folder name.
        /// </summary>
        /// <value>Folder name.</value>
        public string FolderName { get; set; }

        /// <summary>
        /// Gets or sets a value of content type.
        /// </summary>
        /// <value>Content type of file.</value>
        public string ContentType { get; set; }
    }
}
