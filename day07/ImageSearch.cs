using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;

namespace day07
{
    public static class ImageSearch
    {
        [FunctionName(nameof(SearchImages))]
        public static async Task<IActionResult> SearchImages([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req) {
            string text = req.Query["text"];
            if (text == null) { new BadRequestObjectResult("Please pass a value to {text} on the query string"); }

            var result = await SearchForImage(text).ConfigureAwait(false);
            return new RedirectResult(result, false);
        }

        public static async Task<string> SearchForImage(string text)
        {
            var client = HttpClientFactory.Create();
            client.DefaultRequestHeaders.TryAddWithoutValidation(
                "Authorization", $"Client-ID {Environment.GetEnvironmentVariable("UNSPLASH_ACCESS_KEY")}");
      
            using (var request = new HttpRequestMessage()) {
                HttpResponseMessage response = await client.GetAsync(
                    $"https://api.unsplash.com/photos/?query={text}&page=1&per_page=1").ConfigureAwait(false);
                var unsplashResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var results = JsonConvert.DeserializeObject<UnsplashResponse[]>(unsplashResponse);
                return results[0].Links.Download;
            }
        }        
    }
}
