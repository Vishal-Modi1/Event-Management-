using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos;
using Container = Microsoft.Azure.Cosmos.Container;
using MCMWebApp.Model.DataModel;
using Azure.Storage.Blobs;
using DataModels;

namespace EventAzureFunction
{
    public class Function1
    {
        private string CosmosDBAccountUri = "https://cdb-accmainsite-ojjw-syddev.documents.azure.com:443/";
        private string CosmosDBAccountPrimaryKey = "UppkypN5jqpt7roOoasDnfcY7htbZ5hl566HfImndtXLdhW70rndiAtgL42CmztEinI5xaV0xdqaACDbYTzCaw==";
        private string CosmosDbName = "db_evefesven";
        private string CosmosDbContainerEvent = "new_events";
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
            Container containerClient = cosmosDbClient.GetContainer(CosmosDbName, CosmosDbContainerEvent);
            return containerClient;
        }

        [FunctionName("Create")]
        [OpenApiOperation(operationId: "Create", tags: new[] { "Create record operation" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(EventViewModel), Description = "Parameters", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Event), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "event")] HttpRequest req, ILogger log)
        {
            EventViewModel eventData = new();
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                eventData = JsonConvert.DeserializeObject<EventViewModel>(requestBody);
                var container = ContainerClient();
                eventData.createModel.id = Guid.NewGuid().ToString();
                eventData.createModel.isActive = true;
                var attachments = eventData.attachmentModels;
                ItemResponse<Event> eventResponse = await container.CreateItemAsync<Event>(eventData.createModel, new Microsoft.Azure.Cosmos.PartitionKey(eventData.createModel.id));

                if (eventResponse.StatusCode == HttpStatusCode.Created)
                {
                    await UploadFile(eventData.createModel.id, attachments);
                }

            }
            catch (Exception ex)
            {
                log.LogError(ex.Message, ex);
                throw ex;
            }

            return new OkObjectResult(eventData);
        }

        [FunctionName("put")]
        [OpenApiOperation(operationId: "put", tags: new[] { "Update Record operation" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(EventViewModel), Description = "Parameters", Required = true)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Event), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Update(
      [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "event")] HttpRequest req, ILogger log)
        {
            EventViewModel eventViewModelData = new();
            eventViewModelData.createModel = new Event();
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                eventViewModelData = JsonConvert.DeserializeObject<EventViewModel>(requestBody);

                var eventData = eventViewModelData.createModel;
                var container = ContainerClient();

                var attachments = eventViewModelData.attachmentModels;
                ItemResponse<Event> res = await container.ReadItemAsync<Event>(eventData.id, new Microsoft.Azure.Cosmos.PartitionKey(eventData.id));
                //Get Existing Item
                var eventItem = res.Resource;
                if (eventItem != null && eventItem.id != Guid.Empty.ToString())
                {
                    //Replace existing item values with new values
                    eventItem.name = eventData.name;
                    eventItem.date = eventData.date;
                    eventItem.timeopen = eventData.timeopen;
                    eventItem.timeclose = eventData.timeclose;
                    eventItem.description = eventData.description;

                    if (eventData.photos != null)
                    {
                        eventItem.photos = new List<string>();
                        for (int i = 0; i < eventData.photos.Count; i++)
                        {
                            eventItem.photos.Add(System.IO.Path.GetFileName(eventData.photos[i]));
                        }

                    }

                    eventItem.type = eventData.type;
                    eventItem.state = eventData.state;
                    eventItem.genere = eventData.genere;
                    eventItem.venueid = eventData.venueid;
                    eventItem.isActive = eventData.isActive;
                    eventItem.OrgType = eventData.OrgType;
                    eventItem.Environment = eventData.Environment;
                    eventItem.Reason = eventData.Reason;
                    var updateRes = await container.ReplaceItemAsync(eventItem, eventData.id, new Microsoft.Azure.Cosmos.PartitionKey(eventData.id));


                    if (updateRes.StatusCode == HttpStatusCode.OK)
                    {
                        await UploadFile(eventItem.id, attachments);
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

            return new OkObjectResult(eventViewModelData.createModel);
        }



        [FunctionName("Gets")]
        [OpenApiOperation(operationId: "Gets", tags: new[] { "get all record operation" })]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(List<Event>), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Gets(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event")] HttpRequest req, ILogger log)
        {
            List<Event> events = new List<Event>();
            try
            {
                var container = ContainerClient();
                var sqlQuery = "SELECT * FROM Event as v WHERE v.isActive = true";
                QueryDefinition queryDefinition = new QueryDefinition(sqlQuery);
                FeedIterator<Event> queryResultSetIterator = container.GetItemQueryIterator<Event>(queryDefinition);

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Event> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Event @event in currentResultSet)
                    {
                        events.Add(@event);
                    }
                }

                if (events.Count > 0)
                {
                    foreach (var item in events)
                    {
                        if (item.photos == null)
                        {
                            continue;
                        }

                        for (int i = 0; i < item.photos.Count; i++)
                        {
                            item.photos[i] = $"https://samediaojjwsyddev.blob.core.windows.net/imagescontainer/events/{item.id}/{item.photos[i]}";
                        }
                    }

                    return new OkObjectResult(events);
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
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Event), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> GetById([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "event/{id}")] HttpRequest req, ILogger log)
        {
            try
            {
                string id = req.Query["id"];
                var container = ContainerClient();
                ItemResponse<Event> response = await container.ReadItemAsync<Event>(id, new Microsoft.Azure.Cosmos.PartitionKey(id));
                if (response != null && response.Resource != null && response.Resource.id != Guid.Empty.ToString())
                {
                    if (response.Resource.photos != null)
                    {
                        for (int i = 0; i < response.Resource.photos.Count; i++)
                        {
                            response.Resource.photos[i] = $"https://samediaojjwsyddev.blob.core.windows.net/imagescontainer/events/{response.Resource.id}/{response.Resource.photos[i]}";
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
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "text/plain", bodyType: typeof(Event), Description = "Returns a 200 response with text")]
        public async Task<IActionResult> Delete(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "event/{id}")] HttpRequest req, ILogger log)
        {
            try
            {
                string id = req.Query["id"];
                var container = ContainerClient();
                Event eventData = new();
                try
                {
                    //Not doing hard delete
                    //var response = await container.DeleteItemAsync<Event>("id", new Microsoft.Azure.Cosmos.PartitionKey(id));

                    ItemResponse<Event> res = await container.ReadItemAsync<Event>(id, new Microsoft.Azure.Cosmos.PartitionKey(id));
                    //Get Existing Item
                    var eventItem = res.Resource;
                    if (eventItem != null && eventItem.id != Guid.Empty.ToString())
                    {
                        eventItem.isActive = false;
                        var updateRes = await container.ReplaceItemAsync(eventItem, id, new Microsoft.Azure.Cosmos.PartitionKey(id));
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


        [FunctionName("FileUpload")]
        public static async Task<IActionResult> Run(
             [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req, ILogger log)
        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var eventData = JsonConvert.DeserializeObject<ImagesModel>(requestBody);

            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            foreach (var item in eventData.Attachments)
            {
                Stream myBlob = new MemoryStream(item.Content);
                var blobClient = new BlobContainerClient(Connection, containerName);
                var blob = blobClient.GetBlobClient($"{eventData.Id.ToString()}/{item.FileName}");
                await blob.UploadAsync(myBlob);
            }

            return new OkObjectResult("file uploaded successfylly");
        }

        private async Task UploadFile(string id, List<AttachmentModel> Attachments)
        {
            string Connection = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            string containerName = Environment.GetEnvironmentVariable("ContainerName");

            foreach (var item in Attachments)
            {

                Stream myBlob = new MemoryStream(item.Content);
                var blobClient = new BlobContainerClient(Connection, containerName);
                var blob = blobClient.GetBlobClient($"events/{id}/{item.FileName}");
                await blob.UploadAsync(myBlob, overwrite: true);
            }
        }
    }
}

