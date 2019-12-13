using MarkdownDeep;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace day12 {
    public static class Markdowninator {
        public const string storageName = "markdowninator";

        /// <summary>
        /// Learn mode: Get original markdown from github and return HTML
        /// Cheat mode: Use the X-Cheat-Mode header and just get the HTML content from Github directly
        /// </summary>
        [FunctionName(nameof(GetHtml))]
        public static async Task<HttpResponseMessage> GetHtml(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "repos/{user}/{repo}")] HttpRequest req,
            [Table(storageName)] CloudTable table,
            string user, string repo) {

            var response = new HttpResponseMessage();

            if (user == null || repo == null) {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Content = new StringContent("Please pass a name on the query string or in the request body");
            } else {
                var githubUrl = $"https://api.github.com/repos/{user}/{repo}/readme";
                var html = (req.Headers.TryGetValue("X-Cheat-Mode", out StringValues cheatmode )) ?
                    (await RequestContent(new Uri(githubUrl), true)).content :
                    await GetContent(table, user, githubUrl);
                response.StatusCode = HttpStatusCode.OK;
                response.Content = new StringContent(html);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            }

            return response;
        }

        /// <summary>
        /// Learn mode: Get markdown content, check cache for html content, convert and update if missing or out of date
        /// </summary>
        private static async Task<string> GetContent(CloudTable table, string user, string githubUrl) {
            var partitionKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(user));
            var rowKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(githubUrl));
            var htmlContent = string.Empty;
            var etag = string.Empty;

            var cacheResult = await table.ExecuteAsync(
                TableOperation.Retrieve<CacheEntity>(partitionKey, rowKey, new List<string> { "Content", "GithubEtag" })
            );

            if (cacheResult.HttpStatusCode == 200) {
                var cache = (CacheEntity)(cacheResult.Result);
                htmlContent = cache.Content;
                etag = cache.GithubEtag;
            }

            var response = await RequestContent(new Uri(githubUrl));

            if (response.etag != etag) {
                htmlContent = (new Markdown() { ExtraMode = true, SafeMode = true }).Transform(response.content);
                await table.ExecuteAsync(
                    TableOperation.InsertOrMerge(new CacheEntity {
                        PartitionKey = partitionKey,
                        RowKey = rowKey,
                        Content = htmlContent,
                        GithubEtag = response.etag
                    })
                );
            }

            return htmlContent;
        }

        /// <summary>
        /// Get content from Github. If cheatMode is true then HTML content will be pulled from Github.
        /// </summary>
        private static async Task<(string content,string etag)> RequestContent(Uri githubUri, bool cheatMode = false) {
            using (var client = HttpClientFactory.Create()) {
                using (var request = new HttpRequestMessage()) {
                    request.Method = HttpMethod.Get;
                    request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Markdowninator", "1.0"));
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(
                        cheatMode ? "application/vnd.github.v3.html" : "application/json"
                    ));
                    request.RequestUri = githubUri;
                    HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode) { return (string.Empty, string.Empty); }

                    var etag = response.Headers.ETag.ToString() ?? string.Empty;
                    var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    if (cheatMode) { return (result, etag); }

                    var resultObject = JObject.Parse(result);
                    var content = resultObject["content"]?.ToString() ?? string.Empty;
                    var markdown =  Encoding.UTF8.GetString(Convert.FromBase64String(content));

                    return (markdown, etag);
                }
            }
        }        
    }
}
