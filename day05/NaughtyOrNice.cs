using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using System.Text;
using System.Linq;

namespace day05 {
    public static class NaughtyOrNice {

        private static readonly string[] _sentimentLanguages = new string[] {
            "en", "ja", "zh-Hans", "zh-Hant", "fr", "it", "es", "nl", "pt", "de", "da", "fi", "el", "no", "pl", "ru", "sv", "tr"
        };

        [FunctionName(nameof(GetNaughtyOrNice))]
        public static async Task<IActionResult> GetNaughtyOrNice(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "naughtyornice")] HttpRequest req) {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var letter = JsonConvert.DeserializeObject<Letter>(requestBody);
            if (string.IsNullOrWhiteSpace(letter?.Who) || string.IsNullOrWhiteSpace(letter?.Message)) {
                return new BadRequestObjectResult("Please pass a Letter");
            }

            var client = HttpClientFactory.Create();

            var originalMessage = (Text: letter.Message, Language: await DetectTextLanguage(client, letter.Message));

            if (originalMessage.Language == null) {
                return new BadRequestObjectResult("Language could not be determined.");
            }

            var analyzeMessage = _sentimentLanguages.Contains(originalMessage.Language, StringComparer.OrdinalIgnoreCase) ?
                originalMessage : await TranslateText(client, originalMessage);

            var sentiment = await DetectSentiment(client, analyzeMessage);

            return new OkObjectResult($"{letter.Who} is on the {sentiment} list.");
        }

        private static async Task<string> DetectTextLanguage(HttpClient client, string text) {
            using (var request = new HttpRequestMessage()) {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri("https://api.cognitive.microsofttranslator.com/detect?api-version=3.0");
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(new [] { new TranslationRequest { Text = text } }), Encoding.UTF8, "application/json"
                );
                request.Headers.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("TRANSLATOR_TEXT_SUBSCRIPTION_KEY"));
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync();
                var detectResult = JsonConvert.DeserializeObject<DetectResult[]>(result).SingleOrDefault();
                return detectResult.IsTranslationSupported ? detectResult.Language : null;
            }
        }

        private static async Task<(string Text, string Language)> TranslateText(HttpClient client, (string Text, string Language) message) {
            using (var request = new HttpRequestMessage()) {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri("https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&to=en");
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(new [] { new DetectRequest { Text = message.Text } }), Encoding.UTF8, "application/json"
                );
                request.Headers.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("TRANSLATOR_TEXT_SUBSCRIPTION_KEY"));
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync();
                var translationResult = JsonConvert.DeserializeObject<TranslationResult[]>(result).SingleOrDefault()?.Translations?.SingleOrDefault();
                return (Text: translationResult.Text, Language: translationResult.To);
            }
        }

        private static async Task<string> DetectSentiment(HttpClient client, (string Text, string Language) message) {
            using (var request = new HttpRequestMessage()) {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri( Environment.GetEnvironmentVariable("TEXT_ANALYTICS_URL"));
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(new AnalysisRequest {
                        Documents = new RequestDocument[] {
                            new RequestDocument {
                            Language = message.Language,
                            Id = 1,
                            Text = message.Text
                    }}}), Encoding.UTF8, "application/json"
                );
                request.Headers.Add("Ocp-Apim-Subscription-Key", Environment.GetEnvironmentVariable("TEXT_ANALYTICS_SUBSCRIPTION_KEY"));
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                var result = await response.Content.ReadAsStringAsync();
                var detectResult = JsonConvert.DeserializeObject<AnalysisResult>(result)?.Documents?.SingleOrDefault()?.Score;
                return (detectResult >= 0.5) ? "nice" : "naughty";
            }
        }
    }
}
