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
        private string CosmosDBAccountUri = "https://cdb-accmainsite-ojjw-syddev.documents.azure.com:443/";
        private string CosmosDBAccountPrimaryKey = "UppkypN5jqpt7roOoasDnfcY7htbZ5hl566HfImndtXLdhW70rndiAtgL42CmztEinI5xaV0xdqaACDbYTzCaw==";
        private string CosmosDbName = "db_evefesven";
        private string CosmosDbContainerVenue = "new_venues";
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
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Venue), Description = "Parameters", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Venue), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "venue")] HttpRequest req, ILogger log)
        {
            Venue venueData = new();
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                venueData = JsonConvert.DeserializeObject<Venue>(requestBody);
                var container = ContainerClient();
                venueData.id = Guid.NewGuid().ToString();
                venueData.isActive = true;
                ItemResponse<Venue> venueResponse = await container.CreateItemAsync<Venue>(venueData, new Microsoft.Azure.Cosmos.PartitionKey(venueData.id));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                throw ex;
            }

            return new OkObjectResult(venueData);
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

        [FunctionName("put")]
        [OpenApiOperation(operationId: "put", tags: new[] { "Update Record operation" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(Venue), Description = "Parameters", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Venue), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Update(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "venue")] HttpRequest req, ILogger log)
        {
            Venue venueData = new();
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                venueData = JsonConvert.DeserializeObject<Venue>(requestBody);
                var container = ContainerClient();
                ItemResponse<Venue> res = await container.ReadItemAsync<Venue>(venueData.id, new Microsoft.Azure.Cosmos.PartitionKey(venueData.id));
                //Get Existing Item
                var venueItem = res.Resource;
                if (venueItem != null && venueItem.id != Guid.Empty.ToString())
                {
                    //Replace existing item values with new values
                    venueItem.name = venueData.name;
                    venueItem.phone = venueData.phone;
                    venueItem.url = venueData.url;
                    venueItem.address = venueData.address;
                    venueItem.photos = venueData.photos;
                    venueItem.isActive = venueData.isActive;
                    var updateRes = await container.ReplaceItemAsync(venueItem, venueData.id, new Microsoft.Azure.Cosmos.PartitionKey(venueData.id));
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

            return new OkObjectResult(venueData);
        }
    }
}

