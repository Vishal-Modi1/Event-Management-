using MCMWebApp.Model.AzureConfig;

namespace MCMWebApp.Services.Interface
{
    /// <summary>
    /// Interface for avatar service.
    /// </summary>
    public interface IAzureBlobService
    {
        /// <summary>
        /// Method to save blob.
        /// </summary>
        /// <param name="uploadBlobDto">Pass upload blob DTO as parameter.</param>
        /// <param name="folderName">Pass folder name as parameter.</param>
        /// <returns>Return blob DTO.</returns>
        Task<BlobDto> SaveBlobAsync(UploadBlobRequestDto uploadBlobDto, ContainerEnum containerName, string folderName = null);

        /// <summary>
        /// Method to get blob request DTO.
        /// </summary>
        /// <param name="getBlobRequestDto">Pass blob request DTO model as parameter.</param>
        /// <returns>Return blob DTO.</returns>
        Task<BlobDto> GetBlobAsync(GetBlobRequestDto getBlobRequestDto);

        /// <summary>
        /// Method to delete blob file.
        /// </summary>
        /// <param name="filename">Filename you want to remove. Make sure you send only the filename and not the full path.</param>
        /// <returns>Return delete blob file status.</returns>
        Task<bool> DeleteBlobAsync(string filename, ContainerEnum containerName);


        /// <summary>
        /// This method will only be used for data import utility. This method is not for any other use in the application
        /// </summary>
        /// <param name="uploadBlobDto">Pass upload blob DTO as parameter.</param>
        /// <param name="folderName">Pass folder name as parameter.</param>
        /// <returns>Return blob DTO.</returns>
        BlobDto SaveBlobForUtility(UploadBlobRequestDto uploadBlobDto, ContainerEnum containerName, string folderName = null);
    }
}
