using System;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.IO;

namespace day11
{
    public static class Wishmaster {
        public const string storageName = "wishmaster";

        [FunctionName(nameof(GetPageForm))]
        public static async Task<HttpResponseMessage> GetPageForm(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "index.html")] HttpRequest req,
            ExecutionContext context) {
            
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StringContent(
                await File.ReadAllTextAsync(Path.Combine(context.FunctionDirectory, "../index.html"))
            );
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }

        [FunctionName(nameof(SendPageForm))]
        public static async Task<IActionResult> SendPageForm(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/wish")] HttpRequest req,
            [Queue(storageName)] CloudQueue queue) {

            var formData = await req.ReadFormAsync();
            var wish = new Wish {
                Name = formData["Name"],
                Address = formData["Address"],
                WishType = formData["WishType"],
                Description = formData["Description"]
            };

            await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(wish)));

            return (wish.Name == null || wish.Address == null || wish.WishType == null || wish.Description == null)
                ? (ActionResult)new BadRequestObjectResult("Please fill out all fields on the form.")
                : new OkObjectResult($"Hello, {wish.Name}. Your wish has been submitted to Santa.");
        }

        [FunctionName(nameof(SaveWishAndNotify))]
        public static async Task SaveWishAndNotify(
            [QueueTrigger(storageName)] string wishItem,
            [Table(storageName)] CloudTable table,
            ILogger log) {

            var wish = JsonConvert.DeserializeObject<Wish>(wishItem);
            wish.PartitionKey = wish.WishType.ToLowerInvariant();
            wish.RowKey = Guid.NewGuid().ToString();
            await table.ExecuteAsync(TableOperation.Insert(wish));

            string message = $"{wish.Name} has requested a new {wish.WishType} to be delivered to {wish.Address}";
            var webhook = Environment.GetEnvironmentVariable("TEAMS_WEBHOOK");
            if (!string.IsNullOrWhiteSpace(webhook)) {
                await SendTeamsReminder(new Uri(webhook), message);
            }
            log.LogInformation(message);
        }

        private static async Task SendTeamsReminder(Uri webhook, string message) {
            var client = HttpClientFactory.Create();
      
            using (var request = new HttpRequestMessage()) {
                request.Method = HttpMethod.Post;
                request.RequestUri = webhook;
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new MessageCard {
                            Summary = "Notification Service",
                            Sections = new MessageSection[] {
                                new MessageSection {
                                    ActivityTitle = "Service Alert",
                                    ActivitySubtitle = "New Wish",
                                    Facts = new SectionFact[] {
                                        new SectionFact {
                                            Name = "Details: ",
                                            Value = message
                                        },
                                    }
                                }
                            }
                        }                        
                    ), Encoding.UTF8, "application/json"
                );
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                var responseResult = await response.Content.ReadAsStringAsync();
            }
        }
    }
}