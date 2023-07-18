using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using DataModels;
using Microsoft.Extensions.Configuration;
using Container = Microsoft.Azure.Cosmos.Container;
using System.Configuration;
using Azure.Storage.Blobs;

namespace VenueAzureFunction
{
    public class Function1
    {
        private string CosmosDBAccountUri = Environment.GetEnvironmentVariable("CosmosDBAccountUri");
        private string CosmosDBAccountPrimaryKey = "UppkypN5jqpt7roOoasDnfcY7htbZ5hl566HfImndtXLdhW70rndiAtgL42CmztEinI5xaV0xdqaACDbYTzCaw==";
        private string CosmosDbName = Environment.GetEnvironmentVariable("CosmosDbName");
        private string CosmosDbContainerVenue = Environment.GetEnvironmentVariable("CosmosDbContainerVenue");
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> log, IConfiguration configuration)
        {
            _logger = log;
        }

        /// <summary>
        /// Common Container Client, you can also pass the configuration parameter dynamically.
        /// </summary>
        /// <returns> Container Client </returns>
        private Container ContainerClient()
        {
            CosmosClient cosmosDbClient = new CosmosClient(CosmosDBAccountUri, CosmosDBAccountPrimaryKey);
            Container containerClient = cosmosDbClient.GetContainer(CosmosDbName, CosmosDbContainerVenue);
            return containerClient;
        }

        [FunctionName("Create")]
        [OpenApiOperation(operationId: "Create", tags: new[] { "Create record operation" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(VenueViewModel), Description = "Parameters", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Venue), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "venue")] HttpRequest req, ILogger log)
        {
            VenueViewModel venueViewModel = new();
            venueViewModel.createModel = new Venue();
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                venueViewModel = JsonConvert.DeserializeObject<VenueViewModel>(requestBody);
                var container = ContainerClient();
                venueViewModel.createModel.id = Guid.NewGuid().ToString();
                venueViewModel.createModel.isActive = true;
                var attachments = venueViewModel.attachmentModels;
                ItemResponse<Venue> venueResponse = await container.CreateItemAsync<Venue>(venueViewModel.createModel, new Microsoft.Azure.Cosmos.PartitionKey(venueViewModel.createModel.id));

                if (venueResponse.StatusCode == HttpStatusCode.Created)
                {
                    await UploadFile(venueViewModel.createModel.id, attachments);
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                throw ex;
            }

            return new OkObjectResult(venueViewModel.createModel);
        }

        [FunctionName("put")]
        [OpenApiOperation(operationId: "put", tags: new[] { "Update Record operation" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(VenueViewModel), Description = "Parameters", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Venue), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "venue")] HttpRequest req, ILogger log)
        {
            VenueViewModel venueViewModel = new();
            venueViewModel.createModel = new();
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                venueViewModel = JsonConvert.DeserializeObject<VenueViewModel>(requestBody);
                var container = ContainerClient();
                ItemResponse<Venue> res = await container.ReadItemAsync<Venue>(venueViewModel.createModel.id, new Microsoft.Azure.Cosmos.PartitionKey(venueViewModel.createModel.id));
                //Get Existing Item
                var venueItem = res.Resource;
                if (venueItem != null && venueItem.id != Guid.Empty.ToString())
                {
                    //Replace existing item values with new values
                    venueItem.name = venueViewModel.createModel.name;
                    venueItem.phone = venueViewModel.createModel.phone;
                    venueItem.url = venueViewModel.createModel.url;
                    venueItem.address = venueViewModel.createModel.address;
                   
                    var attachments = venueViewModel.attachmentModels;

                    if (venueViewModel.createModel.photos != null)
                    {
                        venueItem.photos = new List<string>();
                        for (int i = 0; i < venueViewModel.createModel.photos.Count; i++)
                        {
                            venueItem.photos.Add(System.IO.Path.GetFileName(venueViewModel.createModel.photos[i]));
                        }

                    }
                    venueItem.isActive = venueViewModel.createModel.isActive;
                    var updateRes = await container.ReplaceItemAsync(venueItem, venueViewModel.createModel.id, new Microsoft.Azure.Cosmos.PartitionKey(venueViewModel.createModel.id));

                    if (updateRes.StatusCode == HttpStatusCode.OK)
                    {
                        await UploadFile(venueItem.id, attachments);
                    }
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }

            return new OkObjectResult(venueViewModel.createModel);
        }

        [FunctionName("Gets")]
        [OpenApiOperation(operationId: "Gets", tags: new[] { "get all record operation" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(List<Venue>), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Gets(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "venue")] HttpRequest req, ILogger log)
        {
            List<Venue> venues = new List<Venue>();

            //        var data = ConfigurationManager.AppSettings["Values"];
            //        var data1 = ConfigurationManager.AppSettings["CosmosDB_DBName"];

            //        var config = new ConfigurationBuilder()
            //.SetBasePath(AppContext.BaseDirectory)
            //.AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
            //.AddEnvironmentVariables()
            //.Build();


            try
            {
                var container = ContainerClient();
                var sqlQuery = "SELECT * FROM Venue as v WHERE v.isActive = true";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Venue> queryResultSetIterator = container.GetItemQueryIterator<Venue>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Venue> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Venue venue in currentResultSet)
                    {
                        venues.Add(venue);
                    }
                }

                if (venues.Count > 0)
                {
                    foreach (var item in venues)
                    {
                        if (item.photos == null)
                        {
                            continue;
                        }

                        for (int i = 0; i < item.photos.Count; i++)
                        {
                            item.photos[i] = $"{Environment.GetEnvironmentVariable("VenuesImageContainer")}/{item.id}/{item.photos[i]}";
                        }
                    }

                    return new OkObjectResult(venues);
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }

        }

        [FunctionName("GetById")]
        [OpenApiOperation(operationId: "GetById", tags: new[] { "get record by id operation" })]
        [OpenApiParameter(name: "Id", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Id** parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Venue), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> GetById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "venue/{id}")] HttpRequest req, ILogger log)
        {
            try
            {
                string id = req.Query["id"];
                var container = ContainerClient();
                ItemResponse<Venue> response = await container.ReadItemAsync<Venue>(id, new Microsoft.Azure.Cosmos.PartitionKey(id));
                if (response != null && response.Resource != null && response.Resource.id != Guid.Empty.ToString())
                {
                    if (response.Resource.photos != null)
                    {
                        for (int i = 0; i < response.Resource.photos.Count; i++)
                        {
                            response.Resource.photos[i] = $"{Environment.GetEnvironmentVariable("VenuesImageContainer")}/{response.Resource.id}/{response.Resource.photos[i]}";
                        }
                    }

                    return new OkObjectResult(response.Resource);
                }
                else
                {
                    return new NotFoundResult();
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }
        }

        [FunctionName("Delete")]
        [OpenApiParameter(name: "Id", In = ParameterLocation.Query, Required = true, Type = typeof(string), Description = "The **Id** parameter")]
        [OpenApiOperation(operationId: "Delete", tags: new[] { "Delete record operation" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Venue), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "venue/{id}")] HttpRequest req, ILogger log)
        {
            try
            {
                string id = req.Query["id"];
                var container = ContainerClient();
                Venue venueData = new();
                try
                {
                    //Not doing hard delete
                    //var response = await container.DeleteItemAsync<Venue>("id", new Microsoft.Azure.Cosmos.PartitionKey(id));

                    ItemResponse<Venue> res = await container.ReadItemAsync<Venue>(id, new Microsoft.Azure.Cosmos.PartitionKey(id));
                    //Get Existing Item
                    var venueItem = res.Resource;
                    if (venueItem != null && venueItem.id != Guid.Empty.ToString())
                    {
                        venueItem.isActive = false;
                        var updateRes = await container.ReplaceItemAsync(venueItem, id, new Microsoft.Azure.Cosmos.PartitionKey(id));
                    }
                    else
                    {
                        return new NotFoundResult();
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex.ToString());
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                throw ex;
            }

            return new OkResult();
        }

        
        private async Task UploadFile(string id, List<AttachmentModel> Attachments)
        {
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            foreach (var item in Attachments)
            {

                Stream myBlob = new MemoryStream(item.Content);
                var blobClient = new BlobContainerClient(Connection, containerName);
                var blob = blobClient.GetBlobClient($"venues/{id}/{item.FileName}");
                await blob.UploadAsync(myBlob, overwrite: true);
            }
        }
    }
}

