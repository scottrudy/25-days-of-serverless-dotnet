using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;
using Stream = System.IO.Stream;

namespace day10 {
    public static class DealOfTheDay {
        private const string TABLE = "TimelineParameters";
        private const string USER = "treasuretruck";
        private const string KEY = "last";
        private const int MAX_TWEETS = 11;

        [FunctionName("FindDealsOnTwitter")]
        public static async Task FindDealsOnTwitter(
            [TimerTrigger("0 */1 * * * *")] TimerInfo myTimer,
            [Table(TABLE, USER, KEY)] TimelineParameter timelineParameter,
            [Blob("web/index.html", FileAccess.Write)] Stream page,
            ILogger log) {

            var timeline = await GetTweets(MAX_TWEETS, timelineParameter?.SinceId ?? -1);

            await WriteToPage(timeline?.Select(t => t.Text).ToArray(), page);

            SetMaxSinceId(timeline, timelineParameter);
        }

        private static void SetMaxSinceId(IEnumerable<ITweet> timeline, TimelineParameter timelineParameter) {
            var sinceId = timeline?.Max(t => t?.Id) ?? -1;
            if (sinceId >= 0) {
                timelineParameter.SinceId = sinceId;
            }
        }

        private static async Task<IEnumerable<ITweet>> GetTweets(int maxTweets, long sinceId) {
            Auth.SetUserCredentials(
                Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY"),
                Environment.GetEnvironmentVariable("TWITTER_CONSUMER_SECRET"),
                Environment.GetEnvironmentVariable("TWITTER_USER_ACCESS_TOKEN"),
                Environment.GetEnvironmentVariable("TWITTER_USER_ACCESS_SECRET")
            );

            var timeline = await TimelineAsync.GetUserTimeline(
                new UserIdentifier(USER),
                new UserTimelineParameters {
                    MaximumNumberOfTweetsToRetrieve = MAX_TWEETS,
                    SinceId = -1, // TODO: sinceId,
                    IncludeEntities = false,
                    IncludeRTS = false,
                    ExcludeReplies = true,
                    TrimUser = true
                });

            return timeline;
        }

        private static async Task WriteToPage(string[] tweets, Stream page) {
            Console.WriteLine(tweets.Length);
            var pageHtml = string.Format(
@"<!DOCTYPE html>
<html>
    <head>
        <title>Get your deals</title>
    </head>
    <body>
        <H1>These are the deals!</H1>
        <ul>
            <li>{0}</li>
            <li>{1}</li>
            <li>{2}</li>
            <li>{3}</li>
            <li>{4}</li>
            <li>{5}</li>
            <li>{6}</li>
            <li>{7}</li>
            <li>{8}</li>
            <li>{9}</li>
        </ul>
    </body>
</html>", tweets[0], tweets[1], tweets[2], tweets[3], tweets[4], tweets[5], tweets[6], tweets[7], tweets[8], tweets[9]);

            using (var sw = new StreamWriter(page, new UTF8Encoding())) {
                await sw.WriteAsync(pageHtml);
                await sw.FlushAsync();
            }
        }
    }
}
