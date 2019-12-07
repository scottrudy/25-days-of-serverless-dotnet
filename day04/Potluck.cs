using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Linq;

namespace day04 {
    public static class Potluck     {
        [FunctionName(nameof(PotluckCreateRead))]
        public static async Task<IActionResult> PotluckCreateRead(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "potluck/{potluckId}/dishes")] HttpRequest req, string potluckId) {
            
            var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference("potluck");
            await table.CreateIfNotExistsAsync();

            if (req.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase)) {
                return new OkObjectResult(await GetDishes(table, potluckId));
            }

            if (!req.Method.Equals("POST", StringComparison.InvariantCultureIgnoreCase)) {
                return new StatusCodeResult(405);
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dish = JsonConvert.DeserializeObject<Dish>(requestBody);
            if (string.IsNullOrWhiteSpace(dish?.Email) || string.IsNullOrWhiteSpace(dish?.Description)) {
                return new BadRequestResult();
            }

            dish.PartitionKey = potluckId;
            dish.RowKey = Guid.NewGuid().ToString("N");
            var entity = (await table.ExecuteAsync(TableOperation.Insert(dish))).Result as Dish;

            return new AcceptedResult($"potluck/{potluckId}/dishes/{entity.RowKey}", new {
                Email = entity.Email,
                Description = entity.Description
            });
        }

        [FunctionName(nameof(PotluckUpdateDelete))]
        public static async Task<IActionResult> PotluckUpdateDelete(
            [HttpTrigger(AuthorizationLevel.Function, "get", "patch", "delete", Route = "potluck/{potluckId}/dishes/{dishId}")] HttpRequest req, string potluckId, string dishId) {

            var account = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
            var tableClient = account.CreateCloudTableClient();
            var table = tableClient.GetTableReference("potluck");
            
            if ((await table.ExistsAsync()) == false) {
                return new NotFoundResult();
            }
            
            var existingDish = await GetDish(table, potluckId, dishId);
            if (existingDish == null) {
                return new NotFoundResult();
            }

            if (req.Method.Equals("GET", StringComparison.InvariantCultureIgnoreCase)) {
                return new OkObjectResult(existingDish);
            }

            if (req.Method.Equals("DELETE", StringComparison.InvariantCultureIgnoreCase)) {
                await DeleteDish(table, potluckId, dishId);
                return new AcceptedResult();
            }

            if (!req.Method.Equals("PATCH", StringComparison.InvariantCultureIgnoreCase)) {
                return new StatusCodeResult(405);
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dish = JsonConvert.DeserializeObject<Dish>(requestBody);

            if (dish?.Email == null || dish?.Description == null) {
                return new BadRequestResult();
            }

            dish.PartitionKey = potluckId;
            dish.RowKey = dishId;
            dish.ETag = "*";
            var entity = (await table.ExecuteAsync(TableOperation.Merge(dish))).Result as Dish;
            
            return new AcceptedResult($"potluck/{potluckId}/dishes/{dishId}", new {
                Email = entity.Email,
                Description = entity.Description
            });
        } 

        private static async Task<IEnumerable<object>> GetDishes(CloudTable table, string potluckId) {
            TableContinuationToken token = null;
            var result = new List<Dish>();
            do {
                var segmentedDishes = await table.ExecuteQuerySegmentedAsync(new TableQuery<Dish>()
                    .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, potluckId)), token);
                result.AddRange(segmentedDishes.Results);
            } while (token != null);

            return result.Select(entity => new {
                Id = entity.RowKey,
                Email = entity.Email,
                Description = entity.Description
            });
        }

        private static async Task<object> GetDish(CloudTable table, string potluckId, string dishId) {
            var dish = (await table.ExecuteAsync(TableOperation.Retrieve<Dish>(potluckId, dishId, null))).Result as Dish;

            return dish == null ? null : new {
                Email = dish.Email,
                Description = dish.Description
            };
        }

        private static async Task DeleteDish(CloudTable table, string potluckId, string dishId) {
            await table.ExecuteAsync(TableOperation.Delete(new Dish { 
                PartitionKey = potluckId,
                RowKey = dishId,
                ETag = "*"
            }));
        }
    }
}
