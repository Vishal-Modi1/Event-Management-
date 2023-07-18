using MCMWebApp.Model.AzureConfig;
using Microsoft.Extensions.Options;
using Microsoft.Azure.Storage.Blob;
using MCMWebApp.Services.Interface;
using Microsoft.Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
//using CloudStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount;
//using CloudBlobClient = Microsoft.Azure.Storage.Blob.CloudBlobClient;
//using CloudBlobContainer = Microsoft.Azure.Storage.Blob.CloudBlobContainer;

namespace MCMWebApp.Services
{
    public class AzureBlobService  : IAzureBlobService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AzureBlobService> _logger;
        const double LENGTH_RATE = 1024f;
        string StorageConnectionString = "";
        string EventImageContainerName = "imagescontainer";
        string VenueImageContainerName = "imagescontainer";
        private IConfiguration configuration;
        public AzureBlobService(IOptions<AzureStorageSettings> options, ILogger<AzureBlobService> logger, IConfiguration configuration)
        {
            _logger = logger;
            this._configuration = configuration;
            var data12= _configuration.GetValue<string>("AzureDBStorageSettings1:Desting");
            //EventImageContainerName = options.Value.EventImageContainerName;
            //VenueImageContainerName = options.Value.VenueImageContainerName;
            //configuration = _configuration;
        }

        /// <summary>
        /// Method to get blob request DTO.
        /// </summary>
        /// <param name="getBlobRequestDto">Pass blob request DTO model as parameter.</param>
        /// <returns>Return blob DTO.</returns>
        public async Task<BlobDto> GetBlobAsync(GetBlobRequestDto getBlobRequestDto)
        {
            _logger.LogDebug("Get in.");
            try
            {
                var blobUriBuilder = new BlobUriBuilder(new Uri(getBlobRequestDto.Uri));
                var container = GetBlobServiceClient().GetBlobContainerClient(blobUriBuilder.BlobContainerName);

                if (!container.Exists())
                {
                    var message = $"Blob storage container {container.Name} doesn't exists";
                    _logger.LogError(message);
                    throw new Exception(message);
                }

                var blockBlob = container.GetBlobClient(blobUriBuilder.BlobName);

                using (var memoryStream = new MemoryStream())
                {
                    var result = await blockBlob.DownloadStreamingAsync();
                    await result.Value.Content.CopyToAsync(memoryStream);

                    _logger.LogDebug("Got azure blob image from URL.");
                    _logger.LogDebug("Get out.");
                    return new BlobDto
                    {
                        Content = memoryStream.ToArray(),
                        Uri = blockBlob.Uri.AbsoluteUri,
                        Size = (int)Math.Round((result.Value.Details.ContentLength / LENGTH_RATE), 0),
                        ContentType = result.Value.Details.ContentType
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Method to save blob.
        /// </summary>
        /// <param name="uploadBlobDto">Pass upload blob DTO as parameter.</param>
        /// <param name="folderName">Pass folder name as parameter.</param>
        /// <returns>Return blob DTO.</returns>
        public async Task<BlobDto> SaveBlobAsync(UploadBlobRequestDto uploadBlobDto, ContainerEnum containerName, string folderName = null)
        {
            _logger.LogDebug("Get in.");
            try
            {
                var uploadContainerName = string.Empty;
                switch (containerName)
                {
                    case ContainerEnum.EVENT:
                        //container = EventImageContainerName;
                        uploadContainerName = EventImageContainerName;
                        break;
                    case ContainerEnum.VENUE:
                        uploadContainerName = VenueImageContainerName;
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(uploadContainerName))
                {
                    //1 ALREADY WORKING CODE
                    var blockBlob = GetContainer(uploadContainerName).GetBlobClient(folderName != null ? $"{folderName}/" : "" + uploadBlobDto.Name);
                    using (var stream = new MemoryStream(uploadBlobDto.Content))
                    {
                        await blockBlob.UploadAsync(stream, new BlobHttpHeaders { ContentType = uploadBlobDto.ContentType });
                        var result = await blockBlob.GetPropertiesAsync();

                        _logger.LogDebug("Upload image on azure blob image.");
                        _logger.LogDebug("Get out.");
                        return new BlobDto
                        {
                            Name = uploadBlobDto.Name,
                            Content = null,
                            FolderName = folderName,
                            Uri = blockBlob.Uri.AbsoluteUri,
                            Size = (int)Math.Round((result.Value.ContentLength / 1024f), 0),
                            ContentType = result.Value.ContentType
                        };
                    }

                    ////2
                    //var container = new BlobContainerClient(StorageConnectionString, uploadContainerName);
                    ////var createResponse1 = await container.CreateIfNotExistsAsync();
                    ////if (createResponse1 != null && createResponse1.GetRawResponse().Status == 201)
                    ////    await container.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

                    //var blob = container.GetBlobClient(uploadBlobDto.Name);
                    ////await blob.DeleteIfExistsAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots);
                    //using (var fileStream = new MemoryStream(uploadBlobDto.Content))
                    //{
                    //    await blob.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = uploadBlobDto.ContentType });
                    //}
                    //var result = blob.GetProperties();
                    //return new BlobDto
                    //{
                    //    Name = uploadBlobDto.Name,
                    //    Content = null,
                    //    FolderName = folderName,
                    //    Uri = container.Uri.AbsoluteUri,
                    //    Size = (int)Math.Round((result.Value.ContentLength / 1024f), 0),
                    //    ContentType = result.Value.ContentType
                    //};

                    //3
                    //AzureSasCredential credential = new AzureSasCredential("");
                    //BlobClient blobClient = new BlobClient(StorageConnectionString, "imagesblob", uploadBlobDto.Name, new BlobClientOptions());

                    //var res = await blobClient.UploadAsync(uploadBlobDto.Content.OpenReadStream(1), new BlobUploadOptions
                    //{
                    //    HttpHeaders = new BlobHttpHeaders { ContentType = uploadBlobDto.ContentType },
                    //    TransferOptions = new StorageTransferOptions
                    //    {
                    //        InitialTransferSize = 1024 * 1024,
                    //        MaximumConcurrency = 10
                    //    },
                    //    ProgressHandler = new Progress<long>((progress) =>
                    //    {
                    //        progressBar = (100.0 * progress / inputFile.Size).ToString("0");

                    //    })

                    //});
                }
                else
                {
                    _logger.LogError("Container did not found within accepted values, Recording and Avatar.");
                    throw new Exception("Container did not found within accepted values, Recording and Avatar.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Method to delete blob file.
        /// </summary>
        /// <param name="filename">Filename you want to remove. Make sure you send only the filename and not the full path.</param>
        /// <returns>Return delete blob file status.</returns>
        public async Task<bool> DeleteBlobAsync(string filename, ContainerEnum containerName)
        {
            _logger.LogDebug("Get in.");
            bool deletestatus;
            try
            {
                var container = string.Empty;
                switch (containerName)
                {
                    case ContainerEnum.EVENT:
                        container = EventImageContainerName;
                        break;
                    case ContainerEnum.VENUE:
                        container = VenueImageContainerName;
                        break;
                    default:
                        break;
                }
                //CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationSettings.Instance.AzureStorageSettings_StorageConnectionString);
                Microsoft.WindowsAzure.Storage.CloudStorageAccount cloudStorageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(EventImageContainerName);
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
                Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(container);
                var blob = cloudBlobContainer.GetBlobReference(filename);
                _logger.LogDebug("Delete image for azure blob image.");
                _logger.LogDebug("Get out.");
                deletestatus = await blob.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                deletestatus = false;
                _logger.LogError(ex.Message, ex);
            }
            return deletestatus;
        }

        /// <summary>
        /// This method will only be used for data import utility. This method is not for any other use in the application
        /// </summary>
        /// <param name="uploadBlobDto">Pass upload blob DTO as parameter.</param>
        /// <param name="folderName">Pass folder name as parameter.</param>
        /// <returns>Return blob DTO.</returns>
        public BlobDto SaveBlobForUtility(UploadBlobRequestDto uploadBlobDto, ContainerEnum containerName, string folderName = null)
        {
            _logger.LogDebug("Get in.");
            try
            {
                var container = string.Empty;
                switch (containerName)
                {
                    case ContainerEnum.EVENT:
                        container = EventImageContainerName;
                        break;
                    case ContainerEnum.VENUE:
                        container = VenueImageContainerName;
                        break;
                    default:
                        break;
                }

                if (!string.IsNullOrEmpty(container))
                {
                    var blockBlob = GetContainer(container).GetBlobClient(folderName != null ? $"{folderName}/" : "" + uploadBlobDto.Name);
                    using (var stream = new MemoryStream(uploadBlobDto.Content))
                    {
                        blockBlob.Upload(stream, new BlobHttpHeaders { ContentType = uploadBlobDto.ContentType });
                        var result = blockBlob.GetProperties();

                        _logger.LogDebug("Upload image on azure blob image.");
                        _logger.LogDebug("Get out.");
                        return new BlobDto
                        {
                            Name = uploadBlobDto.Name,
                            Content = null,
                            FolderName = folderName,
                            Uri = blockBlob.Uri.AbsoluteUri,
                            Size = (int)Math.Round((result.Value.ContentLength / 1024f), 0),
                            ContentType = result.Value.ContentType
                        };
                    }
                }
                else
                {
                    _logger.LogError("Container did not found within accepted values, Recording and Avatar.");
                    throw new Exception("Container did not found within accepted values, Recording and Avatar.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Create the container and return a container client object
        /// </summary>
        /// <returns>Azure Blob container client</returns>
        private BlobContainerClient GetContainer(string name)
        {
            _logger.LogDebug("Get in.");
            try
            {
                var blobServiceClient = GetBlobServiceClient();
                BlobContainerClient container = blobServiceClient.GetBlobContainerClient(name);
                //container.CreateIfNotExists();
                _logger.LogDebug("Get container for azure blob storage.");
                _logger.LogDebug("Get out.");
                return container;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        /// <summary>
        /// Create a BlobServiceClient object which will be used to create a container client
        /// </summary>
        /// <returns>Azure Blob service client</returns>
        private BlobServiceClient GetBlobServiceClient()
        {
            _logger.LogDebug("Get in.");
            try
            {
                return new BlobServiceClient(StorageConnectionString);
                //return new BlobServiceClient(ConfigurationSettings.Instance.AzureStorageSettings_StorageConnectionString);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }
    }
}
