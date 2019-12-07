using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace day06
{
    public static class DurableReminder
    {
        [FunctionName(nameof(SetDurableReminder))]
        public static async Task<HttpResponseMessage> SetDurableReminder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequestMessage req,
            [OrchestrationClient]DurableOrchestrationClient starter) {

            string requestBody = await req.Content.ReadAsStringAsync();
            if (bool.Parse(Environment.GetEnvironmentVariable("Validate"))) {
                var validateError = Validate(req.Headers.Authorization, requestBody);
                if (validateError != null) { return new HttpResponseMessage(HttpStatusCode.OK); }
            }

            string instanceId = await starter.StartNewAsync(nameof(DurableReminderOrchestrator), requestBody);
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK) {
                Content = new StringContent(JsonConvert.SerializeObject(
                    new { type = "message", text = $"Received message." }
                ))};
            return response;
        }

        [FunctionName(nameof(DurableReminderOrchestrator))]
        public static async Task DurableReminderOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context) {
            var requestBody = context.GetInput<string>();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string text = ((string)data?.text).Replace("<at>alpha-scrudy-webhook</at>", string.Empty).Trim();
            var luisResponseBody = await context.CallActivityAsync<string>(nameof(GetLuisResponse), text);


            dynamic luis = JsonConvert.DeserializeObject(luisResponseBody);
            var Entities = luis?.prediction?.entities;
            var subject = Entities["Calendar.Subject"][0].Value.ToString();
            //var date = Entities["Calendar.StartDate"][0].Value.ToString();
            //var time = Entities["Calendar.StartTime"][0].Value.ToString();
            DateTime datetime = DateTime.Parse(Entities["datetimeV2"][0].values[0].timex.ToString());
            datetime = new DateTime(datetime.Ticks, DateTimeKind.Utc);
            var response = $"Reminder for {subject} on {datetime.ToLocalTime().ToString()}";
            await context.CallActivityAsync<Task>(nameof(SendReminder), response);
            var reminderSeconds = (datetime - context.CurrentUtcDateTime).TotalSeconds;
            var reminder = context.CurrentUtcDateTime.AddSeconds(reminderSeconds);
            await context.CreateTimer(reminder, CancellationToken.None);
            await context.CallActivityAsync<Task>(nameof(SendReminder), subject);
        }

        [FunctionName(nameof(GetLuisResponse))]
        public static async Task<string> GetLuisResponse([ActivityTrigger] string message) {
            var result = "";
            var client = HttpClientFactory.Create();
      
            using (var request = new HttpRequestMessage()) {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri($"{Environment.GetEnvironmentVariable("LUIS_URL")}{message}");
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return result;
        }

        [FunctionName(nameof(SendReminder))]
        public static async Task SendReminder([ActivityTrigger] string message, ILogger log) {            
            var client = HttpClientFactory.Create();
      
            using (var request = new HttpRequestMessage()) {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(Environment.GetEnvironmentVariable("TEAMS_WEBHOOK"));
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(
                        new MessageCard {
                            Summary = "Reminder Service",
                            Sections = new MessageSection[] {
                                new MessageSection {
                                    ActivityTitle = "Reminder Service Alert",
                                    ActivitySubtitle = "Reminder",
                                    Facts = new SectionFact[] {
                                        new SectionFact {
                                            Name = "Do this:",
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

        private static string Validate(AuthenticationHeaderValue authenticationHeaderValue, string messageContent) {
            if (authenticationHeaderValue == null) {
                return "Authentication header not present on request.";
            }

            if (!string.Equals("HMAC", authenticationHeaderValue.Scheme)) {
                return "Incorrect authorization header scheme.";
            }

            if (string.IsNullOrEmpty(messageContent)) {
                return "Unable to validate authentication header for messages with empty body.";
            }

            try {
                string providedHmacValue = authenticationHeaderValue.Parameter;
                string calculatedHmacValue = null;
                byte[] serializedPayloadBytes = Encoding.UTF8.GetBytes(messageContent);

                byte[] keyBytes = Convert.FromBase64String(Environment.GetEnvironmentVariable("TEAMS_AUTH_KEY"));
                using (HMACSHA256 hmacSHA256 = new HMACSHA256(keyBytes)) {
                    byte[] hashBytes = hmacSHA256.ComputeHash(serializedPayloadBytes);
                    calculatedHmacValue = Convert.ToBase64String(hashBytes);
                }

                if (string.Equals(providedHmacValue, calculatedHmacValue)) {
                    return null;
                }

                return string.Format(
                    "AuthHeaderValueMismatch. Expected:'{0}' Provided:'{1}'",
                    calculatedHmacValue,
                    providedHmacValue);
            } catch (Exception ex) {
                Trace.TraceError("Exception occcured while verifying HMAC on the incoming request. Exception: {0}", ex);
                return "Exception thrown while verifying MAC on incoming request.";
            }
        }

    }
}