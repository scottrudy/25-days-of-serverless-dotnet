using System;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace day02
{
    public static class Reminder
    {
        [FunctionName(nameof(ReminderCronStart))]
        public static async Task ReminderCronStart(
            [TimerTrigger("0 0 8 13 12 *")]TimerInfo myTimer,
            [OrchestrationClient]DurableOrchestrationClient starter) {

            string instanceId = await starter.StartNewAsync(nameof(ReminderOrchestration), null);
        }        

        [FunctionName(nameof(ReminderOrchestration))]
        public static async Task ReminderOrchestration(
            [OrchestrationTrigger] DurableOrchestrationContext context) {

            await context.CallActivityAsync(nameof(SendReminder), "Start the coffee, set out 4 cups.");
            var reminder = context.CurrentUtcDateTime.AddMinutes(25);
            await context.CreateTimer(reminder, CancellationToken.None);

            await context.CallActivityAsync(nameof(SendReminder), "Pour 2 cups.");
            reminder = context.CurrentUtcDateTime.AddMinutes(5);
            await context.CreateTimer(reminder, CancellationToken.None);

            await context.CallActivityAsync(nameof(SendReminder), "Light the candles.");
            reminder = context.CurrentUtcDateTime.AddMinutes(5);
            await context.CreateTimer(reminder, CancellationToken.None);

            await context.CallActivityAsync(nameof(SendReminder), "Deliver the coffee to Mom and Dad.");
            reminder = context.CurrentUtcDateTime.AddMinutes(4);
            await context.CreateTimer(reminder, CancellationToken.None);

            await context.CallActivityAsync(nameof(SendReminder), "Return to kitchen, fill two more cups.");
            reminder = context.CurrentUtcDateTime.AddMinutes(1);
            await context.CreateTimer(reminder, CancellationToken.None);

            await context.CallActivityAsync(nameof(SendReminder), "Relight the candles.");
            reminder = context.CurrentUtcDateTime.AddMinutes(5);
            await context.CreateTimer(reminder, CancellationToken.None);

            await context.CallActivityAsync(nameof(SendReminder), "Deliver the coffee to Sister and Brother.");
            reminder = context.CurrentUtcDateTime.AddMinutes(4);
            await context.CreateTimer(reminder, CancellationToken.None);

            await context.CallActivityAsync(nameof(SendReminder), "Return to kitchen, take a break!");
        }

        [FunctionName(nameof(SendReminder))]
        public static void SendReminder([ActivityTrigger] string message, ILogger log) {
            var smtpServer = Environment.GetEnvironmentVariable("SmtpServer");
            if (string.IsNullOrWhiteSpace(smtpServer)) {
                log.LogInformation(message);
                return;
            }

            var client = new SmtpClient(smtpServer);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(
                Environment.GetEnvironmentVariable("SmtpUser"), Environment.GetEnvironmentVariable("SmtpPassword"));
            var mailMessage = new MailMessage {
                From = new MailAddress(Environment.GetEnvironmentVariable("SmtpFrom")),
                Body = message,
                Subject = "Reminder",
            };
            mailMessage.To.Add(Environment.GetEnvironmentVariable("SmtpTo"));
            client.Send(mailMessage);
        }
    }
}